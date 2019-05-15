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

        public static BookingDialogData<BookingDetails, string, string> BookingDialogDataSource =>
            new BookingDialogData<BookingDetails, string,  string>
            {
                { null, null, "Where would you like to travel to?" },
                { new BookingDetails(), null, "Where would you like to travel to?" },
                {
                    new BookingDetails
                    {
                        Destination = "Bahamas",
                        Origin = "New York",
                        TravelDate = $"{DateTime.UtcNow.AddDays(1):yyyy-MM-dd}",
                    },
                    null,
                    "I have you booked to Bahamas from New York on tomorrow"
                },
                {
                    new BookingDetails
                    {
                        Destination = "Seattle",
                        Origin = "Bahamas",
                        TravelDate = $"{DateTime.UtcNow:yyyy-MM-dd}",
                    },
                    null,
                    "I have you booked to Seattle from Bahamas on today"
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
        public async Task TaskSelectorWithMemberData(BookingDetails inputBookingInfo, string expectedMessage)
        {
            var sut = new BookingDialog();
            var testBot = new DialogsTestBot(sut, Output, inputBookingInfo);

            var reply = await testBot.SendAsync<IMessageActivity>("irrelevant");
            Assert.Equal("Please confirm, I have you traveling to:", reply.Text);
            reply = await testBot.SendAsync<IMessageActivity>("yes");
            Assert.Equal(expectedMessage, reply.Text);

        }

        public class BookingDialogData<TBookingDetails, TConfirmationPrompt, TExpectedReply> : TheoryData
            where TBookingDetails : BookingDetails
        {
            public void Add(TBookingDetails bookingDetails, TConfirmationPrompt confirmationPrompt, TExpectedReply expectedReply)
            {
                AddRow(bookingDetails, confirmationPrompt, expectedReply);
            }
        }
    }
}
