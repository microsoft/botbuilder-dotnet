// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography.X509Certificates;
using AuthenticationBot;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Add this line to get an IHttpClientFactory in the services collection
            //services.AddHttpClient();

            // Create the Bot Framework Adapter with error handling enabled.
            //services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            //services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            //services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            //services.AddSingleton<ConversationState>();

            // The Dialog that will be run by the bot.
            //services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //services.AddTransient<IBot, AuthBot<MainDialog>>();
            //services.AddTransient<IBot, EchoBot>();

            services.AddSingleton<Dialog, Akhenaten>();

            services.AddBotRuntime(Configuration);

            // Un-comment the lines below to opt-in to MSAL auth.

            // MSAL secret auth:
            //services.AddSingleton<IConfidentialClientApplication>(
            //    serviceProvider => ConfidentialClientApplicationBuilder.Create(Configuration.GetSection("MicrosoftAppId").Value)
            //        .WithClientSecret(Configuration.GetSection("MicrosoftAppPassword").Value)
            //        .Build());

            // MSAL certificate auth:
            //services.AddSingleton<IConfidentialClientApplication>(
            //    serviceProvider => ConfidentialClientApplicationBuilder.Create(Configuration.GetSection("MicrosoftAppId").Value)
            //        .WithCertificate(new X509Certificate2("<path to cert file>", "certPassword"))
            //        .Build());

            // MSAL credential factory: regardless of secret, cert or custom auth, need to add the line below
            // to enable MSAL
            // services.AddSingleton<ServiceClientCredentialsFactory, MsalServiceClientCredentialsFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Add either of these to play with streaming
            //.UseNamedPipes()
            //.UseWebSockets()
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
