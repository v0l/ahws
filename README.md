AHWS 
==

Async Http Web Server

Simple to use Async webserver with websocket support

Example Http Server
===
```
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
```

Example Websocket Echo
===
```
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

s.Start();
```