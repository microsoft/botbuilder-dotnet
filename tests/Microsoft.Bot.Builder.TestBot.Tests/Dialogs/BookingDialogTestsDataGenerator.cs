// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.BotBuilderSamples.Tests.Framework.XUnit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Ignoring to make code more readable")]
    public class BookingDialogTestsDataGenerator : IEnumerable<object[]>
    {
        private readonly List<object[]> _data;

        public BookingDialogTestsDataGenerator()
        {
            _data = new List<object[]>();
            AddTestCase(
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
            AddTestCase(
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
            AddTestCase(
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
            AddTestCase(
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
            AddTestCase(
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
            AddTestCase(
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

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void AddTestCase(string testCaseName, BookingDetails inputBookingInfo, string[,] utterancesAndReplies)
        {
            var testData = new BookingDialogTestData
            {
                TestCaseName = testCaseName,
                BookingDetails = inputBookingInfo,
                UtterancesAndReplies = utterancesAndReplies,
            };
            _data.Add(new object[] { new TestDataObject(testData) });
        }
    }
}
