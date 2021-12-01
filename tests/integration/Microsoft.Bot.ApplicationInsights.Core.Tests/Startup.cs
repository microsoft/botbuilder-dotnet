// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    /// <summary>
    /// Calls services.AddBotApplicationInsights() passing configuration (the most common scenario).
    /// </summary>
    internal class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBotApplicationInsights(Configuration);

            // Adding IConfiguration in sample test server.  Otherwise this appears to be
            // registered.
            services.AddSingleton(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            app.UseBotApplicationInsights();
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
