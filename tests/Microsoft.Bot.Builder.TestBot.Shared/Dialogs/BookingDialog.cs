// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared.Services;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.Bot.Builder.TestBot.Shared.Dialogs
{
    public class BookingDialog : CancelAndHelpDialog
    {
        private readonly IFlightBookingService _flightBookingService;

        public BookingDialog(GetBookingDetailsDialog getBookingDetailsDialog, IFlightBookingService flightBookingService)
            : base(nameof(BookingDialog))
        {
            _flightBookingService = flightBookingService;
            AddDialog(getBookingDetailsDialog);
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            var steps = new WaterfallStep[]
            {
                GetBookingDetailsActionAsync,
                ConfirmActionAsync,
                FinalActionAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetBookingDetailsActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options ?? new BookingDetails();

            return await stepContext.BeginDialogAsync(nameof(GetBookingDetailsDialog), bookingDetails, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Result;

            // Store the booking details in the waterfall state so we can use it once the user confirms
            stepContext.Values["BookingInfo"] = bookingDetails;

            var msg = $"Please confirm, I have you traveling to: {bookingDetails.Destination} from: {bookingDetails.Origin} on: {bookingDetails.TravelDate}";
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string msg;
            if ((bool)stepContext.Result)
            {
                // Pull the booking details from the waterfall state.
                var bookingDetails = (BookingDetails)stepContext.Values["BookingInfo"];

                // Now we have all the booking information to call the booking service.
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Booking your flight, this shouldn't take long..."), cancellationToken);

                var flightBooked = await _flightBookingService.BookFlight(bookingDetails, cancellationToken);
                if (flightBooked)
                {
                    // If the call to the booking service was successful tell the user.
                    var timeProperty = new TimexProperty(bookingDetails.TravelDate);
                    var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                    msg = $"I have you booked to {bookingDetails.Destination} from {bookingDetails.Origin} on {travelDateMsg}";
                }
                else
                {
                    msg = "Sorry, I was unable to secure your reservation, please try another flight";
                }
            }
            else
            {
                msg = "OK, we can do this later";
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
