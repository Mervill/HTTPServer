using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPServer
{
    public abstract class RouteMatch : IRoute
    {
        public enum RouteMatchRule { Basic, RegEx }

        public string MatchString { get; set; }
        public RouteMatchRule MatchRule { get; set; }

        public int Priority { get; set; }
        public bool IsAsync { get; set; }
        
        public bool Execute(HttpListenerContext context)
        {
            if (IsMatch(context))
            {
                OnMatch(context);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ExecuteAsync(HttpListenerContext context)
        {
            if (IsMatch(context))
            {
                await OnMatchAsync(context);
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IsMatch(HttpListenerContext context)
        {
            var localPath = context.Request.Url.LocalPath;
            bool matches = false;
            switch (MatchRule)
            {
                case RouteMatchRule.Basic:
                    matches = localPath.EndsWith(MatchString);
                    break;
            }
            return matches;
        }

        protected abstract void OnMatch(HttpListenerContext context);

        protected abstract Task OnMatchAsync(HttpListenerContext context);
    }
}
