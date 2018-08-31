using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using v0l.ahws.Http;
using v0l.ahws.Websocket.Extensions;

namespace v0l.ahws.Websocket
{
    public class WebsocketServer
    {
        public delegate void NewWebsocket(WebSocket ws);
        public event NewWebsocket OnWebsocketConnect = (ws) => { };

        public async Task<HttpResponse> WebsocketUpgrade(RouteHandle h)
        {
            var rsp = h.Request.CreateResponse(HttpStatus.SwitchingProtocols);

            if (h.Request.Headers.Upgrade != null && (h.Request.Headers.Upgrade as string).ToLower() == "websocket")
            {
                var key = h.Request.Headers["Sec-WebSocket-Key"];
                var version = h.Request.Headers["Sec-WebSocket-Version"];
                var subproto = h.Request.Headers["Sec-WebSocket-Protocol"];
                var ext = h.Request.Headers["Sec-WebSocket-Extensions"];

                if (key != null && version != null)
                {
                    if (version as string == "13")
                    {
                        var wsguid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                        using (SHA1Managed s = new SHA1Managed())
                        {
                            var ha = s.ComputeHash(Encoding.UTF8.GetBytes($"{(key as string)}{wsguid}"));

                            rsp.Headers.Add("Sec-WebSocket-Accept", Convert.ToBase64String(ha));
                            rsp.Headers.Upgrade = "websocket";
                            rsp.Headers.Connection = "Upgrade";

                            var ex = WebsocketExtension.NegotiateAll(ext as string);
                            if (ex != null && ex.Count > 0)
                            {
                                rsp.Headers.Add("Sec-WebSocket-Extensions", string.Join(", ", ex.Select(a => a.Options.ToString())));
                            }
                            
                            OnWebsocketConnect(new WebSocket(h, rsp, ex));
                            h.AfterResponse = (h2) =>
                            {
                                h2.Socket.Close(false);
                            };
                        }
                    }
                    else
                    {
                        rsp.StatusCode = HttpStatus.BadRequest;
                    }
                }
                else
                {
                    rsp.StatusCode = HttpStatus.BadRequest;
                }
            }
            else
            {
                rsp.StatusCode = HttpStatus.UpgradeRequired;
            }

            return rsp;
        }

        public static void WebsocketUpgrade(HttpRequest req, List<WebsocketExtension> ext = null, List<string> subproto = null)
        {
            var key = new byte[10];
            new Random().NextBytes(key);

            req.Headers.Upgrade = "websocket";
            req.Headers.Connection = "Upgrade";
            req.Headers["Sec-WebSocket-Key"] = Convert.ToBase64String(key);
            req.Headers["Sec-WebSocket-Version"] = "13";

            if(ext != null)
            {
                req.Headers["Sec-Websocket-Extensions"] = string.Join(", ", ext.Select(a => a.Options.ToString()));
            }

            if(subproto != null)
            {
                req.Headers["Sec-Websocket-Protocol"] = string.Join(", ", subproto);
            }
        }

        public void EnablePerMessageDeflate()
        {
            WebsocketExtension.EnableExtension(new PerMessageDeflate());
        }
    }
}
