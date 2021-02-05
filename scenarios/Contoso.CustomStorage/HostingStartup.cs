using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: HostingStartup(typeof(Contoso.CustomStorage.HostingStartup))]

namespace Contoso.CustomStorage
{
    public class HostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Override -> Carlos, Memory
                //services.AddSingleton<IStorage, CarlosDbStorage>();
                //services.
                //services.Replace(ServiceDescriptor.Singleton<IStorage>())
            });
        }
    }
}
