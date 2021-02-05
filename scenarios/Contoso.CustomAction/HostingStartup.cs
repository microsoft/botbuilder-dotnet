using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;

[assembly: HostingStartup(typeof(Contoso.CustomAction.HostingStartup))]

namespace Contoso.CustomAction
{
    public class HostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            ComponentRegistration.Add(new CustomComponentRegistration());
        }
    }
}
