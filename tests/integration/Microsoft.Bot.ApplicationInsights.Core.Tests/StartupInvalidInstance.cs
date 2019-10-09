// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    internal class StartupInvalidInstance
    {
        public StartupInvalidInstance(IHostingEnvironment env)
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
            var botConfig = BotConfiguration.Load("testbot.bot", null);
            services.AddBotApplicationInsights(botConfig, "invalidinstance");

            // Adding IConfiguration in sample test server.  Otherwise this appears to be
            // registered.
            services.AddSingleton<IConfiguration>(this.Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseBotApplicationInsights();
            var telemetryClient = app.ApplicationServices.GetService<IBotTelemetryClient>();
            Assert.IsNotNull(telemetryClient);
            Assert.IsTrue(telemetryClient is NullBotTelemetryClient);
        }
    }
}
