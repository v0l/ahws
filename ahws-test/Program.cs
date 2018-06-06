using v0l.ahws;
using v0l.ahws.Http;
using System;
using System.Text;
using v0l.ahws.Websocket;
using System.Collections.Generic;

namespace ahws_test
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer s = new HttpServer(8080);

            WebsocketServer ws = new WebsocketServer();
            ws.OnWebsocketConnect += (w) =>
            {
                w.OnFrame += async (f) =>
                {
                    //send the frame back
                    await f.WebSocket.SendFrame(f.Frame);
                };

                w.Start();
            };

            s.OnLog += Console.WriteLine;
            s.Options = HttpServerOptions.GZip | HttpServerOptions.KeepAlive;

            s.AddRoute("/ws", ws.WebsocketUpgrade);
            s.AddRoute("/", (h) => {
                var rsp = h.Request.CreateResponse(HttpStatus.OK);

                rsp.Content = new HttpContent(Encoding.UTF8.GetBytes("<h1>Hello world!</h1>"));
                rsp.Headers.ContentType = "text/html";

                return rsp;
            });

            s.Start();

            Console.ReadKey();
        }
    }
}
