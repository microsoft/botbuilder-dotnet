using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotFramework.Composer.Core
{
    public static class LuisConfigAdapter
    {
        public static IConfigurationBuilder UseLuisConfigAdapter(this IConfigurationBuilder builder)
        {
            var configuration = builder.Build();
            var settings = new Dictionary<string, string>();
            settings["environment"] = configuration.GetValue<string>("luis:environment");
            settings["region"] = configuration.GetValue<string>("luis:authoringRegion");
            builder.AddInMemoryCollection(settings);
            return builder;
        }
    }
}
