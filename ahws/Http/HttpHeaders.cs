using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ahws.Http
{
    public class HttpHeaders : IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> _headers = new Dictionary<string, object>();

        public object this[string key]
        {
            get
            {
                if (_headers.ContainsKey(key))
                {
                    return _headers[key];
                }
                return null;
            }
            set
            {
                if (_headers.ContainsKey(key))
                {
                    _headers[key] = value;
                }
                else
                {
                    _headers.Add(key, value);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var h in _headers)
            {
                if (h.Value != null)
                {
                    sb.Append(string.Format("{0}: {1}\r\n", h.Key, h.Value.ToString()));
                }
            }

            return sb.ToString();
        }

        public void Add(string key, object val)
        {
            if (!_headers.ContainsKey(key))
            {
                _headers.Add(key, val);
            }
            else
            {
                _headers[key] = val;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _headers.GetEnumerator();
        }

        public object Accept
        {
            get
            {
                return _headers.ContainsKey("Accept") ? _headers["Accept"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Accept"))
                {
                    _headers["Accept"] = value;
                }
                else
                {
                    _headers.Add("Accept", value);
                }
            }
        }

        public object AcceptCharset
        {
            get
            {
                return _headers.ContainsKey("Accept-Charset") ? _headers["Accept-Charset"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Accept-Charset"))
                {
                    _headers["Accept-Charset"] = value;
                }
                else
                {
                    _headers.Add("Accept-Charset", value);
                }
            }
        }

        public object AcceptEncoding
        {
            get
            {
                return _headers.ContainsKey("Accept-Encoding") ? _headers["Accept-Encoding"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Accept-Encoding"))
                {
                    _headers["Accept-Encoding"] = value;
                }
                else
                {
                    _headers.Add("Accept-Encoding", value);
                }
            }
        }

        public object AcceptLanguage
        {
            get
            {
                return _headers.ContainsKey("Accept-Language") ? _headers["Accept-Language"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Accept-Language"))
                {
                    _headers["Accept-Language"] = value;
                }
                else
                {
                    _headers.Add("Accept-Language", value);
                }
            }
        }

        public object AcceptPatch
        {
            get
            {
                return _headers.ContainsKey("Accept-Patch") ? _headers["Accept-Patch"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Accept-Patch"))
                {
                    _headers["Accept-Patch"] = value;
                }
                else
                {
                    _headers.Add("Accept-Patch", value);
                }
            }
        }

        public object AcceptRanges
        {
            get
            {
                return _headers.ContainsKey("Accept-Ranges") ? _headers["Accept-Ranges"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Accept-Ranges"))
                {
                    _headers["Accept-Ranges"] = value;
                }
                else
                {
                    _headers.Add("Accept-Ranges", value);
                }
            }
        }

        public object Age
        {
            get
            {
                return _headers.ContainsKey("Age") ? _headers["Age"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Age"))
                {
                    _headers["Age"] = value;
                }
                else
                {
                    _headers.Add("Age", value);
                }
            }
        }

        public object Allow
        {
            get
            {
                return _headers.ContainsKey("Allow") ? _headers["Allow"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Allow"))
                {
                    _headers["Allow"] = value;
                }
                else
                {
                    _headers.Add("Allow", value);
                }
            }
        }

        public object AltSvc
        {
            get
            {
                return _headers.ContainsKey("Alt-Svc") ? _headers["Alt-Svc"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Alt-Svc"))
                {
                    _headers["Alt-Svc"] = value;
                }
                else
                {
                    _headers.Add("Alt-Svc", value);
                }
            }
        }

        public object Authorization
        {
            get
            {
                return _headers.ContainsKey("Authorization") ? _headers["Authorization"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Authorization"))
                {
                    _headers["Authorization"] = value;
                }
                else
                {
                    _headers.Add("Authorization", value);
                }
            }
        }

        public object CacheControl
        {
            get
            {
                return _headers.ContainsKey("Cache-Control") ? _headers["Cache-Control"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Cache-Control"))
                {
                    _headers["Cache-Control"] = value;
                }
                else
                {
                    _headers.Add("Cache-Control", value);
                }
            }
        }

        public object Connection
        {
            get
            {
                return _headers.ContainsKey("Connection") ? _headers["Connection"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Connection"))
                {
                    _headers["Connection"] = value;
                }
                else
                {
                    _headers.Add("Connection", value);
                }
            }
        }

        public object ContentDisposition
        {
            get
            {
                return _headers.ContainsKey("Content-Disposition") ? _headers["Content-Disposition"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Content-Disposition"))
                {
                    _headers["Content-Disposition"] = value;
                }
                else
                {
                    _headers.Add("Content-Disposition", value);
                }
            }
        }

        public object ContentEncoding
        {
            get
            {
                return _headers.ContainsKey("Content-Encoding") ? _headers["Content-Encoding"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Content-Encoding"))
                {
                    _headers["Content-Encoding"] = value;
                }
                else
                {
                    _headers.Add("Content-Encoding", value);
                }
            }
        }

        public object ContentLanguage
        {
            get
            {
                return _headers.ContainsKey("Content-Language") ? _headers["Content-Language"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Content-Language"))
                {
                    _headers["Content-Language"] = value;
                }
                else
                {
                    _headers.Add("Content-Language", value);
                }
            }
        }

        /// <summary>
        /// This value is always overwritten before sending response to client
        /// <para>Setting this value manually is pointless :)</para>
        /// </summary>
        public long? ContentLength
        {
            get
            {
                long? ret = null;

                if (_headers.ContainsKey("Content-Length"))
                {
                    if (_headers["Content-Length"] is long)
                    {
                        return (long)_headers["Content-Length"];
                    }
                    else
                    {
                        long tl;
                        if (long.TryParse((string)_headers["Content-Length"], out tl))
                        {
                            _headers["Content-Length"] = tl;
                            ret = tl;
                        }
                    }
                }

                return ret;
            }
            set
            {
                if (_headers.ContainsKey("Content-Length"))
                {
                    _headers["Content-Length"] = value;
                }
                else
                {
                    _headers.Add("Content-Length", value);
                }
            }
        }

        public object ContentLocation
        {
            get
            {
                return _headers.ContainsKey("Content-Location") ? _headers["Content-Location"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Content-Location"))
                {
                    _headers["Content-Location"] = value;
                }
                else
                {
                    _headers.Add("Content-Location", value);
                }
            }
        }

        public object ContentType
        {
            get
            {
                return _headers.ContainsKey("Content-Type") ? _headers["Content-Type"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Content-Type"))
                {
                    _headers["Content-Type"] = value;
                }
                else
                {
                    _headers.Add("Content-Type", value);
                }
            }
        }

        public object Cookie
        {
            get
            {
                return _headers.ContainsKey("Cookie") ? _headers["Cookie"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Cookie"))
                {
                    _headers["Cookie"] = value;
                }
                else
                {
                    _headers.Add("Cookie", value);
                }
            }
        }

        public object Date
        {
            get
            {
                return _headers.ContainsKey("Date") ? _headers["Date"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Date"))
                {
                    _headers["Date"] = value;
                }
                else
                {
                    _headers.Add("Date", value);
                }
            }
        }

        public object ETag
        {
            get
            {
                return _headers.ContainsKey("ETag") ? _headers["ETag"] : null;
            }
            set
            {
                if (_headers.ContainsKey("ETag"))
                {
                    _headers["ETag"] = value;
                }
                else
                {
                    _headers.Add("ETag", value);
                }
            }
        }

        public object Expect
        {
            get
            {
                return _headers.ContainsKey("Expect") ? _headers["Expect"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Expect"))
                {
                    _headers["Expect"] = value;
                }
                else
                {
                    _headers.Add("Expect", value);
                }
            }
        }

        public object Expires
        {
            get
            {
                return _headers.ContainsKey("Expires") ? _headers["Expires"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Expires"))
                {
                    _headers["Expires"] = value;
                }
                else
                {
                    _headers.Add("Expires", value);
                }
            }
        }

        public object Forwarded
        {
            get
            {
                return _headers.ContainsKey("Forwarded") ? _headers["Forwarded"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Forwarded"))
                {
                    _headers["Forwarded"] = value;
                }
                else
                {
                    _headers.Add("Forwarded", value);
                }
            }
        }

        public object From
        {
            get
            {
                return _headers.ContainsKey("From") ? _headers["From"] : null;
            }
            set
            {
                if (_headers.ContainsKey("From"))
                {
                    _headers["From"] = value;
                }
                else
                {
                    _headers.Add("From", value);
                }
            }
        }

        public object Host
        {
            get
            {
                return _headers.ContainsKey("Host") ? _headers["Host"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Host"))
                {
                    _headers["Host"] = value;
                }
                else
                {
                    _headers.Add("Host", value);
                }
            }
        }

        public object IfMatch
        {
            get
            {
                return _headers.ContainsKey("If-Match") ? _headers["If-Match"] : null;
            }
            set
            {
                if (_headers.ContainsKey("If-Match"))
                {
                    _headers["If-Match"] = value;
                }
                else
                {
                    _headers.Add("If-Match", value);
                }
            }
        }

        public object IfModifiedSince
        {
            get
            {
                return _headers.ContainsKey("If-Modified-Since") ? _headers["If-Modified-Since"] : null;
            }
            set
            {
                if (_headers.ContainsKey("If-Modified-Since"))
                {
                    _headers["If-Modified-Since"] = value;
                }
                else
                {
                    _headers.Add("If-Modified-Since", value);
                }
            }
        }

        public object IfNoneMatch
        {
            get
            {
                return _headers.ContainsKey("If-None-Match") ? _headers["If-None-Match"] : null;
            }
            set
            {
                if (_headers.ContainsKey("If-None-Match"))
                {
                    _headers["If-None-Match"] = value;
                }
                else
                {
                    _headers.Add("If-None-Match", value);
                }
            }
        }

        public object IfRange
        {
            get
            {
                return _headers.ContainsKey("If-Range") ? _headers["If-Range"] : null;
            }
            set
            {
                if (_headers.ContainsKey("If-Range"))
                {
                    _headers["If-Range"] = value;
                }
                else
                {
                    _headers.Add("If-Range", value);
                }
            }
        }

        public object IfUnmodifiedSince
        {
            get
            {
                return _headers.ContainsKey("If-Unmodified-Since") ? _headers["If-Unmodified-Since"] : null;
            }
            set
            {
                if (_headers.ContainsKey("If-Unmodified-Since"))
                {
                    _headers["If-Unmodified-Since"] = value;
                }
                else
                {
                    _headers.Add("If-Unmodified-Since", value);
                }
            }
        }

        public object LastModified
        {
            get
            {
                return _headers.ContainsKey("Last-Modified") ? _headers["Last-Modified"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Last-Modified"))
                {
                    _headers["Last-Modified"] = value;
                }
                else
                {
                    _headers.Add("Last-Modified", value);
                }
            }
        }

        public object Link
        {
            get
            {
                return _headers.ContainsKey("Link") ? _headers["Link"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Link"))
                {
                    _headers["Link"] = value;
                }
                else
                {
                    _headers.Add("Link", value);
                }
            }
        }

        public object Location
        {
            get
            {
                return _headers.ContainsKey("Location") ? _headers["Location"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Location"))
                {
                    _headers["Location"] = value;
                }
                else
                {
                    _headers.Add("Location", value);
                }
            }
        }

        public object MaxForwards
        {
            get
            {
                return _headers.ContainsKey("Max-Forwards") ? _headers["Max-Forwards"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Max-Forwards"))
                {
                    _headers["Max-Forwards"] = value;
                }
                else
                {
                    _headers.Add("Max-Forwards", value);
                }
            }
        }

        public object Origin
        {
            get
            {
                return _headers.ContainsKey("Origin") ? _headers["Origin"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Origin"))
                {
                    _headers["Origin"] = value;
                }
                else
                {
                    _headers.Add("Origin", value);
                }
            }
        }

        public object P3P
        {
            get
            {
                return _headers.ContainsKey("P3P") ? _headers["P3P"] : null;
            }
            set
            {
                if (_headers.ContainsKey("P3P"))
                {
                    _headers["P3P"] = value;
                }
                else
                {
                    _headers.Add("P3P", value);
                }
            }
        }

        public object Pragma
        {
            get
            {
                return _headers.ContainsKey("Pragma") ? _headers["Pragma"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Pragma"))
                {
                    _headers["Pragma"] = value;
                }
                else
                {
                    _headers.Add("Pragma", value);
                }
            }
        }

        public object ProxyAuthorization
        {
            get
            {
                return _headers.ContainsKey("Proxy-Authorization") ? _headers["Proxy-Authorization"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Proxy-Authorization"))
                {
                    _headers["Proxy-Authorization"] = value;
                }
                else
                {
                    _headers.Add("Proxy-Authorization", value);
                }
            }
        }

        public object PublicKeyPins
        {
            get
            {
                return _headers.ContainsKey("Public-Key-Pins") ? _headers["Public-Key-Pins"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Public-Key-Pins"))
                {
                    _headers["Public-Key-Pins"] = value;
                }
                else
                {
                    _headers.Add("Public-Key-Pins", value);
                }
            }
        }

        public object Range
        {
            get
            {
                return _headers.ContainsKey("Range") ? _headers["Range"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Range"))
                {
                    _headers["Range"] = value;
                }
                else
                {
                    _headers.Add("Range", value);
                }
            }
        }

        public object Referer
        {
            get
            {
                return _headers.ContainsKey("Referer") ? _headers["Referer"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Referer"))
                {
                    _headers["Referer"] = value;
                }
                else
                {
                    _headers.Add("Referer", value);
                }
            }
        }

        public object RetryAfter
        {
            get
            {
                return _headers.ContainsKey("Retry-After") ? _headers["Retry-After"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Retry-After"))
                {
                    _headers["Retry-After"] = value;
                }
                else
                {
                    _headers.Add("Retry-After", value);
                }
            }
        }

        public object Server
        {
            get
            {
                return _headers.ContainsKey("Server") ? _headers["Server"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Server"))
                {
                    _headers["Server"] = value;
                }
                else
                {
                    _headers.Add("Server", value);
                }
            }
        }

        public object SetCookie
        {
            get
            {
                return _headers.ContainsKey("Set-Cookie") ? _headers["Set-Cookie"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Set-Cookie"))
                {
                    _headers["Set-Cookie"] = value;
                }
                else
                {
                    _headers.Add("Set-Cookie", value);
                }
            }
        }

        public object Status
        {
            get
            {
                return _headers.ContainsKey("Status") ? _headers["Status"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Status"))
                {
                    _headers["Status"] = value;
                }
                else
                {
                    _headers.Add("Status", value);
                }
            }
        }

        public object StrictTransportSecurity
        {
            get
            {
                return _headers.ContainsKey("Strict-Transport-Security") ? _headers["Strict-Transport-Security"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Strict-Transport-Security"))
                {
                    _headers["Strict-Transport-Security"] = value;
                }
                else
                {
                    _headers.Add("Strict-Transport-Security", value);
                }
            }
        }

        public object TE
        {
            get
            {
                return _headers.ContainsKey("TE") ? _headers["TE"] : null;
            }
            set
            {
                if (_headers.ContainsKey("TE"))
                {
                    _headers["TE"] = value;
                }
                else
                {
                    _headers.Add("TE", value);
                }
            }
        }

        public object Trailer
        {
            get
            {
                return _headers.ContainsKey("Trailer") ? _headers["Trailer"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Trailer"))
                {
                    _headers["Trailer"] = value;
                }
                else
                {
                    _headers.Add("Trailer", value);
                }
            }
        }

        public object TransferEncoding
        {
            get
            {
                return _headers.ContainsKey("Transfer-Encoding") ? _headers["Transfer-Encoding"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Transfer-Encoding"))
                {
                    _headers["Transfer-Encoding"] = value;
                }
                else
                {
                    _headers.Add("Transfer-Encoding", value);
                }
            }
        }

        public object TSV
        {
            get
            {
                return _headers.ContainsKey("TSV") ? _headers["TSV"] : null;
            }
            set
            {
                if (_headers.ContainsKey("TSV"))
                {
                    _headers["TSV"] = value;
                }
                else
                {
                    _headers.Add("TSV", value);
                }
            }
        }

        public object Upgrade
        {
            get
            {
                return _headers.ContainsKey("Upgrade") ? _headers["Upgrade"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Upgrade"))
                {
                    _headers["Upgrade"] = value;
                }
                else
                {
                    _headers.Add("Upgrade", value);
                }
            }
        }

        public object UserAgent
        {
            get
            {
                return _headers.ContainsKey("User-Agent") ? _headers["User-Agent"] : null;
            }
            set
            {
                if (_headers.ContainsKey("User-Agent"))
                {
                    _headers["User-Agent"] = value;
                }
                else
                {
                    _headers.Add("User-Agent", value);
                }
            }
        }

        public object Vary
        {
            get
            {
                return _headers.ContainsKey("Vary") ? _headers["Vary"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Vary"))
                {
                    _headers["Vary"] = value;
                }
                else
                {
                    _headers.Add("Vary", value);
                }
            }
        }

        public object Via
        {
            get
            {
                return _headers.ContainsKey("Via") ? _headers["Via"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Via"))
                {
                    _headers["Via"] = value;
                }
                else
                {
                    _headers.Add("Via", value);
                }
            }
        }

        public object Warning
        {
            get
            {
                return _headers.ContainsKey("Warning") ? _headers["Warning"] : null;
            }
            set
            {
                if (_headers.ContainsKey("Warning"))
                {
                    _headers["Warning"] = value;
                }
                else
                {
                    _headers.Add("Warning", value);
                }
            }
        }

        public object WWWAuthenticate
        {
            get
            {
                return _headers.ContainsKey("WWW-Authenticate") ? _headers["WWW-Authenticate"] : null;
            }
            set
            {
                if (_headers.ContainsKey("WWW-Authenticate"))
                {
                    _headers["WWW-Authenticate"] = value;
                }
                else
                {
                    _headers.Add("WWW-Authenticate", value);
                }
            }
        }

        public object XForwardFor
        {
            get
            {
                return _headers.ContainsKey("X-Forwarded-For") ? _headers["X-Forwarded-For"] : null;
            }
            set
            {
                if (_headers.ContainsKey("X-Forwarded-For"))
                {
                    _headers["X-Forwarded-For"] = value;
                }
                else
                {
                    _headers.Add("X-Forwarded-For", value);
                }
            }
        }
    }
}
