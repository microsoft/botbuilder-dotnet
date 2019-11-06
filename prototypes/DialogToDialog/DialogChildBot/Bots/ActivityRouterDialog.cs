// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DialogChildBot.Dialogs;
using DialogChildBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DialogChildBot.Bots
{
    /// <summary>
    /// A root dialog that can route activities sent to the skill based on the ones sent by a skill host.
    /// </summary>
    public class ActivityRouterDialog : ComponentDialog
    {
        private readonly DialogSkillBotRecognizer _luisRecognizer;

        public ActivityRouterDialog(DialogSkillBotRecognizer luisRecognizer, IConfiguration configuration)
            : base("MyNameIsGroot")
        {
            _luisRecognizer = luisRecognizer;

            AddDialog(new BookingDialog());
            AddDialog(new OAuthTestDialog(configuration));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { ProcessActivityAsync }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ProcessActivityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // A skill can send trace activities if needed :)
            await stepContext.Context.SendTraceActivityAsync($"{GetType().Name}.ProcessActivityAsync()", value: $"Got ActivityType: {stepContext.Context.Activity.Type}", cancellationToken: cancellationToken);

            switch (stepContext.Context.Activity.Type)
            {
                case ActivityTypes.Message:
                    return await ProcessMessageAsync(stepContext, cancellationToken);

                case ActivityTypes.Invoke:
                    return await ProcessInvokeAsync(stepContext, cancellationToken);

                case ActivityTypes.Event:
                    return await ProcessEventAsync(stepContext, cancellationToken);

                default:
                    // We didn't get an activity type we can handle.
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Unrecognized ActivityType: \"{stepContext.Context.Activity.Type}\"."), cancellationToken);
                    return new DialogTurnResult(DialogTurnStatus.Complete);
            }
        }

        private async Task<DialogTurnResult> ProcessEventAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"**{GetType().Name}.ProcessEventAsync().**\r\nName: {activity.Name}. Value: {GetObjectAsJsonString(activity.Value)}"), cancellationToken);

            // Resolve what to execute based on the event name.
            switch (activity.Name)
            {
                case "BookFlight":
                    var bookingDetails = new BookingDetails();
                    if (activity.Value != null)
                    {
                        bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(JsonConvert.SerializeObject(activity.Value));
                    }

                    // Start the booking dialog
                    var bookingDialog = FindDialog(nameof(BookingDialog));
                    return await stepContext.BeginDialogAsync(bookingDialog.Id, bookingDetails, cancellationToken);

                case "OAuthTest":
                    // Start the OAuthTestDialog
                    var oAuthDialog = FindDialog(nameof(OAuthTestDialog));
                    return await stepContext.BeginDialogAsync(oAuthDialog.Id, null, cancellationToken);

                default:
                    // We didn't get an event name we can handle.
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Unrecognized EventName: \"{activity.Name}\"."), cancellationToken);
                    return new DialogTurnResult(DialogTurnStatus.Complete);
            }
        }

        private async Task<DialogTurnResult> ProcessInvokeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"**{GetType().Name}.ProcessInvokeAsync().**\r\nName: {activity.Name}. Value: {GetObjectAsJsonString(activity.Value)}"), cancellationToken);

            // Resolve what to execute based on the invoke name.
            switch (activity.Name)
            {
                case "GetWeather":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Getting your weather forecast..."), cancellationToken);

                    // Create and return an invoke activity with the weather results.
                    var invokeResponseActivity = new Activity(type: "invokeResponse")
                    {
                        Value = new InvokeResponse
                        {
                            Body = new[] { "New York, NY, Clear, 56 F", "Bellevue, WA, Mostly Cloudy, 48 F" },
                            Status = (int)HttpStatusCode.OK
                        }
                    };
                    await stepContext.Context.SendActivityAsync(invokeResponseActivity, cancellationToken);
                    break;

                default:
                    // We didn't get an invoke name we can handle.
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Unrecognized InvokeName: \"{activity.Name}\"."), cancellationToken);
                    break;
            }

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        // This method just gets a message activity and runs it through LUIS. 
        // A developer can chose to start a dialog based on the LUIS results (not implemented here).
        private async Task<DialogTurnResult> ProcessMessageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"**{GetType().Name}.ProcessMessageAsync().**\r\nText: \"{activity.Text}\". Value: {GetObjectAsJsonString(activity.Value)}"), cancellationToken);

            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file."), cancellationToken);
            }
            else
            {
                // Call LUIS with the utterance.
                var luisResult = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);

                // Create a message showing the LUIS results.
                var sb = new StringBuilder();
                sb.AppendLine($"LUIS results for \"{activity.Text}\":");
                var (intent, intentScore) = luisResult.Intents.FirstOrDefault(x => x.Value.Equals(luisResult.Intents.Values.Max()));
                sb.AppendLine($"Intent: \"{intent}\" Score: {intentScore.Score}");
                sb.AppendLine($"Entities found: {luisResult.Entities.Count - 1}");
                foreach (var luisResultEntity in luisResult.Entities)
                {
                    if (!luisResultEntity.Key.Equals("$instance"))
                    {
                        sb.AppendLine($"* {luisResultEntity.Key}");
                    }
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(sb.ToString()), cancellationToken);
            }

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        private string GetObjectAsJsonString(object value)
        {
            return value == null ? string.Empty : JsonConvert.SerializeObject(value);
        }
    }
}
