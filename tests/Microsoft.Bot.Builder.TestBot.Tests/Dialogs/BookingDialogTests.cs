// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TestBot.Shared;
using Microsoft.Bot.Builder.TestBot.Shared.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared.Services;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Dialogs.TestData;
using Microsoft.BotBuilderSamples.Tests.Framework;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class BookingDialogTests : BotTestBase
    {
        private readonly XUnitDialogTestLogger[] _middlewares;

        public BookingDialogTests(ITestOutputHelper output)
            : base(output)
        {
            _middlewares = new[] { new XUnitDialogTestLogger(output) };
        }

        [Theory]
        [MemberData(nameof(BookingDialogTestsDataGenerator.BookingFlows), MemberType = typeof(BookingDialogTestsDataGenerator))]
        public async Task DialogFlowUseCases(TestDataObject testData)
        {
            // Arrange
            var bookingTestData = testData.GetObject<BookingDialogTestCase>();
            var mockFlightBookingService = new Mock<IFlightBookingService>();
            mockFlightBookingService
                .Setup(x => x.BookFlight(It.IsAny<BookingDetails>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(bookingTestData.FlightBookingServiceResult));

            var mockGetBookingDetailsDialog = SimpleMockFactory.CreateMockDialog<GetBookingDetailsDialog>(bookingTestData.GetBookingDetailsDialogResult).Object;

            var sut = new BookingDialog(mockGetBookingDetailsDialog, mockFlightBookingService.Object);
            var testClient = new DialogTestClient(Channels.Test, sut, middlewares: _middlewares);

            // Act/Assert
            Output.WriteLine($"Test Case: {bookingTestData.Name}");
            for (var i = 0; i < bookingTestData.UtterancesAndReplies.GetLength(0); i++)
            {
                var utterance = bookingTestData.UtterancesAndReplies[i, 0];

                // Send the activity and receive the first reply or just pull the next
                // activity from the queue if there's nothing to send
                var reply = !string.IsNullOrEmpty(utterance)
                    ? await testClient.SendActivityAsync<IMessageActivity>(utterance)
                    : testClient.GetNextReply<IMessageActivity>();

                Assert.Equal(bookingTestData.UtterancesAndReplies[i, 1], reply.Text);
            }

            Assert.Equal(bookingTestData.ExpectedDialogResult.Status, testClient.DialogTurnResult.Status);
        }
    }
}
