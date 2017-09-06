using ahws.Http;
using ahws.Websocket.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ahws.Websocket
{
    public class WebSocket
    {
        public static List<string> SupportedExtensions { get; set; }
        public List<string> SubProtocols { get; set; }
        public List<WebsocketExtension> Extensions { get; set; }

        private bool _isServerClient;
        private bool _sentClose;
        private Socket _s;
        private NetworkStream _ns;
        private CancellationTokenSource _ct;
        private HttpRequest _origReq;
        private HttpResponse _origRsp;
        private MemoryStream _fragStream;
        private WebSocketOpCode _fragMessageType;

        public delegate void FrameEvent(FrameEventArgs e);
        public event FrameEvent OnFrame = (e) => { };

        public delegate void Error(Exception e);
        public event Error OnError = (e) => { };

        public delegate void Log(string msg);
        public event Log OnLog = (m) => { };

        public delegate void Disconnect(WebsocketCloseCode c, string msg);
        public event Disconnect OnDisconnect = (a, b) => { };
        
        public EndPoint Ip
        {
            get
            {
                if (_s != null && _s.Connected)
                {
                    return _s.RemoteEndPoint;
                }
                return null;
            }
        }

        public HttpRequest Request
        {
            get
            {
                return _origReq;
            }
        }

        public WebSocket(HttpRequest req, HttpResponse rsp, Socket s, List<WebsocketExtension> opt)
        {
            _isServerClient = true;
            Extensions = opt ?? new List<WebsocketExtension>();

            _ct = new CancellationTokenSource();
            _origReq = req;
            _origRsp = rsp;
            _s = s;
            _ns = new NetworkStream(_s);
        }

        public void Start()
        {
            doRead();
        }

        public async Task Close(WebsocketCloseCode c = WebsocketCloseCode.Normal, string msg = "")
        {
            //Console.WriteLine(string.Format("Closing socket {0} {1}", c.ToString(), msg));

            var wsf = new WebSocketFrame();
            wsf.Flags = WebSocketFlags.FinalFragment;
            wsf.OpCode = WebSocketOpCode.ConnectionClose;

            wsf.Payload = new byte[2 + msg.Length];
            wsf.PayloadLength = (ulong)wsf.Payload.LongLength;

            var dt = BitConverter.GetBytes((UInt16)(int)c);

            wsf.Payload[0] = dt[1];
            wsf.Payload[1] = dt[0];

            if (!string.IsNullOrEmpty(msg))
            {
                var dt2 = Encoding.UTF8.GetBytes(msg);
                Buffer.BlockCopy(dt2, 0, wsf.Payload, 2, dt2.Length);
            }

            await SendFrame(wsf);
            _sentClose = true;
        }

        private void End()
        {
            _ct.Cancel();

            if (_s.Connected)
            {
                _s.Close();
            }
        }

        public async Task Pong(WebSocketFrame f)
        {
            f.OpCode = WebSocketOpCode.Pong;

            await SendFrame(f);
        }

        private async void doRead()
        {
            while (!_ct.IsCancellationRequested)
            {
                try
                {
                    byte[] b1 = await ReadExact(2); //options & len (2)
                    byte[] b2 = null; //extended length
                    byte[] b3 = null; //mask-key (4)
                    byte[] b4 = null; //payload (x+y)

                    if (b1 == null || b1.Length == 0)
                    {
                        End(); //tcp connection closed
                        continue;
                    }

                    ulong len = 0;

                    var fs = _fragStream != null; //fragment started
                    var ft = b1[0] & 0xF0; //frame type
                    var rsv = (b1[0] & 0x70) > 0; //reserve bits are set
                    var op = b1[0] & 0x0F; //opcode
                    var pl = b1[1] & 0x7F; //payload length
                    var msk = (b1[1] & 0x80) == 0x80; //masking key set
                    var cf = (op & 0x08) == 0x08; //control frame

                    if (pl == 126 || pl == 127)
                    {
                        b2 = await ReadExact((ulong)(pl == 126 ? 2 : 8));
                        b2 = b2.Reverse().ToArray();
                        len = pl == 126 ? BitConverter.ToUInt16(b2, 0) : BitConverter.ToUInt64(b2, 0);
                    }
                    else
                    {
                        len = (ulong)pl;
                    }

                    if (msk)
                    {
                        b3 = await ReadExact(4);
                    }
                    
                    b4 = await ReadExact(len);

                    var wsf = new WebSocketFrame();
                    wsf.ParsePayload((WebSocketFlags)ft, (WebSocketOpCode)op, msk, len, b3, b4);

                    //Console.WriteLine(string.Format("Got Frame {0} {1} {2} {3}", wsf.Flags.ToString(), wsf.OpCode.ToString(), wsf.PayloadLength, _sentClose));

                    if (!_sentClose)
                    {
                        if (cf)
                        {
                            if (wsf.OpCode == WebSocketOpCode.ConnectionClose) //client is closing the connection 
                            {
                                if (rsv)
                                {
                                    await Close(WebsocketCloseCode.ProtocolError, "RSV set and no extension negotiated");
                                }
                                else if(wsf.PayloadLength == 0)
                                {
                                    await Close();
                                }
                                else if(wsf.PayloadLength == 1)
                                {
                                    await Close(WebsocketCloseCode.ProtocolError, "Close frame code invalid");
                                }
                                else if (wsf.PayloadLength > 125)
                                {
                                    await Close(WebsocketCloseCode.ProtocolError, "Close frame too long");
                                }
                                else
                                {
                                    var cc = BitConverter.ToUInt16(new byte[] { wsf.Payload[1], wsf.Payload[0] }, 0);
                                    var cm = wsf.PayloadLength > 2 ? Encoding.UTF8.GetString(wsf.Payload, 2, (int)wsf.PayloadLength - 2) : null;

                                    if (cc >= 0 && cc <= 999)
                                    {
                                        await Close(WebsocketCloseCode.ProtocolError, "Close code invalid");
                                    }
                                    else
                                    {
                                        await Close();
                                    }

                                    OnDisconnect((WebsocketCloseCode)cc, cm);
                                }

                                End();
                            }
                            else if (wsf.PayloadLength > 125)
                            {
                                await Close(WebsocketCloseCode.ProtocolError, "Control frame with payload length > 125 octets");
                            }
                            else if (op >= 0xB && op <= 0xF)
                            {
                                await Close(WebsocketCloseCode.ProtocolError, "Control frame using reserved opcode");
                            }
                            else if (!wsf.Flags.HasFlag(WebSocketFlags.FinalFragment))
                            {
                                await Close(WebsocketCloseCode.ProtocolError, "Fragmented control frame");
                            }
                            else if (wsf.OpCode == WebSocketOpCode.Ping)
                            {
                                await Pong(wsf);
                            }
                        }
                        else
                        {
                            if (rsv && Extensions.Count() == 0)
                            {
                                await Close(WebsocketCloseCode.ProtocolError, "RSV set and no extension negotiated");
                            }
                            else if (op >= 3 && op <= 7)
                            {
                                await Close(WebsocketCloseCode.ProtocolError, "Data frame using reserved opcode");
                            }
                            else
                            {
                                if (!fs && !wsf.Flags.HasFlag(WebSocketFlags.FinalFragment))
                                {
                                    if (wsf.OpCode == WebSocketOpCode.BinaryFrame || wsf.OpCode == WebSocketOpCode.TextFrame)
                                    {
                                        _fragStream = new MemoryStream();
                                        _fragMessageType = wsf.OpCode;
                                        await _fragStream.WriteAsync(wsf.Payload, 0, (int)wsf.PayloadLength);
                                    }
                                    else
                                    {
                                        await Close(WebsocketCloseCode.ProtocolError, "Cant start fragmented message on continuation frame");
                                    }
                                }
                                else if (fs && !wsf.Flags.HasFlag(WebSocketFlags.FinalFragment))
                                {
                                    if (wsf.OpCode == WebSocketOpCode.ContinuationFrame)
                                    {
                                        await _fragStream.WriteAsync(wsf.Payload, 0, (int)wsf.PayloadLength);
                                    }
                                    else
                                    {
                                        await Close(WebsocketCloseCode.ProtocolError, "Received non-continuation data frame while inside fragmented message");
                                    }
                                }
                                else if (fs && wsf.Flags.HasFlag(WebSocketFlags.FinalFragment))
                                {
                                    if (wsf.OpCode == WebSocketOpCode.ContinuationFrame)
                                    {
                                        await _fragStream.WriteAsync(wsf.Payload, 0, (int)wsf.PayloadLength);
                                        wsf.Payload = _fragStream.ToArray();
                                        wsf.PayloadLength = (ulong)_fragStream.Length;
                                        wsf.OpCode = _fragMessageType;

                                        await ProcessFrame(new FrameEventArgs() { Frame = wsf, WebSocket = this });
                                    }
                                    else
                                    {
                                        await Close(WebsocketCloseCode.ProtocolError, "Received non-continuation data frame while inside fragmented message");
                                    }

                                    _fragStream.Dispose();
                                    _fragStream = null;
                                }
                                else if (!fs && wsf.OpCode == WebSocketOpCode.ContinuationFrame)
                                {
                                    await Close(WebsocketCloseCode.ProtocolError, "Received continuation data frame outside fragmented message");
                                }
                                else
                                {
                                    await ProcessFrame(new FrameEventArgs() { Frame = wsf, WebSocket = this });
                                }
                            }
                        }
                    }
                    else if (cf && wsf.OpCode == WebSocketOpCode.ConnectionClose)
                    {
                        //Close response from client after we sent close code
                        End();
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        private async Task ProcessFrame(FrameEventArgs f)
        {
            //process extensions first
            if(Extensions.Count > 0)
            {
                foreach(var ex in Extensions)
                {
                    await ex.ProcessReceivedFrame(f.Frame);
                }
            }

            if(f.Frame.OpCode == WebSocketOpCode.TextFrame)
            {
                if (!ValidateUTF8(f.Frame.Payload))
                {
                    await Close(WebsocketCloseCode.DataTypeError, "UTF-8 Encoding error");
                    return;
                }
            }

            OnFrame(f);
        }

        private bool ValidateUTF8(byte[] data)
        {
            for (var x = 0; x < data.Length; x++)
            {
                if (data[x] > 127 && data[x] < 194) // 1 byte
                {
                    return false;
                }
                else if(data[x] >= 194 && data[x] <= 223) // 2 byte
                {
                    if (x + 1 >= data.Length)
                    {
                        return false;
                    }
                    if (data[x + 1] < 128 || data[x + 1] > 191)
                    {
                        return false;
                    }
                    x += 1;
                }
                else if (data[x] >= 224 && data[x] <= 239) // 3 byte
                {
                    if (x + 2 >= data.Length)
                    {
                        return false;
                    }

                    for (var y = 1; y < 3; y++)
                    {
                        if (data[x + y] < 128 || data[x + y] > 191)
                        {
                            return false;
                        }
                    }
                    x += 2;
                }
                else if (data[x] >= 240 && data[x] <= 255) // 4 byte
                {
                    if(x + 3 >= data.Length)
                    {
                        return false;
                    }
                    for (var y = 1; y < 4; y++)
                    {
                        if (data[x + y] < 128 || data[x + y] > 191)
                        {
                            return false;
                        }
                    }
                    x += 3;
                }
            }
            
            return true;
        }

        private async Task<byte[]> ReadExact(UInt64 len)
        {
            if(len == 0)
            {
                return new byte[0];
            }

            try
            {
                var bf = new byte[len];
                int offset = 0;

                read_more:
                int rlen = 0;
                if (!_ct.IsCancellationRequested)
                {
                    rlen = await _ns.ReadAsync(bf, offset, (int)len - offset);
                }
                else
                {
                    return null;
                }

                if (offset + rlen < (int)len && rlen != 0)
                {
                    offset += rlen;
                    goto read_more;
                }

                if (rlen == 0)
                {
                    _ct.Cancel();
                    bf = null;
                }

                return bf;
            }
            catch (Exception ex)
            {
                OnError(ex);
                End();
                return null;
            }
        }

        public void SendData<T>(T data)
        {
            SendDataAsync(data).Wait();
        }

        public async Task SendDataAsync<T>(T data)
        {
            await SendFrame(await WebSocketFrame.PackData(data));
        }

        public void SendData(string data)
        {
            SendDataAsync(data).Wait();
        }

        public async Task SendDataAsync(string data)
        {
            await SendFrame(await WebSocketFrame.PackData(data));
        }

        public async Task SendFrame(WebSocketFrame wf)
        {
            try
            {
                if (wf != null && _s.Connected)
                {
                    if (_isServerClient)
                    {
                        wf.Mask = false; //Frames from the server cannot be masked;
                    }

                    //process extensions 
                    if(Extensions.Count > 0)
                    {
                        foreach(var ex in Extensions)
                        {
                            await ex.ProcessFrame(wf);
                        }
                    }

                    var fd = await wf.GetDataAsync();

                    if (fd != null && fd.Length > 0)
                    {
                        await _ns.WriteAsync(fd, 0, fd.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError(ex);

                End();
            }
        }
    }
}
