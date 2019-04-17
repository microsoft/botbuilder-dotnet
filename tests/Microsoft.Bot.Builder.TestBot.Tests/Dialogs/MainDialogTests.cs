using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class MainDialogTests : DialogTestsBase
    {
        [Fact]
        public void DialogConstructor()
        {
            // TODO: check with the team if there's value in these types of test or if there's a better way of asserting the
            // dialog got composed properly.
            var mockConfig = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<MainDialog>>();

            var sut = new MainDialog(mockConfig.Object, mockLogger.Object);

            Assert.Equal("MainDialog", sut.Id);
            Assert.IsType<TextPrompt>(sut.FindDialog("TextPrompt"));
            Assert.IsType<BookingDialog>(sut.FindDialog("BookingDialog"));
            Assert.IsType<WaterfallDialog>(sut.FindDialog("WaterfallDialog"));
        }

        [Fact]
        public async Task HappyPath()
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<MainDialog>>();
            var sut = new MainDialog(mockConfig.Object, mockLogger.Object);

            var testFlow = BuildTestFlow(sut);

            await testFlow.Send("hi")
                .AssertReply(activity =>
                {
                    var message = (IMessageActivity)activity;
                    Assert.Equal(
                        "NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.",
                        message.Text);
                })
                .AssertReply(activity =>
                {
                    var message = (IMessageActivity)activity;
                    Assert.Equal(
                        "Where would you like to travel to?",
                        message.Text);
                })
                .Send("Bahamas")
                .AssertReply(activity =>
                {
                    var message = (IMessageActivity)activity;
                    Assert.Equal(
                        "Where are you traveling from?",
                        message.Text);
                })
                .Send("New York")
                .AssertReply(activity =>
                {
                    var message = (IMessageActivity)activity;
                    Assert.Equal(
                        "When would you like to travel?",
                        message.Text);
                })
                .Send("tomorrow at 5 PM")
                .AssertReply(activity =>
                {
                    // TODO: I had to add the Yes No for the channelId = test, the emulator displays suggested actions instead.
                    var message = (IMessageActivity)activity;
                    Assert.Equal(
                        "Please confirm, I have you traveling to: Bahamas from: New York on: 2019-04-18T17 (1) Yes or (2) No",
                        message.Text);
                })
                .Send("Yes")
                .AssertReply(activity =>
                {
                    var message = (IMessageActivity)activity;
                    Assert.Equal(
                        "I have you booked to Bahamas from New York on tomorrow 5PM",
                        message.Text);
                })
                .StartTestAsync();
        }
    }
}
