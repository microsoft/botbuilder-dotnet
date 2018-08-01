// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Ai.Luis;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.BotFramework;
using System.Linq;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Extensions.Options;

namespace AspNetCore_LUIS_Bot
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
            services.AddBot<MyLuisBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, anything stored in memory will be gone. 

                // The File data store, shown here, is suitable for bots that run on 
                // a single machine and need durable state across application restarts.                 
                // IStorage dataStore = new FileStorage(System.IO.Path.GetTempPath());

                // For production bots use the Azure Table Store, Azure Blob, or 
                // Azure CosmosDB storage provides, as seen below. To include any of 
                // the Azure based storage providers, add the Microsoft.Bot.Builder.Azure 
                // Nuget package to your solution. That package is found at:
                //      https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/

                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureTableStorage("AzureTablesConnectionString", "TableName");
                //IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage("AzureBlobConnectionString", "containerName");
                IStorage dataStore = new MemoryStorage();

                // *NEW* CREATE NEW CONVERSATION STATE                
                var userState = new UserState(dataStore);
                options.State.Add(userState);

                // Forces storage to auto-load on a new message, and auto-save when complete.  
                // Put at the beginning of the pipeline.
                var stateSet = new BotStateSet(options.State.ToArray());
                options.Middleware.Add(stateSet);                                                     
            });

            // Now that the bot is registered, create and register any state accesssors. 
            // These accessors are passed into the Bot on every turn. 
            services.AddSingleton<MyBotAccessors>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the State Accessors");
                }

                var accessors = new MyBotAccessors
                {
                    Reminders = options.UserState.CreateProperty<List<Reminder>>(MyBotAccessors.RemindersName, () => new List<Reminder>()),
                    UserDialogState = options.UserState.CreateProperty<Dictionary<string, object>>(MyBotAccessors.DialogStateName, () => new Dictionary<string, object>())
                };

                return accessors;
            });

            // Create a LUIS Recognizer that is initialized and suitable for passing
            // into the Bot on each turn. 
            services.AddSingleton<LuisRecognizer>(sp =>
            {
                var applicationId = Configuration.GetSection("Luis-ApplicationId")?.Value;
                var endpointKey = Configuration.GetSection("Luis-EndpointKey")?.Value;
                var azureRegion = Configuration.GetSection("Luis-AzureRegion")?.Value;

                if (string.IsNullOrWhiteSpace(applicationId))
                {
                    throw new InvalidOperationException("The Luis ApplicationId ('Luis-ApplicationId') is required to run this sample.");
                }

                if (string.IsNullOrWhiteSpace(endpointKey))
                {
                    throw new InvalidOperationException("The Luis endpoint key ('Luis-EndpointKey') is required to run this sample.");
                }

                if (string.IsNullOrWhiteSpace(azureRegion))
                {
                    throw new InvalidOperationException("The Luis Azure Region ('Luis-AzureRegion') is required to run this sample.");
                }

                var app = new LuisApplication(applicationId, endpointKey, azureRegion);
                var recognizer = new LuisRecognizer(app);

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

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }        
    }
}
