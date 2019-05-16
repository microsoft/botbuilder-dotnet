// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Ignoring to make code more readable")]
    public class BookingDialogTestsDialogFlowData : TheoryData<string, BookingDetails, string[,]>
    {
        public BookingDialogTestsDialogFlowData()
        {
            Add(
                "Full flow",
                new BookingDetails(),
                new[,]
                {
                    { "irrelevant", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "New York", "When would you like to travel?" },
                    { "tomorrow", $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "yes", "I have you booked to Seattle from New York on tomorrow" },
                });
            Add(
                "Full flow with 'no' at confirmation",
                new BookingDetails(),
                new[,]
                {
                    { "irrelevant", "Where would you like to travel to?" },
                    { "Seattle", "Where are you traveling from?" },
                    { "New York", "When would you like to travel?" },
                    { "tomorrow", $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "no", "OK, we can do this later." },
                });
            Add(
                "Destination given",
                new BookingDetails
                {
                    Destination = "Bahamas",
                    Origin = null,
                    TravelDate = null,
                },
                new[,]
                {
                    { "irrelevant", "Where are you traveling from?" },
                });
            Add(
                "Destination and Origin given",
                new BookingDetails
                {
                    Destination = "Bahamas",
                    Origin = "New York",
                    TravelDate = null,
                },
                new[,]
                {
                    { "irrelevant", "When would you like to travel?" },
                });
            Add(
                "All booking details given for today",
                new BookingDetails
                {
                    Destination = "Seattle",
                    Origin = "Bahamas",
                    TravelDate = $"{DateTime.UtcNow:yyyy-MM-dd}",
                },
                new[,]
                {
                    { "irrelevant", $"Please confirm, I have you traveling to: Seattle from: Bahamas on: {DateTime.UtcNow:yyyy-MM-dd} (1) Yes or (2) No" },
                    { "yes", "I have you booked to Seattle from Bahamas on today" },
                });
            Add(
                "All booking details given for tomorrow",
                new BookingDetails
                {
                    Destination = "Bahamas",
                    Origin = "New York",
                    TravelDate = $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}",
                },
                new[,]
                {
                    { "irrelevant", $"Please confirm, I have you traveling to: Bahamas from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                    { "yes", "I have you booked to Bahamas from New York on tomorrow" },
                });
        }
    }
}
