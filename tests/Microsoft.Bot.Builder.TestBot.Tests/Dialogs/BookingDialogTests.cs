// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Services;
using Microsoft.BotBuilderSamples.Tests.Dialogs.TestData;
using Microsoft.BotBuilderSamples.Tests.Framework;
using Moq;
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

            var mockGetBookingDetailsDialog = DialogUtils.CreateMockDialog<GetBookingDetailsDialog>(bookingTestData.GetBookingDetailsDialogResult).Object;

            var sut = new BookingDialog(mockGetBookingDetailsDialog, mockFlightBookingService.Object);
            var testClient = new DialogTestClient(sut, null, new[] { new XUnitOutputMiddleware(Output) });

            // Act/Assert
            Output.WriteLine($"Test Case: {bookingTestData.Name}");
            for (var i = 0; i < bookingTestData.UtterancesAndReplies.GetLength(0); i++)
            {
                var message = bookingTestData.UtterancesAndReplies[i, 0];
                IMessageActivity reply;
                if (!string.IsNullOrEmpty(message))
                {
                    reply = await testClient.SendActivityAsync<IMessageActivity>(message);
                }
                else
                {
                    reply = testClient.GetNextReply<IMessageActivity>();
                }

                Assert.Equal(bookingTestData.UtterancesAndReplies[i, 1], reply.Text);
            }

            Assert.Equal(bookingTestData.ExpectedDialogResult.Status, testClient.DialogTurnResult.Status);
        }
    }
}
