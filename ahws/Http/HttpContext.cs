namespace ahws.Http
{
    public enum HttpContextDirection
    {
        Unknown,
        Request,
        Response
    }

    public class HttpContext
    {
        public HttpContextDirection Direction { get; set; }
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public HttpServerOptions Options { get; set; }
        public string ServerName { get; set; }
    }
}
