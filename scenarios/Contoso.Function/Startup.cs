using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Contoso.Function.Startup))]

namespace Contoso.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {                         
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder.AddInMemoryCollection(new[] { new KeyValuePair<string, string>(WebHostDefaults.HostingStartupAssembliesKey, "Contoso.CustomConfigAdapter") });
            base.ConfigureAppConfiguration(builder);
        }
    }
}
