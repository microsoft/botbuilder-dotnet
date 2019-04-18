using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Common;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class MainDialogIdealTests : DialogTestsBase
    {
        [Fact]
        public async Task FullEnchilada()
        {
            // Note: this test doesn't use a mock for booking so it ends up executing things that are out of scope from MainDialog.
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object);
            var testBot = new DialogsTestBot(sut);

            var reply = await testBot.SendAsync<IMessageActivity>("Hi");
            Assert.Equal("What can I help you with today?", reply.Text);

            reply = await testBot.SendAsync<IMessageActivity>("irrelevant");
            Assert.Equal("Where would you like to travel to?", reply.Text);

            reply = await testBot.SendAsync<IMessageActivity>("Bahamas");
            Assert.Equal("Where are you traveling from?", reply.Text);

            reply = await testBot.SendAsync<IMessageActivity>("New York");
            Assert.Equal("When would you like to travel?", reply.Text);

            reply = await testBot.SendAsync<IMessageActivity>("tomorrow at 5 PM");
            Assert.Equal("Please confirm, I have you traveling to: Bahamas from: New York on: 2019-04-19T17 (1) Yes or (2) No", reply.Text);
        }

        [Theory]
        [InlineData("{\"Destination\":\"Bahamas\",\"Origin\":\"New York\",\"TravelDate\":\"2019-04-19\"}", "I have you booked to Bahamas from New York on tomorrow")]
        [InlineData(null, "Thank you.")]
        public async Task MainDialogWithMockBooking(string bookingResult, string endMessage)
        {
            var expectedResult = bookingResult == null ? null : JsonConvert.DeserializeObject<BookingDetails>(bookingResult);
            var mockBookingDialog = CreateMockDialog<BookingDialog>(expectedResult);

            var sut = new MainDialog(MockConfig.Object, MockLogger.Object, mockBookingDialog.Object);
            var testBot = new DialogsTestBot(sut);

            var reply = await testBot.SendAsync<IMessageActivity>("Hi");
            Assert.Equal("What can I help you with today?", reply.Text);

            reply = await testBot.SendAsync<IMessageActivity>("irrelevant");
            Assert.Equal("BookingDialog mock invoked", reply.Text);

            reply = await testBot.GetNextReplyAsync<IMessageActivity>();
            Assert.Equal(endMessage, reply.Text);
        }

        private static Mock<T> CreateMockDialog<T>(object expectedResult)
            where T : Dialog
        {
            var mockDialog = new Mock<T>();
            var mockDialogNameTypeName = typeof(T).Name;
            mockDialog.Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(async (DialogContext dialogContext, object options, CancellationToken cancellationToken) =>
                {
                    await dialogContext.Context.SendActivityAsync($"{mockDialogNameTypeName} mock invoked", cancellationToken: cancellationToken);

                    return await dialogContext.EndDialogAsync(expectedResult, cancellationToken);
                });
            return mockDialog;
        }
    }
}
