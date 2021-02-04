// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Tests.Plugins.TestComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins.Implementations
{
    public class MultiAdapterPlugin : IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            IServiceCollection services = context.Services;
            IConfiguration configuration = context.Configuration;

            // ContosoAdapter
            var contosoSection = configuration.GetSection(typeof(ContosoAdapter).FullName);

            if (contosoSection.Exists())
            {
                services.AddSingleton<ContosoAdapter>(sp => new ContosoAdapter(contosoSection));
            }

            // AdventureWorksAdapter
            var adventureWorksSection = configuration.GetSection(typeof(AdventureWorksAdapter).FullName);

            if (adventureWorksSection.Exists())
            {
                var options = adventureWorksSection.Get<AdventureWorksAdapterOptions>();
                services.AddSingleton<AdventureWorksAdapter>(new AdventureWorksAdapter(options));
            }
        }
    }
}
