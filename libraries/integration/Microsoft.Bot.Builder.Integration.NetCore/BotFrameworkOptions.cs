using Microsoft.AspNetCore.Http;
using BotMiddleware = Microsoft.Bot.Builder.Middleware;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    public class BotFrameworkOptions
    {
        public BotFrameworkOptions()
        {
            RouteBaseUrl = "/bots";
            Middleware = new List<BotMiddleware.IMiddleware>();
        }

        public PathString RouteBaseUrl { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationPassword { get; set; }
        public List<BotMiddleware.IMiddleware> Middleware { get; }

    }
}
