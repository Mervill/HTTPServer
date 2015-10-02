using System;
using System.Collections.Generic;
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
            ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
            var server = new AsyncServer("http://+:8080/");
            Task.Run((Func<Task>)server.Start).Wait();
        }
    }
}
