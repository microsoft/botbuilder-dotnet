// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.CognitiveModels;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : ComponentDialog
    {
        private readonly IConfiguration _configuration;
        private readonly IntentDialogMap _intentsAndDialogs;
        private readonly ILogger _logger;
        private readonly IRecognizer _luisRecognizer;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger, IRecognizer luisRecognizer, IntentDialogMap intentsAndDialogs)
            : base(nameof(MainDialog))
        {
            _configuration = configuration;
            _logger = logger;
            _luisRecognizer = luisRecognizer;

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Add dialogs for intents
            _intentsAndDialogs = intentsAndDialogs;
            foreach (var dialog in intentsAndDialogs.Values)
            {
                AddDialog(dialog);
            }

            // Create and add waterfall for main conversation loop
            var steps = new WaterfallStep[]
            {
                PromptForTaskStepAsync,
                InvokeTaskStepAsync,
                FinalStepAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptForTaskStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_configuration["LuisAppId"]) || string.IsNullOrEmpty(_configuration["LuisAPIKey"]) || string.IsNullOrEmpty(_configuration["LuisAPIHostName"]))
            {
                var activity = MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.");
                activity.InputHint = InputHints.IgnoringInput;
                await stepContext.Context.SendActivityAsync(activity, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var promptText = stepContext.Options?.ToString() ?? "What can I help you with today?";

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptText) }, cancellationToken);
        }

        private async Task<DialogTurnResult> InvokeTaskStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);

            switch (luisResult.TopIntent().intent)
            {
                case FlightBooking.Intent.BookFlight:
                    // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
                    var bookingDetails = await _luisRecognizer.RecognizeAsync<BookingDetails>(stepContext.Context, cancellationToken);

                    // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

                case FlightBooking.Intent.GetWeather:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    await stepContext.Context.SendActivityAsync("TODO: get weather flow here", cancellationToken: cancellationToken);
                    break;

                default:
                    // Catch all for unhandled intents
                    await stepContext.Context.SendActivityAsync($"Sorry Dave, I didn't get that (intent was {luisResult.TopIntent().intent})", cancellationToken: cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // We have completed the task (or the user cancelled), we restart main dialog with a different prompt text.
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(Id, promptMessage, cancellationToken);

            //// If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
            //if (stepContext.Result != null)
            //{
            //    var result = (BookingDetails)stepContext.Result;

            //    // Now we have all the booking details call the booking service.

            //    // If the call to the booking service was successful tell the user.
            //    var timeProperty = new TimexProperty(result.TravelDate);
            //    var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            //    var msg = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
            //    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            //}
            //else
            //{
            //    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            //}

            //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
