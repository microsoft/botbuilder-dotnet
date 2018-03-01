using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    internal class BotBuilder : IBotBuilder
    {
        private readonly IServiceCollection _serviceCollection;

        public BotBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public List<IMiddleware> Middleware => new List<IMiddleware>();
    }
}
