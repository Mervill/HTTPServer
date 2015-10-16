using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var pcout = Environment.ProcessorCount;
            ThreadPool.SetMinThreads(pcout, pcout);
            ThreadPool.SetMaxThreads(pcout, pcout);

            var server = new AsyncServer("http://+:8080/");
            server.Routes.Add(new RouteMatchAction {
                MatchString = "example",
                MatchRule = RouteMatch.RouteMatchRule.Basic,
                Priority = int.MaxValue,
                IsAsync = true,
                ActionAsync = GetExample
            });

            Task.Run(() => server.Start()).Wait();
        }

        static async Task GetExample(HttpListenerContext httpContext)
        {
            var source = await SimpleGet.GetAsync("http://www.example.com");
            byte[] buf = Encoding.UTF8.GetBytes(source);
            var resp = httpContext.Response;
            resp.ContentLength64 = buf.Length;
            await resp.OutputStream.WriteAsync(buf, 0, buf.Length);
        }
    }
}
