using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using ahws.Http;

namespace ahws
{
    public enum RouteType
    {
        Absolute,
        Regex
    }

    public class Route
    {
        public Route(string path)
        {
            Type = RouteType.Absolute;
            Value = path;
        }

        public Route(Regex r)
        {
            Type = RouteType.Regex;
            Value = r;
        }
        public RouteType Type { get; set; }

        public object Value { get; set; }
        
        public bool Match(HttpRequest req)
        {
            if(Type == RouteType.Absolute)
            {
                return req.RequestUri.AbsolutePath.ToLower() == (Value as string).ToLower();
            }
            else if(Type == RouteType.Regex)
            {
                return (Value as Regex).IsMatch(req.RequestUri.AbsolutePath.ToLower());
            }

            return false;
        }
    }

    public class RouteHandle
    {
        public RouteHandle(Route r, HttpServer s, HttpRequest req, HttpSocket so)
        {
            Route = r;
            Server = s;
            Request = req;
            Socket = so;
        }

        public HttpSocket Socket { get; set; }
        public HttpServer Server { get; set; }
        public HttpRequest Request { get; set; }
        public Route Route { get; set; }
    }

    public class Routes
    {
        private static string error_html = @"
<html>
    <head>
        <title>Route Error</title>
        <style>
            html, body {{ margin:0; padding: 10px; font-family: Arial; background-color: red; color: white; }}
        </style>
    </head>
    <body>
        <h3>{0}</h3>
        <pre>{1}</pre>
    </body>
</html>";

        private ConcurrentDictionary<Route, Func<RouteHandle, HttpResponse>> routes = new ConcurrentDictionary<Route, Func<RouteHandle, HttpResponse>>();
        
        public bool AddRoute(string path, Func<RouteHandle, HttpResponse> func)
        {
            return routes.TryAdd(new Route(path), func);
        }

        public bool AddRoute(Regex r, Func<RouteHandle, HttpResponse> func)
        {
            return routes.TryAdd(new Route(r), func);
        }

        public bool AddRoute(Route r, Func<RouteHandle, HttpResponse> func)
        {
            return routes.TryAdd(r, func);
        }

        public async Task<HttpResponse> HandleRequest(HttpServer srv, HttpRequest req, HttpSocket so)
        {
            var handle = routes.Keys.FirstOrDefault(a => a.Match(req));
            
            if (handle != null)
            {
                try
                {
                    var rsp = await Task.Run<HttpResponse>(() => routes[handle](new RouteHandle(handle, srv, req, so)));
                    if(rsp != null)
                    {
                        return rsp;
                    }
                    else
                    {
                        return req.CreateResponse(HttpStatus.InternalServerError);
                    }
                }
                catch(Exception ex)
                {
                    var rsp = req.CreateResponse(HttpStatus.InternalServerError);
                    
                    var dt = Encoding.UTF8.GetBytes(string.Format(error_html, ex.Message, ex.ToString()));
                    rsp.Content = new HttpContent(dt);
                    rsp.Headers.ContentLength = dt.LongLength;
                    rsp.Headers.ContentType = "text/html";

                    return rsp;
                }
            }
            else
            {
                return req.CreateResponse(HttpStatus.NotFound);
            }
        }

        public List<Route> GetRoutes()
        {
            return routes.Keys.ToList();
        }
    }
}
