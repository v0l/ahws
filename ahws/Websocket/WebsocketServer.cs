using ahws.Http;
using ahws.Websocket.Extensions;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ahws.Websocket
{
    public class WebsocketServer
    {
        public delegate void NewWebsocket(WebSocket ws);
        public event NewWebsocket OnWebsocketConnect = (ws) => { };

        public HttpResponse WebsocketUpgrade(RouteHandle h)
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
                            var ha = s.ComputeHash(Encoding.UTF8.GetBytes(string.Format("{0}{1}", (key as string), wsguid)));

                            rsp.Headers.Add("Sec-WebSocket-Accept", Convert.ToBase64String(ha));
                            rsp.Headers.Upgrade = "websocket";
                            rsp.Headers.Connection = "Upgrade";

                            var ex = WebsocketExtension.NegotiateAll(ext as string);
                            if (ex != null && ex.Count > 0)
                            {
                                rsp.Headers.Add("Sec-WebSocket-Extensions", string.Join(", ", ex.Select(a => a.Options.ToString())));
                            }

                            OnWebsocketConnect(new WebSocket(h.Request, rsp, h.Socket.Socket, ex));

                            h.Socket.Close();
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

        public void EnablePerMessageDeflate()
        {
            WebsocketExtension.EnableExtension(new PerMessageDeflate());
        }
    }
}
