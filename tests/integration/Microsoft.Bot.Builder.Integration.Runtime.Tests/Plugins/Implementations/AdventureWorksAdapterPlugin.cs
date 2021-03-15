// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Tests.Plugins.TestComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins.Implementations
{
    public class AdventureWorksAdapterPlugin : IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            IServiceCollection services = context.Services;
            IConfiguration configuration = context.Configuration;

            var options = configuration.Get<AdventureWorksAdapterOptions>();

            if (options != null)
            {
                services.AddSingleton<IBotFrameworkHttpAdapter>(new AdventureWorksAdapter(options));
            }
        }
    }
}
