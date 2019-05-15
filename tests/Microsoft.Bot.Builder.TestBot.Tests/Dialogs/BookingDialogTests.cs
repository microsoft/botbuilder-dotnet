// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class BookingDialogTests : DialogTestsBase
    {
        public BookingDialogTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static BookingDialogData<string, BookingDetails, string[,]> BookingDialogDataSource =>
            new BookingDialogData<string, BookingDetails, string[,]>
            {
                {
                    "Full flow",
                    new BookingDetails(),
                    new[,]
                    {
                        { "irrelevant", "Where would you like to travel to?" },
                        { "Seattle", "Where are you traveling from?" },
                        { "New York", "When would you like to travel?" },
                        { "tomorrow", $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                        { "yes", "I have you booked to Seattle from New York on tomorrow" },
                    }
                },
                {
                    "Full flow with 'no' at confirmation",
                    new BookingDetails(),
                    new[,]
                    {
                        { "irrelevant", "Where would you like to travel to?" },
                        { "Seattle", "Where are you traveling from?" },
                        { "New York", "When would you like to travel?" },
                        { "tomorrow", $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No" },
                        { "no", "OK, we can do this later." },
                    }
                },
                {
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
                    }
                },
                {
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
                    }
                },
                {
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
                    }
                },
                {
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
                    }
                },
            };

        [Fact]
        public void DialogConstructor()
        {
            var sut = new BookingDialog();

            Assert.Equal("BookingDialog", sut.Id);
            Assert.IsType<TextPrompt>(sut.FindDialog("TextPrompt"));
            Assert.IsType<ConfirmPrompt>(sut.FindDialog("ConfirmPrompt"));
            Assert.IsType<DateResolverDialog>(sut.FindDialog("DateResolverDialog"));
            Assert.IsType<WaterfallDialog>(sut.FindDialog("WaterfallDialog"));
        }

        [Theory]
        [MemberData(nameof(BookingDialogDataSource))]
        public async Task FlowScenarios(string useCaseName, BookingDetails inputBookingInfo, string[,] utterancesAndReplies)
        {
            var sut = new BookingDialog();
            var testBot = new DialogsTestBot(sut, Output, inputBookingInfo);

            Output.WriteLine($"Use Case: {useCaseName}");
            for (var i = 0; i < utterancesAndReplies.GetLength(0); i++)
            {
                var reply = await testBot.SendAsync<IMessageActivity>(utterancesAndReplies[i, 0]);
                Assert.Equal(utterancesAndReplies[i, 1], reply.Text);
            }
        }

        [Fact]
        public void ShouldBeAbleToCancelAtAnyTime()
        {
            // TODO
        }

        public class BookingDialogData<TUseCaseName, TBookingDetails, TUtterancesAndReplies> : TheoryData
            where TBookingDetails : BookingDetails
        {
            public void Add(TUseCaseName useCaseName, TBookingDetails bookingDetails, TUtterancesAndReplies utterancesAndReplies)
            {
                AddRow(useCaseName, bookingDetails, utterancesAndReplies);
            }
        }
    }
}
