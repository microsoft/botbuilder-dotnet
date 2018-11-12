// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.TestBot
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                IStorage dataStore = new MemoryStorage();

                var conversationState = new ConversationState(dataStore);

                var accessors = new TestBotAccessors
                {
                    ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                    ConversationState = new ConversationState(dataStore)
                };

                return accessors;
            });

            services.AddSingleton<IAdapterIntegration>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var accessors = sp.GetRequiredService<TestBotAccessors>();

                options.Middleware.Add(new AutoSaveStateMiddleware(accessors.ConversationState));
                options.Middleware.Add(new ShowTypingMiddleware());

                var botFrameworkAdapter = new BotFrameworkAdapter(options.CredentialProvider, options.ChannelProvider, options.ConnectorClientRetryPolicy, options.HttpClient)
                {
                    OnTurnError = options.OnTurnError,
                };

                foreach (var middleware in options.Middleware)
                {
                    botFrameworkAdapter.Use(middleware);
                }

                //return botFrameworkAdapter;

                return new InteceptorAdapter(botFrameworkAdapter);
            });

            services.AddBot<TestBot>();

            //services.AddBot<TestBot>(options =>
            //{
            //    IStorage dataStore = new MemoryStorage();
            //    options.State.Add(new ConversationState(dataStore));
            //    options.Middleware.Add(new AutoSaveStateMiddleware(options.State.ToArray()));
            //    options.Middleware.Add(new ShowTypingMiddleware());
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
