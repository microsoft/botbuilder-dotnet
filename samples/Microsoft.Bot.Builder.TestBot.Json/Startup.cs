// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Debugger;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TestBot.Json.Recognizers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugger;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            HostingEnvironment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            // set the configuration for types
            TypeFactory.Configuration = this.Configuration;

            // register adaptive library types
            TypeFactory.RegisterAdaptiveTypes();

            // register custom types
            TypeFactory.Register("Testbot.RuleRecognizer", typeof(RuleRecognizer));
        }

        public IHostingEnvironment HostingEnvironment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(this.Configuration);

            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);
            var userStateMap = userState.CreateProperty<Dictionary<string, object>>("user");
            var accessors = new TestBotAccessors
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                ConversationState = conversationState,
                UserState = userState
            };

            var sourceMap = new SourceMap();
            DebugAdapter adapter = null;
            bool enableDebugger = false;
            if (enableDebugger)
            {
                adapter = new DebugAdapter(sourceMap, sourceMap, new DebugLogger(nameof(DebugAdapter)));
            }

            // manage all bot resources
            var resourceExplorer = ResourceExplorer.LoadProject(HostingEnvironment.ContentRootPath);

            services.AddBot<IBot>(
                (IServiceProvider sp) =>
                {
                    // declarative Adaptive dialogs bot sample
                    return new TestBot(accessors, resourceExplorer, sourceMap);

                    // LG bot sample
                    // return new TestBotLG(accessors);
                },
                (BotFrameworkOptions options) =>
                {
                    options.OnTurnError = async (turnContext, exception) =>
                    {
                        await conversationState.ClearStateAsync(turnContext);
                        await conversationState.SaveChangesAsync(turnContext);
                    };

                    if (adapter != null)
                    {
                        options.Middleware.Add(adapter);
                    }

                    options.Middleware.Add(new RegisterClassMiddleware<IStorage>(dataStore));
                    options.Middleware.Add(new RegisterClassMiddleware<ResourceExplorer>(resourceExplorer));

                    var lg = new LGLanguageGenerator(resourceExplorer);
                    options.Middleware.Add(new RegisterClassMiddleware<ILanguageGenerator>(lg));
                    options.Middleware.Add(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)));

                    options.Middleware.Add(new AutoSaveStateMiddleware(conversationState));
                });
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
            app.UseExceptionHandler();
        }
    }
}
