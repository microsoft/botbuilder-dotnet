// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    // TODO: cleanup this dialog
    public class ActionRouterDialog : ComponentDialog
    {
        public ActionRouterDialog(IConfiguration configuration)
            : base("MyNameIsGroot")
        {
            AddDialog(new BookingDialog());
            AddDialog(new OAuthTestDialog(configuration));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { ProcessActivityAsync }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ProcessActivityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var turnContext = stepContext.Context;
            var activity = turnContext.Activity;
            if (activity.Type == ActivityTypes.Event)
            {
                // A skill can send trace activities y needed :)
                await turnContext.SendTraceActivityAsync($"{GetType().Name}", value: $"Got Event: {activity.Name}", cancellationToken: cancellationToken);

                // Resolve what to execute based on the semantic action ID.
                await turnContext.SendActivityAsync(MessageFactory.Text($"Got Event: {activity.Name}"), cancellationToken);
                switch (activity.Name)
                {
                    case "BookFlight":
                        var bookingDetails = new BookingDetails();
                        if (activity.Value != null)
                        {
                            bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(JsonConvert.SerializeObject(activity.Value));
                            await turnContext.SendActivityAsync(MessageFactory.Text($"Got Value parameter: {JsonConvert.SerializeObject(bookingDetails)}"), cancellationToken);
                        }

                        // Start the booking dialog
                        var dialog = FindDialog(nameof(BookingDialog));
                        return await stepContext.BeginDialogAsync(dialog.Id, bookingDetails, cancellationToken);

                    case "GetWeather":
                        // This is not done yet, should a a couple of debug messages and end right away.
                        await turnContext.SendActivityAsync(MessageFactory.Text("TODO: This will handle GetWeather flow"), cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Complete);

                    case "OAuthTest":
                        // Start the OAuthTestDialog
                        var oAuthDialog = FindDialog(nameof(OAuthTestDialog));
                        return await stepContext.BeginDialogAsync(oAuthDialog.Id, null, cancellationToken);

                    default:
                        await turnContext.SendActivityAsync(MessageFactory.Text("Unknown event name"), cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Complete);
                }
            }

            // Here we would need to resolve against LUIS or determine what else to do.
            await turnContext.SendActivityAsync(MessageFactory.Text($"Didn't get an event. We got an activity of type \"{activity.Type}\" and value is \"{JsonConvert.SerializeObject(activity.Value)}\"."), cancellationToken);
            if (activity.Type == ActivityTypes.Message)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"We need to run this through our own LUIS model. The activity text was \"{activity.Text}\"."), cancellationToken);
            }

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }
    }
}
