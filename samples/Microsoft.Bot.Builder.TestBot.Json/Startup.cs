// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.LanguageGeneration.Renderer;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Debug;
using System.Diagnostics;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            this.HostingEnvironment = env;
            this.Configuration = configuration;

            // set the configuration for types
            TypeFactory.Configuration = this.Configuration;

            // register adaptive library types
            TypeFactory.RegisterAdaptiveTypes();

            // register custom types
            TypeFactory.Register("Testbot.CalculateDogYears", typeof(CalculateDogYears));
            TypeFactory.Register("Testbot.JavascriptStep", typeof(JavascriptStep));
            TypeFactory.Register("Testbot.CSharpStep", typeof(CSharpStep));
        }

        public IHostingEnvironment HostingEnvironment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                TelemetryConfiguration.Active.DisableTelemetry = true;
            }

            // hook up debugging support
            var sourceMap = new SourceMap();
            DebugAdapter debugAdapter = null;
            bool enableDebugger = true;
            if (enableDebugger)
            {
                // by setting the source registry all dialogs will register themselves to be debugged as execution flows
                DebugSupport.SourceRegistry = sourceMap;
                var model = new DataModel(Coercion.Instance);
                var port = Configuration.GetValue<int>("debugport", 4712);
                Console.WriteLine($"Debugger listening on port:{port}");
                Console.WriteLine("     use --debugport # or use 'debugport' setting to change)");
                debugAdapter = new DebugAdapter(port, model, sourceMap, sourceMap, new DebugLogger(nameof(DebugAdapter)));
            }

            services.AddSingleton<IConfiguration>(this.Configuration);

            IStorage dataStore = new MemoryStorage();
            var userState = new UserState(dataStore);
            var conversationState = new ConversationState(dataStore);

            // manage all bot resources
            var resourceExplorer = ResourceExplorer.LoadProject(HostingEnvironment.ContentRootPath);

            services.AddBot<IBot>(
                (IServiceProvider sp) =>
                {
                    // declarative Adaptive dialogs bot sample
                    return new TestBot(userState, conversationState, resourceExplorer, DebugSupport.SourceRegistry);

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

                    options.CredentialProvider = new SimpleCredentialProvider(this.Configuration["AppId"], this.Configuration["AppPassword"]);
                    options.Middleware.Add(new RegisterClassMiddleware<IStorage>(dataStore));
                    options.Middleware.Add(new RegisterClassMiddleware<ResourceExplorer>(resourceExplorer));

                    if (debugAdapter != null)
                    {
                        options.Middleware.Add(debugAdapter);
                    }

                    var lg = new LGLanguageGenerator(resourceExplorer);
                    options.Middleware.Add(new RegisterClassMiddleware<ILanguageGenerator>(lg));
                    options.Middleware.Add(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)));
                    options.Middleware.Add(new IgnoreConversationUpdateForBotMiddleware());
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

public class IgnoreConversationUpdateForBotMiddleware : IMiddleware
{
    public Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
        {
            var cu = turnContext.Activity.AsConversationUpdateActivity();
            if (!cu.MembersAdded.Any(ma => ma.Id != cu.Recipient.Id))
            {
                // eat it if it is the bot
                return Task.CompletedTask;
            }
        }
        return next(cancellationToken);
    }
}
