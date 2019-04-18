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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

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
            bool enableDebugger = true;
            if (enableDebugger)
            {
                services.Configure<BotFrameworkOptions>(Configuration);
                services.AddSingleton<ILogger>(new DebugLogger(nameof(DebugAdapter)));
                // https://andrewlock.net/how-to-register-a-service-with-multiple-interfaces-for-in-asp-net-core-di/
                services.AddSingleton<SourceMap>();
                services.AddSingleton<Source.IRegistry>(x => x.GetRequiredService<SourceMap>());
                services.AddSingleton<IBreakpoints>(x => x.GetRequiredService<SourceMap>());
                services.AddSingleton<ICoercion, Coercion>();
                services.AddSingleton<IDataModel, DataModel>();
                // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-2.2#use-di-services-to-configure-options
                services.AddTransient<IConfigureOptions<BotFrameworkOptions>, ConfigureDebugOptions>();
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
                    return new TestBot(userState, conversationState, resourceExplorer, sp.GetRequiredService<Source.IRegistry>());

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
                    var lg = new LGLanguageGenerator(resourceExplorer);
                    options.Middleware.Add(new RegisterClassMiddleware<ILanguageGenerator>(lg));
                    options.Middleware.Add(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)));
                    options.Middleware.Add(new IgnoreConversationUpdateForBotMiddleware());
                    options.Middleware.Add(new AutoSaveStateMiddleware(conversationState));
                });
        }

        private sealed class ConfigureDebugOptions : IConfigureOptions<BotFrameworkOptions>
        {
            public Action<BotFrameworkOptions> Configure{ get; }
            public ConfigureDebugOptions(IApplicationLifetime applicationLifetime, IDataModel dataModel, Source.IRegistry registry, IBreakpoints breakpoints, ILogger logger)
            {
                Configure = (options) =>
                {
                    // by setting the source registry all dialogs will register themselves to be debugged as execution flows
                    DebugSupport.SourceRegistry = registry;
                    var adapter = new DebugAdapter(options.DebugPort, dataModel, registry, breakpoints, applicationLifetime.StopApplication, logger);
                    options.Middleware.Add(adapter);
                };
            }
            void IConfigureOptions<BotFrameworkOptions>.Configure(BotFrameworkOptions options)
            {
                this.Configure(options);
            }
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
