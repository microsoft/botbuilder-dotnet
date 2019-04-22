// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.LanguageGeneration.Renderer;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Dialogs.Declarative;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            this.HostingEnvironment = env;
            this.Configuration = configuration;
        }

        public IHostingEnvironment HostingEnvironment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IConfiguration>(this.Configuration);

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            IStorage storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);
            var resourceExplorer = ResourceExplorer
                .LoadProject(HostingEnvironment.ContentRootPath);
            
            // TODO get rid of this dependency
            TypeFactory.Configuration = this.Configuration;

            // set up bot framework runtime environment (Aka the adapter)
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>((s) =>
            {
                var adapter = new BotFrameworkHttpAdapter();
                adapter
                    .UseStorage(storage)
                    .UseState()
                    .UseResourceExplorer(resourceExplorer, () =>
                    {
                        TypeFactory.Register("Testbot.CalculateDogYears", typeof(CalculateDogYears));
                        TypeFactory.Register("Testbot.JavascriptStep", typeof(JavascriptStep));
                        TypeFactory.Register("Testbot.CSharpStep", typeof(CSharpStep));
                    })
                    .UseLanguageGenerator(new LGLanguageGenerator(resourceExplorer))
                    .UseDebugger(Configuration.GetValue<int>("debugport", 4712));

                adapter.OnTurnError = async (turnContext, exception) =>
                {
                    await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);

                    await conversationState.ClearStateAsync(turnContext).ConfigureAwait(false);
                    await conversationState.SaveChangesAsync(turnContext).ConfigureAwait(false);
                };
                return adapter;
            });

            services.AddSingleton<IBot, TestBot>((sp) => new TestBot(conversationState, resourceExplorer, DebugSupport.SourceRegistry));
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

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

