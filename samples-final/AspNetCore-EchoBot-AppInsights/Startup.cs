// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.BotFramework;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Bot.Builder.Integration;
using System.Collections.Generic;
using AspNetCore_EchoBot_With_AppInsights.AppInsights;
using Microsoft.Bot.Builder.Ai.Luis;

namespace AspNetCore_EchoBot_With_AppInsights
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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
            // Bind appsettings.json values to a class that can be injected into Index.cshtml
            services.Configure<ApplicationConfiguration>(Configuration);

            services.AddMvc();

            services.AddBot<MyAppInsightsBot>(options =>
            {
                // Add MyAppInsightsLoggerMiddleware (logs activity messages into Application Insights)
                var instrumentationKey = Configuration.GetSection("AppInsights-InstrumentationKey")?.Value;
                var appInsightsLogger = new MyAppInsightsLoggerMiddleware(instrumentationKey, logUserName: true, logOriginalMessage: true);
                options.Middleware.Add(appInsightsLogger);


                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, anything stored in memory will be gone. 
                IStorage dataStore = new MemoryStorage();
                // For production Azure CosmosDB or Azure Blob storage provides storage as seen below. 
                // To include add the Microsoft.Bot.Builder.Azure Nuget package to your solution. That package is found at:
                //      https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/
                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.CosmosDbStorage("AzureTablesConnectionString", "TableName");
                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage("AzureBlobConnectionString", "containerName");

                // Create User State object.
                // The User State object is where we persist anything at the user-scope (note: the definition of a user
                // is channel specific).
                // The User and Conversation state objects are very commonly used.  Custom state objects can also be 
                // created.
                //
                // NOTE: State Property Accessors that are required for Middleware components *could* be built here
                // for passing into Middleware construction below.
                // In this particular sample, there are no components that are passed state property accessors in the 
                // Startup.ConfigureServices() method.  
                // However, all state property accessors are built and passed to the IBot-derived class via Asp.net Direct
                // Injection via the Singleton defined below (MyBotAccessor).
                var conversationState = new ConversationState(dataStore);

                // Add to State Object to options State collection.
                /// Objects in the State list enable other components to get access to the state providers
                /// during the start up process.  For example, creating state property accessors within a ASP.net Core Singleton
                /// that could be passed to your IBot-derived class (See MyAppInsightsBotAccessors below).
                /// The providers in this list are not associated with the BotStateSet Middleware component. To clarify, state providers
                /// in this list are not automatically loaded or saved during the turn process.
                options.State.Add(conversationState);

                // Add State to BotStateSet Middleware (that require auto-save)
                // The BotStateSet Middleware forces state storage to auto-save when the Bot is complete processing the message.
                // Note: Developers may choose not to add all the State providers to this Middleware if save is not required.
                var stateSet = new BotStateSet(options.State.ToArray());
                options.Middleware.Add(stateSet);
            });

            // Now that the bot is registered, create and register any state accesssors. 
            // These accessors are passed into the IBot-derived class (MyAppInsightsBot) on every turn. 
            services.AddSingleton<MyAppInsightsBotAccessors>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the State Accessors");
                }

                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                if (conversationState == null)
                {
                    throw new InvalidOperationException("UserState must be defined and added before adding user-scoped state accessors.");
                }

                // Create Custom State Property Accessors
                // State Property Accessors enable components to read and write individual properties, without having to 
                // pass the entire State object.
                var accessors = new MyAppInsightsBotAccessors
                {
                    CounterState = conversationState.CreateProperty<CounterState>(MyAppInsightsBotAccessors.CounterName),
                    ConversationDialogState = conversationState.CreateProperty<Dictionary<string, object>>(MyAppInsightsBotAccessors.DialogStateName),
                };

                return accessors;
            });

            // Create a LUIS Recognizer that is initialized and suitable for passing
            // into the IBot-derived class (MyAppInsightsBot) on each turn. 
            // This custom recognizer also logs results to Application Insights.
            services.AddSingleton<MyAppInsightLuisRecognizer>(sp =>
            {
                var applicationId = Configuration.GetSection("Luis-ApplicationId")?.Value;
                var endpointKey = Configuration.GetSection("Luis-EndpointKey")?.Value;
                var azureRegion = Configuration.GetSection("Luis-AzureRegion")?.Value;

                if (string.IsNullOrWhiteSpace(applicationId))
                {
                    throw new InvalidOperationException("The Luis ApplicationId ('Luis-ApplicationId') is required to run this sample.  Please update your appsettings.json");
                }

                if (string.IsNullOrWhiteSpace(endpointKey))
                {
                    throw new InvalidOperationException("The Luis endpoint key ('Luis-EndpointKey') is required to run this sample.  Please update your appsettings.json");
                }

                if (string.IsNullOrWhiteSpace(azureRegion))
                {
                    throw new InvalidOperationException("The Luis Azure Region ('Luis-AzureRegion') is required to run this sample.  Please update your appsettings.json");
                }

                var app = new LuisApplication(applicationId, endpointKey, azureRegion);
                var recognizer = new MyAppInsightLuisRecognizer(app);

                return recognizer;
            });

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }

    // This class is used to retrieve strings from appsettings.json on demand
    public class ApplicationConfiguration
    {
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }
    }
}
