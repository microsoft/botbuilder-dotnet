// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            //services.AddSingleton<IAdapterIntegration>(sp =>
            //{
            //    var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
            //    var accessors = sp.GetRequiredService<TestBotAccessors>();

            //    options.Middleware.Add(new AutoSaveStateMiddleware(accessors.ConversationState));
            //    options.Middleware.Add(new ShowTypingMiddleware());

            //    var botFrameworkAdapter = new BotFrameworkAdapter(options.CredentialProvider, options.ChannelProvider, options.ConnectorClientRetryPolicy, options.HttpClient)
            //    {
            //        OnTurnError = options.OnTurnError,
            //    };

            //    foreach (var middleware in options.Middleware)
            //    {
            //        botFrameworkAdapter.Use(middleware);
            //    }

            //    //return botFrameworkAdapter;

            //    return new InteceptorAdapter(botFrameworkAdapter);
            //});

            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);

            var accessors = new TestBotAccessors
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                ConversationState = conversationState
            };

            services.AddBot<IBot>(
                (IServiceProvider sp) =>
                {
                    return new TestBot(accessors);
                },
                (BotFrameworkOptions options) =>
                {
                    options.OnTurnError = async (turnContext, exception) =>
                    {
                        await conversationState.ClearStateAsync(turnContext);
                        await conversationState.SaveChangesAsync(turnContext);
                    };
                    options.Middleware.Add(new AutoSaveStateMiddleware(conversationState));
                });

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

            // NOTE: Uncomment this to force request buffering to test accessing the request body in buffered scenarios (default is always unbuffered)
            //app.Use(async (httpContext, next) =>
            //{
            //    httpContext.Request.EnableBuffering();

            //    await next();
            //});

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
