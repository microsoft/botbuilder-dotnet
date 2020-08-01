// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    public class GetBookingDetailsDialog : CancelAndHelpDialog
    {
        public GetBookingDetailsDialog()
            : base(nameof(GetBookingDetailsDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new DateResolverDialog());
            var steps = new WaterfallStep[]
            {
                DestinationActionAsync,
                OriginActionAsync,
                TravelDateActionAsync,
                FinalActionAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }

        private JObject GetBookingDetails(WaterfallStepContext stepContext)
        {
            return stepContext.Options is JObject ? stepContext.Options as JObject : JObject.FromObject(stepContext.Options);
        }

        private async Task<DialogTurnResult> DestinationActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = GetBookingDetails(stepContext);
            var destination = bookingDetails.Value<string>("Destination");

            if (destination == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Where would you like to travel to?") }, cancellationToken);
            }

            return await stepContext.NextAsync(destination, cancellationToken);
        }

        private async Task<DialogTurnResult> OriginActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = GetBookingDetails(stepContext);
            bookingDetails["Destination"] = (string)stepContext.Result;

            var origin = bookingDetails.Value<string>("Origin");
            if (origin == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Where are you traveling from?") }, cancellationToken);
            }

            return await stepContext.NextAsync(origin, cancellationToken);
        }

        private async Task<DialogTurnResult> TravelDateActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = GetBookingDetails(stepContext);
            bookingDetails["Origin"] = (string)stepContext.Result;

            var travelDate = bookingDetails.Value<string>("TravelDate");
            if (travelDate == null || IsAmbiguous(travelDate))
            {
                // Run the DateResolverDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), travelDate, cancellationToken);
            }

            return await stepContext.NextAsync(travelDate, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = GetBookingDetails(stepContext);
            bookingDetails["TravelDate"] = (string)stepContext.Result;

            // We are done collection booking  details, return the data to the caller.
            return await stepContext.EndDialogAsync(stepContext.Options, cancellationToken);
        }
    }
}
