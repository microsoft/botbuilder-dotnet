// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SendChildBot.Dialogs;

namespace SendChildBot.Bots
{
    public class ActionRouterDialog : ComponentDialog
    {
        public ActionRouterDialog()
            : base("MyNameIsRoot")
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
            if (activity.SemanticAction != null)
            {
                // Resolve what to execute based on the semantic action ID.
                await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {activity.Text}"), cancellationToken);
                switch (activity.SemanticAction.Id)
                {
                    case "BookFlight":
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Got Semantic Action: {activity.SemanticAction.Id}"), cancellationToken);

                        var bookingDetails = new BookingDetails();
                        if (activity.SemanticAction.Entities != null && activity.SemanticAction.Entities.ContainsKey("bookingInfo"))
                        {
                            var entity = activity.SemanticAction.Entities["bookingInfo"];
                            bookingDetails = entity.GetAs<BookingDetails>();
                            await turnContext.SendActivityAsync(MessageFactory.Text($"Got Entity: {entity.Type} {JsonConvert.SerializeObject(bookingDetails)}"), cancellationToken);
                        }

                        // Start the booking dialog
                        var dialog = FindDialog(nameof(BookingDialog));
                        return await innerDc.BeginDialogAsync(dialog.Id, bookingDetails, cancellationToken);

                    case "GetWeather":
                        // This is not done yet, should a a couple of debug messages and end right away.
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Got Semantic Action: {activity.SemanticAction.Id}"), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text("TODO: This will handle GetWeather flow"), cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Complete);
                }
            }

            // TODO: here we would need to resolve against LUIS
            await turnContext.SendActivityAsync(MessageFactory.Text($"Dunno what to do with what you said... (SkillBot)"), cancellationToken);

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }
    }
}
