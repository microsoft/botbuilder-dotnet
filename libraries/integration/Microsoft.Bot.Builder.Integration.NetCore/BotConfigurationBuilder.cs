// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    internal class BotConfigurationBuilder : IBotConfigurationBuilder
    {
        private readonly List<IMiddleware> _middleware = new List<IMiddleware>();
        private readonly IServiceCollection _serviceCollection;

        public BotConfigurationBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public List<IMiddleware> Middleware { get => _middleware; }
    }
}
