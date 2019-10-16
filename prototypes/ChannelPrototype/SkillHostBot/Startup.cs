// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills.Preview;
using Microsoft.Bot.Builder.Skills.Preview.Adapters;
using Microsoft.Bot.Builder.Skills.Preview.Integration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillHost.Bots;

namespace SkillHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Configure credentials
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the Bot Framework Adapter with error handling enabled.
            var botAdapter = new AdapterWithErrorHandler(Configuration, null);
            services.AddSingleton<BotAdapter>(botAdapter);
            services.AddSingleton<BotFrameworkHttpAdapter>(botAdapter);
            services.AddSingleton<BotFrameworkSkillAdapter>();

            services.AddSingleton((s) => (SkillAdapter)s.GetService<BotFrameworkSkillAdapter>());

            services.AddSingleton<BotFrameworkHttpSkillsServer>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, SkillHostBot>();

            // force this to be resolved
            var skillAdapter = services.BuildServiceProvider().GetService<BotFrameworkSkillAdapter>();
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
    }
}

