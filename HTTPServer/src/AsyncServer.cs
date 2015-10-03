using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPServer
{
    public class AsyncServer
    {
        private readonly HttpListener _listener;
        volatile int total_reqs;

        public DateTime BootTime { get; private set; }
        public List<IRoute> Routes { get; set; }

        public AsyncServer(params string[] prefixes)
        {
            _listener = new HttpListener();
            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            Routes = new List<IRoute>()
            {
                new RouteMatchAction
                {
                    MatchString = "info",
                    MatchRule   = RouteMatch.RouteMatchRule.Basic,
                    Priority    = int.MaxValue,
                    IsAsync     = true,
                    ActionAsync = GetInfo
                }
            };
        }

        public async Task Start()
        {
            Routes = Routes.OrderBy((r) => r.Priority).ToList();

            _listener.Start();

            BootTime = DateTime.UtcNow.ToUniversalTime();

            try
            {
                while (_listener != null && _listener.IsListening)
                {
                    var httpctx = await _listener.GetContextAsync();
#pragma warning disable CS4014
                    ProcessContext(httpctx);
#pragma warning restore CS4014
                }
            }catch(Exception except)
            {
                Console.WriteLine("[Exception::Start]");
                Console.WriteLine(except);
            }
        }

        async Task ProcessContext(HttpListenerContext httpContext)
        {
            //total_reqs++;
            try
            {
                //foreach (var ro in Routes)
                for(int x = 0; x < Routes.Count; x++)
                {
                    var ro = Routes[x];
                    if (ro.IsAsync)
                    {
                        if (await ro.ExecuteAsync(httpContext)) break;
                    }
                    else
                    {
                        if (ro.Execute(httpContext)) break;
                    }
                }
            }
            catch (Exception ex)
            {
                ReturnException(httpContext.Response, ex);
            }
            finally
            {
                httpContext.Response.Close();
            }
        }

        async Task GetInfo(HttpListenerContext httpContext)
        {
            var workers = 0;
            var asyncio = 0;
            ThreadPool.GetMaxThreads(out workers, out asyncio);

            if(workers != Environment.ProcessorCount)
            {
                var pcout = Environment.ProcessorCount;
                ThreadPool.SetMinThreads(pcout, pcout);
                ThreadPool.SetMaxThreads(pcout, pcout);
            }
            
            var now = DateTime.UtcNow.ToUniversalTime();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("System booted at {0}, it is now {1}\n", BootTime, now);
            sb.AppendFormat("Thats {0} total seconds!\n", (now - BootTime).TotalSeconds);
            sb.AppendFormat("There are {0} workers and {1} async I/O threads, there are {2} processors.\n", workers, asyncio, Environment.ProcessorCount);
            if (workers != Environment.ProcessorCount) sb.Append("Attempted to force the thread count.");
            sb.AppendFormat("There have been {0} total requests\n", total_reqs);

            await WriteBuffer(httpContext.Response, sb.ToString());
        }

        async Task WriteBuffer(HttpListenerResponse resp, string content)
        {
            byte[] buf = Encoding.UTF8.GetBytes(content);
            resp.ContentLength64 = buf.Length;
            await resp.OutputStream.WriteAsync(buf, 0, buf.Length);
        }

        void ReturnException(HttpListenerResponse resp, Exception ex)
        {
            try
            {
                resp.StatusCode = (int)HttpStatusCode.InternalServerError;
                resp.ContentType = "application/json; charset=utf-8";
                resp.ContentEncoding = Encoding.UTF8;

                byte[] buf = Encoding.UTF8.GetBytes(ex.ToString());
                resp.ContentLength64 = buf.Length;
                resp.OutputStream.Write(buf, 0, buf.Length);
            }
            catch (Exception except)
            {
                //log.Error(ex);
                Console.WriteLine("[Exception::ReturnException]");
                Console.WriteLine(except);
            }
        }

    }
}