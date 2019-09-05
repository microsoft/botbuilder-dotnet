// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Adapters.Webex.TestBot.Bots;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Adapters.Webex.TestBot
{
    public class Startup
    {
        private readonly WebexAdapter _adapter;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var options = new WebexAdapterOptions(configuration["AccessToken"], configuration["PublicAddress"], configuration["Secret"], configuration["WebhookName"]);
            _adapter = new WebexAdapter(options, new WebexClientWrapper());

            RegisterWebhookAsync().Wait();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the options for the WebexAdapter
            services.AddSingleton<WebexAdapterOptions, ConfigurationWebexAdapterOptions>();

            // Create the Bot Framework Adapter.
            services.AddSingleton(_adapter);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, EchoBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // app.UseHttpsRedirection();
            app.UseMvc();
        }

        public async Task RegisterWebhookAsync()
        {
            var webhookList = await _adapter.ListWebhookSubscriptionsAsync();

            // await _adapter.ResetWebhookSubscriptionsAsync(webhookList);
            await _adapter.RegisterWebhookSubscriptionAsync("/api/messages", webhookList);
            await _adapter.RegisterAdaptiveCardsWebhookSubscriptionAsync("/api/messages", webhookList);
        }
    }
}
