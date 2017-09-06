using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace ahws.Http
{
    public enum HttpStatus
    {
        Accepted = 202,
        Ambiguous = 300,
        BadGateway = 502,
        BadRequest = 400,
        Conflict = 409,
        Continue = 100,
        Created = 201,
        ExpectationFailed = 417,
        Forbidden = 403,
        Found = 302,
        GatewayTimeout = 504,
        Gone = 410,
        HttpVersionNotSupported = 505,
        InternalServerError = 500,
        LengthRequired = 411,
        MethodNotAllowed = 405,
        Moved = 301,
        MovedPermanently = 301,
        MultipleChoices = 300,
        NoContent = 204,
        NonAuthoritativeInformation = 203,
        NotAcceptable = 406,
        NotFound = 404,
        NotImplemented = 501,
        NotModified = 304,
        OK = 200,
        PartialContent = 206,
        PaymentRequired = 402,
        PreconditionFailed = 412,
        ProxyAuthenticationRequired = 407,
        Redirect = 302,
        RedirectKeepVerb = 307,
        RedirectMethod = 303,
        RequestedRangeNotSatisfiable = 416,
        RequestEntityTooLarge = 413,
        RequestTimeout = 408,
        RequestUriTooLong = 414,
        ResetContent = 205,
        SeeOther = 303,
        ServiceUnavailable = 503,
        SwitchingProtocols = 101,
        TemporaryRedirect = 307,
        Unauthorized = 401,
        UnsupportedMediaType = 415,
        Unused = 306,
        UpgradeRequired = 426,
        UseProxy = 305
    }

    public class HttpContent
    {
        private byte[] Data { get; set; }

        public HttpContent(byte[] dt)
        {
            Data = dt;
        }

        public async Task<byte[]> GetData(HttpContext ctx)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                if (ctx.Direction == HttpContextDirection.Response)
                {
                    if (ctx.Options.HasFlag(HttpServerOptions.GZip) && (ctx.Request != null && ctx.Request.Headers.AcceptEncoding != null && (ctx.Request.Headers.AcceptEncoding as string).Contains("gzip")))
                    {
                        ctx.Response.Headers.ContentEncoding = "gzip";
                        using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
                        {
                            await gz.WriteAsync(Data, 0, Data.Length);
                        }
                    }
                    else
                    {
                        await ms.WriteAsync(Data, 0, Data.Length);
                    }
                }

                return ms.ToArray();
            }
        }
    }

    public class HttpResponse
    {
        public HttpStatus StatusCode { get; set; }

        public Version Version { get; set; }

        public HttpHeaders Headers { get; set; }

        public HttpContent Content { get; set; }

        public HttpRequest Request { get; set; }

        public HttpResponse()
        {
            Headers = new HttpHeaders();
        }

        public HttpResponse(HttpStatus st)
        {
            Headers = new HttpHeaders();
            StatusCode = st;
            Version = new Version("1.1");
        }

        public async Task<byte[]> ToBuffer(HttpContext ctx)
        {
            byte[] ret = null;

            //set content length header
            Headers.Server = ctx.ServerName;
            if (Headers.Connection == null || Headers.Connection as string != "Upgrade")
            {
                if (ctx.Options.HasFlag(HttpServerOptions.KeepAlive))
                {
                    Headers.Connection = "keep-alive";
                }
                else
                {
                    Headers.Connection = "close";
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] dt = new byte[0];
                Headers.ContentLength = 0;

                if (Content != null)
                {
                    dt = await Content.GetData(ctx);
                    Headers.ContentLength = dt.Length;
                }

                StringBuilder sb = new StringBuilder();

                sb.Append(string.Format("HTTP/{0} {1} {2}\r\n", Version.ToString(), (int)StatusCode, StatusCode.ToString()));
                sb.Append(Headers.ToString());
                sb.Append("\r\n");

                byte[] header = Encoding.UTF8.GetBytes(sb.ToString());

                await ms.WriteAsync(header, 0, header.Length);
                await ms.WriteAsync(dt, 0, dt.Length);

                ret = ms.ToArray();
            }

            return ret;
        }

        public static HttpResponse Parse(string headers)
        {
            var ret = new HttpResponse();

            int ln = 0;
            foreach (string line in headers.Split('\n'))
            {
                var l = line.Trim();

                if (!string.IsNullOrEmpty(l))
                {
                    if (ln == 0) //not k: v
                    {
                        string[] ls = l.Split(' ');
                        string status = ls[1];

                        ret.StatusCode = (HttpStatus)int.Parse(status);
                        ret.Version = Version.Parse(ls[0].Split('/')[1]);
                    }
                    else
                    {
                        string[] header = l.Split(new char[] { ':' }, 2);

                        ret.Headers.Add(header[0].Trim(), header[1].Trim());
                    }
                }
                ln++;
            }

            return ret;
        }
    }
}
