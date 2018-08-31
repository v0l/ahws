using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace v0l.ahws.Http
{
    public class HttpSocket
    {
        private HttpServer _srv { get; set; }
        private Socket _s { get; set; }
        private Stream _ns { get; set; }
        private bool _leaveOpen { get; set; }
        private CancellationTokenSource _ct { get; set; }
        private Task _readTask {get;set;}

        public Socket Socket => _s;

        public delegate Task HandleResponse(HttpContent ctx);
        public event HandleResponse OnResponse = (c) => { return null; };

        public HttpSocket(HttpRequest req, bool leaveOpen = true)
        {
            _s = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _s.Connect(req.RequestUri.Host, req.RequestUri.Port);

            _leaveOpen = leaveOpen;
            _ct = new CancellationTokenSource();

            if (req.RequestUri.Scheme == "https")
            {
                var ss = new SslStream(new NetworkStream(_s, false));
                ss.AuthenticateAsClient(req.RequestUri.Host);
                _ns = ss;
            }
            else
            {
                _ns = new NetworkStream(_s, false);
            }

            _readTask = readResponseTask();
        }

        public HttpSocket(HttpServer srv, Socket s, bool leaveOpen = true)
        {
            _srv = srv;
            _s = s;
            _leaveOpen = leaveOpen;

            _ct = new CancellationTokenSource();
            _ns = new NetworkStream(s, false);

            _readTask = readTask();
        }

        public void Close(bool closeSocket = true)
        {
            if (_ct != null)
            {
                _ct.Cancel();
                _ct.Dispose();
                _ct = null;
            }

            if (_ns != null)
            {
                _ns.Close();
                _ns.Dispose();
                _ns = null;

            }

            if (closeSocket && _s != null)
            {
                _s.Close();
                _s.Dispose();
                _s = null;
            }
        }

        private async Task readResponseTask()
        {
            var crlf = Encoding.UTF8.GetBytes("\r\n\r\n");

            while (_ct != null && !_ct.Token.IsCancellationRequested)
            {
                try
                {
                    var headers = await ReadTillSequence(crlf);
                    if (headers == null)
                    {
                        Close();
                        break;
                    }
                    var hs = Encoding.UTF8.GetString(headers);
                    var rsp = HttpResponse.Parse(hs);

                }
                catch { }
            }
        }

        private async Task readTask()
        {
            var crlf = Encoding.UTF8.GetBytes("\r\n\r\n");

            while (_ct != null && !_ct.Token.IsCancellationRequested)
            {
                try
                {
                    var headers = await ReadTillSequence(crlf);
                    if (headers == null)
                    {
                        Close();
                        break;
                    }
                    var hs = Encoding.UTF8.GetString(headers);

                    _srv.L(hs);

                    var req = HttpRequest.Parse(hs);
                    Tuple<HttpResponse, RouteHandle> rsp = null;

                    if (req != null)
                    {
                        if (req.Method == HttpMethod.POST && req.Headers.ContentLength != null)
                        {
                            req.Body = new HttpContent(await ReadExactData(req.Headers.ContentLength.Value));
                        }

                        //handle request
                        rsp = await _srv.Routes.HandleRequest(_srv, req, this);
                    }
                    else
                    {
                        rsp = new Tuple<HttpResponse, RouteHandle>(new HttpResponse(HttpStatus.BadRequest), null);
                    }

                    if (rsp.Item1 != null)
                    {
                        HttpContext ctx = new HttpContext()
                        {
                            Request = req,
                            Response = rsp.Item1,
                            Direction = HttpContextDirection.Response,
                            Options = _srv.Options,
                            ServerName = _srv.ServerName
                        };

                        var dt = await rsp.Item1.ToBuffer(ctx);

                        _srv.L(Encoding.UTF8.GetString(dt));

                        await _ns.WriteAsync(dt, 0, dt.Length);
                    }

                    if (rsp.Item2 != null)
                    {
                        rsp.Item2.AfterResponse?.Invoke(rsp.Item2);
                    }

                    if (req != null && req.Headers.Connection != null)
                    {
                        if (req.Headers.Connection as string == "Close")
                        {
                            Close();
                        }
                    }
                }
                catch
                {
                    Close();
                    break;
                }
            }
        }

        private async Task<byte[]> ReadTillSequence(byte[] seq)
        {
            try
            {
                byte[] ret = new byte[1024];
                int offset = 0;

                read_more:
                int rlen = 0;
                if (!_ct.IsCancellationRequested)
                {
                    rlen = await _ns.ReadAsync(ret, offset, ret.Length - offset);

                    if (rlen != 0 && !EndsWithSeq(ret, offset + rlen, seq))
                    {
                        //extend buffer
                        Array.Resize<byte>(ref ret, ret.Length + 1024);

                        offset += rlen;
                        goto read_more;
                    }
                }

                if (rlen == 0)
                {
                    return null;
                }

                if (ret.Length > offset + rlen)
                {
                    Array.Resize<byte>(ref ret, offset + rlen);
                }

                return ret;
            }
            catch { return null; }
        }

        private bool EndsWithSeq(byte[] data, int end, byte[] seq)
        {
            bool m = true;

            for (var x = end - 1; x > end - 1 - seq.Length; x--)
            {
                if (data[x] != seq[seq.Length - (end - x - 1) - 1])
                {
                    m = false;
                    break;
                }
            }

            return m;
        }

        private async Task<byte[]> ReadExactData(long len)
        {
            try
            {
                byte[] ret = new byte[1024];
                int offset = 0;

                read_more:
                int rlen = 0;
                if (!_ct.IsCancellationRequested)
                {
                    rlen = await _ns.ReadAsync(ret, offset, ret.Length - offset);

                    if (rlen + offset < len && rlen != 0)
                    {
                        //extend buffer
                        Array.Resize<byte>(ref ret, ret.Length + 1024);

                        offset += rlen;
                        goto read_more;
                    }
                }

                if (rlen == 0)
                {
                    return null;
                }

                if (ret.Length > offset + rlen)
                {
                    Array.Resize<byte>(ref ret, offset + rlen);
                }

                return ret;
            }
            catch { return null; }
        }
    }
}
