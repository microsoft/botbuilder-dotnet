using System;   
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Contoso.CustomAdapter
{
    public class AdventureWorksAdapterPlugin : IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            IServiceCollection services = context.Services;
            IConfiguration configuration = context.Configuration;

            //if (configuration.GetSection("Contoso.CustomAdapter.AdventureWorksAdapter").Exists())
            {
                services.AddSingleton<IBotFrameworkHttpAdapter, AdventureWorksAdapter>(sp => new AdventureWorksAdapter(configuration));
                services.AddSingleton<BotAdapter>(new AdventureWorksAdapter(configuration));
                services.AddSingleton<AdventureWorksAdapter>(sp => new AdventureWorksAdapter(configuration));
                services.AddSingleton<AdventureWorksAdapter>(new AdventureWorksAdapter(configuration));
                services.AddSingleton<IBotFrameworkHttpAdapter>(sp => new AdventureWorksAdapter(configuration));
                services.AddSingleton<IBotFrameworkHttpAdapter>(sp => new AdventureWorksAdapter(configuration));
            }
        }
    }
}
