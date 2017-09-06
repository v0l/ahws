using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace ahws.Websocket.Extensions
{
    public class PerMessageDeflate : WebsocketExtension
    {
        public override string Name => "permessage-deflate";
                
        public override void Negotiate(List<WebsocketExtensionOptions> o)
        {
            foreach(var op in o)
            {
                if(op.Name.ToLower() == Name.ToLower())
                {
                    Options = op;
                    if (Options.Parameters != null)
                    {
                        Options.Parameters.Clear();
                        Options.Parameters.Add("client_no_context_takeover", null);
                        //Options.Parameters.Add("server_no_context_takeover", null);
                    }
                    break;
                }
            }
        }

        public override async Task ProcessFrame(WebSocketFrame f)
        {
            if (f.OpCode == WebSocketOpCode.BinaryFrame || f.OpCode == WebSocketOpCode.TextFrame)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (DeflateStream df = new DeflateStream(ms, CompressionMode.Compress, true))
                        {
                            await df.WriteAsync(f.Payload, 0, f.Payload.Length);
                        }

                        await ms.WriteAsync(new byte[] { 0x00 }, 0, 1);

                        f.Flags |= WebSocketFlags.RSV3;
                        f.Payload = ms.ToArray();
                        f.PayloadLength = (ulong)f.Payload.Length;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public override async Task ProcessReceivedFrame(WebSocketFrame f)
        {
            if ((f.OpCode == WebSocketOpCode.BinaryFrame || f.OpCode == WebSocketOpCode.TextFrame) && f.Flags.HasFlag(WebSocketFlags.RSV3))
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (var ms2 = new MemoryStream())
                        {
                            await ms2.WriteAsync(f.Payload, 0, f.Payload.Length);
                            await ms2.WriteAsync(new byte[] { 0x00, 0x00, 0xff, 0xff }, 0, 4);
                            ms2.Seek(0, SeekOrigin.Begin);

                            using (DeflateStream df = new DeflateStream(ms2, CompressionMode.Decompress, true))
                            {
                                await df.CopyToAsync(ms);
                            }
                        }

                        f.Payload = ms.ToArray();
                        f.PayloadLength = (ulong)f.Payload.Length;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
