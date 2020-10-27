// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared;
using Microsoft.Bot.Builder.Testing.XUnit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs.TestData
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Ignoring to make code more readable")]
    public class BookingDialogTestsDataGenerator
    {
        public static IEnumerable<object[]> BookingFlows()
        {
            yield return BuildTestCaseObject(
                "Full flow",
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "New York",
                    TravelDate = $"{DateTime.Now.AddDays(1):yyyy-MM-dd}",
                },
                new[,]
                {
                    { "hi", "GetBookingDetailsDialog mock invoked" },
                    { null, $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.Now.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "yes", "Booking your flight, this shouldn't take long..." },
                    { null, "I have you booked to Seattle from New York on tomorrow" },
                });

            yield return BuildTestCaseObject(
                "Full flow with 'no' at confirmation",
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "New York",
                    TravelDate = $"{DateTime.Now.AddDays(1):yyyy-MM-dd}",
                },
                new[,]
                {
                    { "hi", "GetBookingDetailsDialog mock invoked" },
                    { null, $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.Now.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "no", "OK, we can do this later" },
                });

            yield return BuildTestCaseObject(
                "Full flow with 'cancel' at confirmation",
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "New York",
                    TravelDate = $"{DateTime.Now.AddDays(1):yyyy-MM-dd}",
                },
                new[,]
                {
                    { "hi", "GetBookingDetailsDialog mock invoked" },
                    { null, $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.Now.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "cancel", "Cancelling" },
                },
                true,
                new DialogTurnResult(DialogTurnStatus.Complete));

            yield return BuildTestCaseObject(
                "Full flow with failed call to FlightBookingService",
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "New York",
                    TravelDate = $"{DateTime.Now.AddDays(1):yyyy-MM-dd}",
                },
                new[,]
                {
                    { "hi", "GetBookingDetailsDialog mock invoked" },
                    { null, $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.Now.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "yes", "Booking your flight, this shouldn't take long..." },
                    { null, "Sorry, I was unable to secure your reservation, please try another flight" },
                }, false);
        }

        private static object[] BuildTestCaseObject(string testCaseName, BookingDetails inputBookingInfo, string[,] utterancesAndReplies, bool flightBookingServiceResult = true, DialogTurnResult expectedDialogTurnResult = null)
        {
            var testData = new BookingDialogTestCase
            {
                Name = testCaseName,
                GetBookingDetailsDialogResult = inputBookingInfo,
                UtterancesAndReplies = utterancesAndReplies,
                FlightBookingServiceResult = flightBookingServiceResult,
                ExpectedDialogResult = expectedDialogTurnResult ?? new DialogTurnResult(DialogTurnStatus.Complete),
            };
            return new object[] { new TestDataObject(testData) };
        }
    }
}
