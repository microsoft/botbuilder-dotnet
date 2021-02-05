using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: HostingStartup(typeof(Contoso.CustomConfigAdapter.HostingStartup))]

namespace Contoso.CustomConfigAdapter
{
    public class HostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddMvc();

                services.AddOptions()
                    .Configure<ContosoAdapterOptions>(context.Configuration.GetSection(ContosoAdapter.Kind))
                    .AddTransient<ContosoAdapter>()
                    .AddTransient<ContosoController>();
            });

            builder.Configure((context, app) =>
            {
                var adapterOptions = app.ApplicationServices.GetService<IOptions<ContosoAdapterOptions>>()?.Value;

                app
                    .UseRouting()
                    .UseEndpoints(endpoints => endpoints.MapControllerRoute(ContosoAdapter.Kind, adapterOptions.ApiPath, new { controller = "Contoso", action = "Post" }));
            });
        }
    }
}
