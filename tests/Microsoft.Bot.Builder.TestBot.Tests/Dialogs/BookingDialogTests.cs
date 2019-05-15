// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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

        public static BookingDialogData<BookingDetails, List<Tuple<string, string>>> BookingDialogDataSource =>
            new BookingDialogData<BookingDetails, List<Tuple<string, string>>>
            {
                {
                    new BookingDetails(),
                    new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("irrelevant", "Where would you like to travel to?"),
                        new Tuple<string, string>("Seattle", "Where are you traveling from?"),
                        new Tuple<string, string>("New York", "When would you like to travel?"),
                        new Tuple<string, string>("tomorrow", $"Please confirm, I have you traveling to: Seattle from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No"),
                        new Tuple<string, string>("yes", "I have you booked to Seattle from New York on tomorrow"),
                    }
                },
                {
                    new BookingDetails
                    {
                        Destination = "Bahamas",
                        Origin = null,
                        TravelDate = null,
                    },
                    new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("irrelevant", "Where are you traveling from?"),
                    }
                },
                {
                    new BookingDetails
                    {
                        Destination = "Bahamas",
                        Origin = "New York",
                        TravelDate = null,
                    },
                    new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("irrelevant", "When would you like to travel?"),
                    }
                },
                {
                    new BookingDetails
                    {
                        Destination = "Bahamas",
                        Origin = "New York",
                        TravelDate = $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}",
                    },
                    new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("irrelevant", $"Please confirm, I have you traveling to: Bahamas from: New York on: {DateTime.UtcNow.AddDays(1):yyyy-MM-dd} (1) Yes or (2) No"),
                        new Tuple<string, string>("yes", "I have you booked to Bahamas from New York on tomorrow"),
                    }
                },
                {
                    new BookingDetails
                    {
                        Destination = "Seattle",
                        Origin = "Bahamas",
                        TravelDate = $"{DateTime.UtcNow:yyyy-MM-dd}",
                    },
                    new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("irrelevant", $"Please confirm, I have you traveling to: Seattle from: Bahamas on: {DateTime.UtcNow:yyyy-MM-dd} (1) Yes or (2) No"),
                        new Tuple<string, string>("yes", "I have you booked to Seattle from Bahamas on today"),
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
        public async Task TaskSelectorWithMemberData(BookingDetails inputBookingInfo, List<Tuple<string, string>> utterancesAndReplies)
        {
            var sut = new BookingDialog();
            var testBot = new DialogsTestBot(sut, Output, inputBookingInfo);

            foreach (var utterancesAndReply in utterancesAndReplies)
            {
                var reply = await testBot.SendAsync<IMessageActivity>(utterancesAndReply.Item1);
                Assert.Equal(utterancesAndReply.Item2, reply.Text);
            }
        }

        public class BookingDialogData<TBookingDetails, TUtterancesAndReplies> : TheoryData
            where TBookingDetails : BookingDetails
        {
            public void Add(TBookingDetails bookingDetails, TUtterancesAndReplies utterancesAndReplies)
            {
                AddRow(bookingDetails, utterancesAndReplies);
            }
        }
    }
}
