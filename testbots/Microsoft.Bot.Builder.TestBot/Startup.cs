// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Extensions.DependencyInjection;
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

            // Scenario 1 - this just adds everything that a bot needs (Authentication, HttpAdapter, and Storage).
            // if the developer wants to override these they need to use service.Add... to add their own desired
            // implementations for these base services before calling AddBotRuntime.
            // The extension method AddBotRuntime will register common services to the service collection.

            //services.AddBotRuntime(Configuration);

            // Scenario 2 - here the user adds each service they need in a fluent style.
            // Use the fluent extension methods to register needed services for the bot. Here the developer needs to add
            // each required service by adding a .Use... for each service they need. They can use the basic extension method
            // to get the default service, if they want to override they use another extension method where they can provide the 
            // type of the service they want to register.
            services.UseBotConfiguration(Configuration)
                    .UseBotAuthentication()
                    .UseBotHttpAdapter()
                    .UseBotStorage<MemoryStorage>()
                    .UseBotState<UserState>() // or .UseBotUserState()
                    .UseBotState<ConversationState>() // or .UseBotConversationState()
                    .UseBotDialog<MainDialog>()
                    .UseBotDialog<BookingDialog>(new BookingDialog(new GetBookingDetailsDialog(), new FlightBookingService()))
                    .UseBot<MyBot>()
                    .UseBot<DialogBot<MainDialog>>()
                    .UseBot<DialogAndWelcomeBot<MainDialog>>();

            // Register LUIS recognizer
            RegisterLuisRecognizers(services);

            // Register dialogs that will be used by the bot.
            //RegisterDialogs(services);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //services.AddScoped<MyBot>();
            //services.AddScoped<DialogBot<MainDialog>>();
            //services.AddScoped<DialogAndWelcomeBot<MainDialog>>();

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
