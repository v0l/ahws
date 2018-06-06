using v0l.ahws;
using v0l.ahws.Http;
using System;
using System.Text;

namespace ahws_test
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer s = new HttpServer(8080);
            s.OnLog += Console.WriteLine;
            s.Options = HttpServerOptions.GZip | HttpServerOptions.KeepAlive;

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
