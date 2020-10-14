// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Bot.Builder.TestBot.Shared;
using Microsoft.Bot.Builder.Testing.XUnit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs.TestData
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Ignoring to make code more readable")]
    public class GetBookingDetailsDialogTestsDataGenerator
    {
        public static IEnumerable<object[]> BookingFlows()
        {
            yield return BuildTestCaseObject(
                "Full flow",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "New York", "When would you like to travel?" },
                    { $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}", null },
                },
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "New York",
                    TravelDate = $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}",
                });

            yield return BuildTestCaseObject(
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
                    { "New York", "When would you like to travel?" },
                    { $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}", null },
                },
                new BookingDetails
                {
                    Destination = "Bahamas",
                    Origin = "New York",
                    TravelDate = $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}",
                });

            yield return BuildTestCaseObject(
                "Destination and Origin given",
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "New York",
                    TravelDate = null,
                },
                new[,]
                {
                    { "hi", "When would you like to travel?" },
                    { $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}", null },
                },
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "New York",
                    TravelDate = $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}",
                });

            yield return BuildTestCaseObject(
                "All booking details given for today",
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "Bahamas",
                    TravelDate = $"{DateTime.UtcNow:yyyy-MM-dd}",
                },
                new[,]
                {
                    { "hi", null },
                },
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "Bahamas",
                    TravelDate = $"{DateTime.UtcNow:yyyy-MM-dd}",
                });
        }

        public static IEnumerable<object[]> CancelFlows()
        {
            yield return BuildTestCaseObject(
                "Cancel on origin prompt",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "cancel", "Cancelling" },
                },
                null);

            yield return BuildTestCaseObject(
                "Cancel on destination prompt",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "cancel", "Cancelling" },
                },
                null);

            yield return BuildTestCaseObject(
                "Cancel on date prompt",
                new BookingDetails(),
                new[,]
                {
                    { "hi", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "New York", "When would you like to travel?" },
                    { "cancel", "Cancelling" },
                },
                null);
        }

        private static object[] BuildTestCaseObject(string testCaseName, BookingDetails inputBookingInfo, string[,] utterancesAndReplies, BookingDetails expectedBookingInfo)
        {
            var testData = new GetBookingDetailsDialogTestCase
            {
                Name = testCaseName,
                InitialBookingDetails = inputBookingInfo,
                UtterancesAndReplies = utterancesAndReplies,
                ExpectedBookingDetails = expectedBookingInfo,
            };
            return new object[] { new TestDataObject(testData) };
        }
    }
}
