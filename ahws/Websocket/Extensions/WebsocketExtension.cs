using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ahws.Websocket.Extensions
{
    public abstract class WebsocketExtension
    {
        private static Dictionary<string, Type> EnabledExtensions = new Dictionary<string, Type>();

        public WebsocketExtensionOptions Options { get; set; }

        public abstract string Name { get; }
        
        public static void EnableExtension(WebsocketExtension ext)
        {
            if (!EnabledExtensions.ContainsKey(ext.Name))
            {
                EnabledExtensions.Add(ext.Name, ext.GetType());
            }
        }

        public static List<WebsocketExtensionOptions> ParseOptions(string h)
        {
            List<WebsocketExtensionOptions> opt = new List<WebsocketExtensionOptions>();

            if (h.Contains(","))
            {
                foreach (var o in h.Split(','))
                {
                    opt.Add(new WebsocketExtensionOptions(o));
                }
            }
            else
            {
                opt.Add(new WebsocketExtensionOptions(h));
            }

            return opt;
        }

        public static List<WebsocketExtension> NegotiateAll(string h)
        {
            if (string.IsNullOrEmpty(h))
            {
                return null;
            }

            List<WebsocketExtension> tokens = new List<WebsocketExtension>();
            var opt = ParseOptions(h);

            foreach (var ext in EnabledExtensions)
            {
                var ex = (WebsocketExtension)Activator.CreateInstance(ext.Value);
                ex.Negotiate(opt);
                if (ex.Options != null)
                {
                    tokens.Add(ex);
                }
            }

            return tokens;
        }

        public abstract void Negotiate(List<WebsocketExtensionOptions> o);

        public abstract Task ProcessFrame(WebSocketFrame f);

        public abstract Task ProcessReceivedFrame(WebSocketFrame f);
    }

    public class WebsocketExtensionOptions
    {
        public WebsocketExtensionOptions(string o)
        {
            Parameters = new Dictionary<string, string>();

            if (o.Contains(";"))
            {
                var os = o.Split(';');
                Name = os[0].Trim();

                for (var x = 1; x < os.Length; x++)
                {
                    if (os[x].Contains("="))
                    {
                        var p = os[x].Split('=');
                        if (!Parameters.ContainsKey(p[0].Trim()))
                        {
                            Parameters.Add(p[0].Trim(), p[1].Trim());
                        }
                    }
                    else
                    {
                        if (!Parameters.ContainsKey(os[x].Trim()))
                        {
                            Parameters.Add(os[x].Trim(), null);
                        }
                    }
                }
            }else
            {
                Name = o.Trim();
            }
        }

        public string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            b.Append(Name);

            if(Parameters.Count > 0)
            {
                b.Append("; ");
                b.Append(string.Join("; ", Parameters.Select(a => a.Value != null ? string.Format("{0}={1}", a.Key, a.Value) : a.Key)));
            }

            return b.ToString();
        }
    }
}
