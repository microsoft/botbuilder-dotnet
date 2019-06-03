// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.BotBuilderSamples.Tests.Framework.XUnit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Ignoring to make code more readable")]
    public static class BookingDialogTestsDataGenerator
    {
        public static IEnumerable<object[]> BookingFlows()
        {
            yield return BuildTestDataItem(
                "Full flow",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "New York", "When would you like to travel?" },
                    { "tomorrow", $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "yes", "I have you booked to Seattle from New York on tomorrow" },
                });
            yield return BuildTestDataItem(
                "Full flow with 'no' at confirmation",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "New York", "When would you like to travel?" },
                    { "tomorrow", $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "no", "OK, we can do this later." },
                });
            yield return BuildTestDataItem(
                "Destination given",
                new BookingDetails
                {
                    Destination = "Bahamas",
                    Origin = null,
                    TravelDate = null,
                },
                new[,]
                {
                    { "hi", "Where are you traveling from?" },
                });
            yield return BuildTestDataItem(
                "Destination and Origin given",
                new BookingDetails
                {
                    Destination = "Bahamas",
                    Origin = "New York",
                    TravelDate = null,
                },
                new[,]
                {
                    { "hi", "When would you like to travel?" },
                });
            yield return BuildTestDataItem(
                "All booking details given for today",
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "Bahamas",
                    TravelDate = $"{DateTime.UtcNow:yyyy-MM-dd}",
                },
                new[,]
                {
                    { "hi", $"Please confirm, I have you traveling to: Seattle from: Bahamas on: {DateTime.UtcNow:yyyy-MM-dd} (1) Yes or (2) No" },
                    { "yes", "I have you booked to Seattle from Bahamas on today" },
                });
            yield return BuildTestDataItem(
                "All booking details given for tomorrow",
                new BookingDetails
                {
                    Destination = "Bahamas",
                    Origin = "New York",
                    TravelDate = $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}",
                },
                new[,]
                {
                    { "hi", $"Please confirm, I have you traveling to: Bahamas from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "yes", "I have you booked to Bahamas from New York on tomorrow" },
                });
        }

        public static IEnumerable<object[]> CancelFlows()
        {
            yield return BuildTestDataItem(
                "Cancel on origin prompt",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "cancel", "Cancelling" },
                });

            yield return BuildTestDataItem(
                "Cancel on destination prompt",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "cancel", "Cancelling" },
                });

            yield return BuildTestDataItem(
                "Cancel on date prompt",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "New York", "When would you like to travel?" },
                    { "cancel", "Cancelling" },
                });

            yield return BuildTestDataItem(
                "Cancel on confirm prompt",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "New York", "When would you like to travel?" },
                    { "tomorrow", $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "cancel", "Cancelling" },
                });
        }

        private static object[] BuildTestDataItem(string testCaseName, BookingDetails inputBookingInfo, string[,] utterancesAndReplies)
        {
            var testData = new BookingDialogTestData
            {
                TestCaseName = testCaseName,
                BookingDetails = inputBookingInfo,
                UtterancesAndReplies = utterancesAndReplies,
            };
            var item = new object[] { new TestDataObject(testData) };
            return item;
        }
    }
}
