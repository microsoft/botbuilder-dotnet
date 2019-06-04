// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Schema;
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
        [MemberData(nameof(BookingDialogTestsDataGenerator.BookingFlows), MemberType = typeof(BookingDialogTestsDataGenerator))]
        [MemberData(nameof(BookingDialogTestsDataGenerator.CancelFlows), MemberType = typeof(BookingDialogTestsDataGenerator))]
        public async Task DialogFlowUseCases(TestDataObject testData)
        {
            // Arrange
            var bookingTestData = testData.GetObject<BookingDialogTestCase>();
            var sut = new BookingDialog();
            var testClient = new DialogTestClient(sut, Output, bookingTestData.BookingDetails);

            // Act/Assert
            Output.WriteLine($"Test Case: {bookingTestData.Name}");
            for (var i = 0; i < bookingTestData.UtterancesAndReplies.GetLength(0); i++)
            {
                var reply = await testClient.SendAsync<IMessageActivity>(bookingTestData.UtterancesAndReplies[i, 0]);
                Assert.Equal(bookingTestData.UtterancesAndReplies[i, 1], reply.Text);
            }
        }

        [Theory]
        [MemberData(nameof(BookingDialogTestsDataGenerator.CancelFlows), MemberType = typeof(BookingDialogTestsDataGenerator))]
        public async Task ShouldBeAbleToCancelAtAnyTime(TestDataObject testData)
        {
            // Arrange
            var bookingTestData = testData.GetObject<BookingDialogTestCase>();
            var sut = new BookingDialog();
            var testClient = new DialogTestClient(sut, Output, bookingTestData.BookingDetails);

            // Act/Assert
            Output.WriteLine($"Test Case: {bookingTestData.Name}");
            for (var i = 0; i < bookingTestData.UtterancesAndReplies.GetLength(0); i++)
            {
                var reply = await testClient.SendAsync<IMessageActivity>(bookingTestData.UtterancesAndReplies[i, 0]);
                Assert.Equal(bookingTestData.UtterancesAndReplies[i, 1], reply.Text);
            }

            Assert.Equal(DialogTurnStatus.Cancelled, testClient.DialogTurnResult.Status);
        }
    }
}
