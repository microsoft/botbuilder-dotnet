using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotBuilderSamples;
using Microsoft.BotBuilderSamples.Bots;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(CoreFunctionBot.Startup))]

namespace CoreFunctionBot
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            builder.Services.AddSingleton<IStorage>(new MemoryStorage());

            // Create the User state. (Used in this bot's Dialog implementation.)
            builder.Services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            builder.Services.AddSingleton<ConversationState>();

            // Register LUIS recognizer
            builder.Services.AddSingleton<FlightBookingRecognizer>();

            // Register the BookingDialog.
            builder.Services.AddSingleton<BookingDialog>();

            // The MainDialog that will be run by the bot.
            builder.Services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            builder.Services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();

            // Use this startup code if using the V5 AddBotRuntime startup method

            //// Register LUIS recognizer
            //builder.Services.AddSingleton<FlightBookingRecognizer>();

            //builder.Services.AddSingleton<IBotFrameworkHttpAdapter, FunctionBotAdapter>();

            //// Register the BookingDialog.
            //builder.Services.AddSingleton<BookingDialog>();
            //builder.Services.AddBotRuntime<DialogAndWelcomeBot<MainDialog>, MainDialog>(builder.GetContext().Configuration);
        }
    }
}
