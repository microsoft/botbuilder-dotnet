// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using DialogChildBot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace DialogChildBot.Bots
{
    // TODO: cleanup this dialog
    public class ActionRouterDialog : ComponentDialog
    {
        public ActionRouterDialog()
            : base("MyNameIsGroot")
        {
            AddDialog(new BookingDialog());
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = new CancellationToken())
        {
            // Do I have an active dialog?
            // IF Yes, continue with what I got
            // If No
            //   Did I get an action?
            //   If yes
            //      Run that action
            //   If No
            //      Did I get an utterance?
            //      If yes, resolve against LUIS
            var turnContext = innerDc.Context;
            var activity = turnContext.Activity;
            if (activity.Type == ActivityTypes.Event)
            {
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
                        return await innerDc.BeginDialogAsync(dialog.Id, bookingDetails, cancellationToken);

                    case "GetWeather":
                        // This is not done yet, should a a couple of debug messages and end right away.
                        await turnContext.SendActivityAsync(MessageFactory.Text("TODO: This will handle GetWeather flow"), cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Complete);

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
