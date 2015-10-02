using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPServer
{
    public class RouteMatchAction : RouteMatch
    {
        public Action<HttpListenerContext> Action;
        public Func<HttpListenerContext, Task> ActionAsync;

        protected override void OnMatch(HttpListenerContext context)
        {
            if (Action != null) { Action(context); return; }
            throw new InvalidOperationException("There is no action defined for this route!");
        }

        protected override async Task OnMatchAsync(HttpListenerContext context)
        {
            if (ActionAsync != null) { await ActionAsync(context); return; }
            throw new InvalidOperationException("There is no action defined for this route!");
        }
    }
}
