using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ahws.Http
{
    public enum HttpMethod
    {
        UNKNOWN,
        HEAD,
        GET,
        POST,
        OPTIONS,
        PUT,
        DELETE,
        TRACE,
        CONNECT,
        PATCH
    }

    public class HttpRequest
    {
        public string OriginalMessage { get; set; }

        public HttpMethod Method { get; set; }

        public Version Version { get; set; }

        public Uri RequestUri { get; set; }

        public HttpHeaders Headers { get; set; }

        public HttpContent Body { get; set; }

        public HttpRequest()
        {
            Headers = new HttpHeaders();
        }

        public HttpResponse CreateResponse(HttpStatus status)
        {
            HttpResponse ret = new HttpResponse();
            ret.StatusCode = status;
            ret.Version = Version;
            ret.Headers.Connection = Headers.Connection;
            ret.Request = this;

            return ret;
        }

        public async Task<byte[]> ToBuffer()
        {
            byte[] ret = null;

            using (MemoryStream ms = new MemoryStream())
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(string.Format("{0} {1} HTTP/{2}\r\n", Method.ToString(), RequestUri.PathAndQuery, Version.ToString()));
                sb.Append(Headers.ToString());
                sb.Append("\r\n");

                var header = Encoding.UTF8.GetBytes(sb.ToString());

                await ms.WriteAsync(header, 0, header.Length);

                ret = ms.ToArray();
            }

            return ret;
        }

        public static HttpRequest Parse(string headers)
        {
            if (!string.IsNullOrEmpty(headers))
            {
                HttpRequest rsp = new HttpRequest();
                rsp.OriginalMessage = headers;

                int ln = 0;
                foreach (string line in headers.Split('\n'))
                {
                    var l = line.Trim();

                    if (!string.IsNullOrEmpty(l))
                    {
                        if (ln == 0) //not k: v
                        {
                            string[] ls = l.Split(' ');
                            string method = ls[0];
                            string path = ls[1];
                            string proto = ls[2];

                            rsp.Method = GetMethod(method);
                            rsp.RequestUri = new Uri(path, UriKind.Relative);
                            rsp.Version = Version.Parse(proto.Split('/')[1]);
                        }
                        else
                        {
                            string[] header = l.Split(':');

                            rsp.Headers.Add(header[0].Trim(), header[1].Trim());
                        }
                    }
                    ln++;
                }

                //fill in some extra values after all headers are loaded
                rsp.RequestUri = new Uri(new Uri(string.Format("http://{0}", rsp.Headers.Host)), rsp.RequestUri.OriginalString);

                return rsp;
            }
            return null;
        }

        private static HttpMethod GetMethod(string me)
        {
            var ret = HttpMethod.UNKNOWN;

            foreach(var m in Enum.GetValues(typeof(HttpMethod)))
            {
                if(m.ToString() == me)
                {
                    ret = (HttpMethod)(int)m;
                }
            }

            return ret;
        }
    }
}
