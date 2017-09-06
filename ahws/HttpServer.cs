﻿using ahws.Http;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace ahws
{
    [Flags]
    public enum HttpServerOptions
    {
        None = 1,
        Debug = 2,
        KeepAlive = 4,
        GZip = 8
    }

    public class HttpServer
    {
        private Socket sock;
        private IPEndPoint ip;
        private Thread _t;
        private bool _running;
        private ManualResetEvent _acceptre;

        public Routes Routes;

        public HttpServerOptions Options { get; set; }
        public string ServerName { get; set; }

        public delegate void Log(string msg);
        public event Log OnLog = (e) => { };

        public HttpServer(int port = 11666)
        {
            Init(new IPEndPoint(IPAddress.Any, port));
        }

        public HttpServer(IPAddress i = null, int port = 11666)
        {
            Init(new IPEndPoint(i ?? IPAddress.Any, port));
        }

        private void Init(IPEndPoint i)
        {
            var v = Assembly.GetExecutingAssembly().GetName();
            ServerName = string.Format("{0} v{1}.{2}.{3}", v.Name, v.Version.Major, v.Version.Minor, v.Version.Build);
            Options = HttpServerOptions.KeepAlive;

            Routes = new Routes();
            ip = i;
            AddDefaultRoutes();
        }

        private void AddDefaultRoutes()
        {
            Routes.AddRoute("/favicon.ico", (h) =>
            {
                HttpResponse rsp = h.Request.CreateResponse(HttpStatus.OK);

                rsp.Headers.ContentType = "image/png";
                rsp.Content = new HttpContent(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAAABAAAAAQBPJcTWAAAA2UlEQVR4nO2WUQ6CMAyG6wUgEnkXb+AV2ORceiA4AQTuo3fQLtaIS8FsmTVZ1uTPtjbtvqc/BUghFxXqbOkgCdCg7pYaSQDFAKgoAXJUidrRmUsDtKgb6kpnS/laCmC0PhmlAXrrkz5KAM5U9pIAnKmcJAG4YXWUABk8DeWlTBqgg09T6aQBXEzlJwAuwxKAN4BZiy7wdjRzryQBQrmaNwC3KPgMSwBxAgwBAIZvAJopaGqarPxE+bUNd6ln8Z+CLopk7ltqOs5qmt5A9RA9BazExjHv2/PfeAA/ToosuKQ06gAAAABJRU5ErkJggg=="));

                return rsp;
            });
        }

        public bool AddRoute(string path, Func<RouteHandle, HttpResponse> func)
        {
            return Routes.AddRoute(path, func);
        }

        public bool AddRoute(Regex r, Func<RouteHandle, HttpResponse> func)
        {
            return Routes.AddRoute(r, func);
        }

        public bool AddRoute(Route r, Func<RouteHandle, HttpResponse> func)
        {
            return Routes.AddRoute(r, func);
        }

        internal void L(string msg)
        {
            if (Options.HasFlag(HttpServerOptions.Debug))
            {
                OnLog(msg);
            }
        }

        public void Start()
        {
            _running = true;
            _t = new Thread(new ThreadStart(() => {
                try
                {
                    sock = new Socket(SocketType.Stream, ProtocolType.Tcp);

                    sock.Bind(ip);
                    sock.Listen(100);

                    OnLog(String.Format("[HttpServer] Listening on {0}", ip.ToString()));
                }
                catch (Exception ex)
                {
                    OnLog(string.Format("[HttpServer] Start failed: {0}", ex.ToString()));
                    return;
                }

                while (_running)
                {
                    StartListening();
                }
            }));
            _t.Start();
        }

        public void Stop()
        {
            _running = false;
            _acceptre.Set();
            _t.Join();
        }
        
        private void StartListening()
        {
            _acceptre = new ManualResetEvent(false);

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += (s, a) => {
                HandleSocket(e.AcceptSocket);
                _acceptre.Set();
            };
            
            try
            {
                if (!sock.AcceptAsync(e))
                {
                    HandleSocket(e.AcceptSocket);
                    _acceptre.Set();
                }
            }
            catch(Exception ex)
            {
                OnLog(ex.ToString());
            }

            _acceptre.WaitOne();
        }
        

        private void HandleSocket(Socket s)
        {
            L(String.Format("[HttpServer] Connection from: {0}", s.RemoteEndPoint.ToString()));
            new HttpSocket(this, s);
        }
    }
}