// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TestBot.Shared.Bots;
using Microsoft.Bot.Builder.TestBot.Shared.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared.Services;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration that represents a set of key/value application configuration properties.
        /// </summary>
        /// <value>
        /// The <see cref="IConfiguration"/> that represents a set of key/value application configuration properties.
        /// </value>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddHttpClient();

            // Create the debug middleware
            services.AddSingleton(sp => new MicrosoftAppCredentials(sp.GetRequiredService<IConfiguration>()["MicrosoftAppId"], sp.GetRequiredService<IConfiguration>()["MicrosoftAppPassword"]));
            services.AddSingleton<InspectionMiddleware>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the App state. (Used by the DebugMiddleware.)
            services.AddSingleton<InspectionState>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Register LUIS recognizer
            RegisterLuisRecognizers(services);

            // Register dialogs that will be used by the bot.
            RegisterDialogs(services);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddScoped<MyBot>();
            services.AddScoped<DialogBot<MainDialog>>();
            services.AddScoped<DialogAndWelcomeBot<MainDialog>>();

            // We can also run the inspection at a different endpoint. Just uncomment these lines.
            // services.AddSingleton<DebugAdapter>();
            // services.AddTransient<DebugBot>();
            services.AddTransient<Func<string, IBot>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case "mybot":
                        return serviceProvider.GetService<MyBot>();
                    case "dialogbot":
                        return serviceProvider.GetService<DialogBot<MainDialog>>();
                    case "messages":
                    case "dialogandwelcomebot":
                        return serviceProvider.GetService<DialogAndWelcomeBot<MainDialog>>();
                    default:
                        return null;
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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

        private static void RegisterDialogs(IServiceCollection services)
        {
            // Register booking dialog
            services.AddSingleton(new BookingDialog(new GetBookingDetailsDialog(), new FlightBookingService()));

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainDialog>();
        }

        private void RegisterLuisRecognizers(IServiceCollection services)
        {
            var luisIsConfigured = !string.IsNullOrEmpty(Configuration["LuisAppId"]) && !string.IsNullOrEmpty(Configuration["LuisAPIKey"]) && !string.IsNullOrEmpty(Configuration["LuisAPIHostName"]);
            if (luisIsConfigured)
            {
                var luisApplication = new LuisApplication(
                    Configuration["LuisAppId"],
                    Configuration["LuisAPIKey"],
                    "https://" + Configuration["LuisAPIHostName"]);

                var recognizer = new LuisRecognizer(new LuisRecognizerOptionsV2(luisApplication));
                services.AddSingleton<IRecognizer>(recognizer);
            }
        }
    }
}
