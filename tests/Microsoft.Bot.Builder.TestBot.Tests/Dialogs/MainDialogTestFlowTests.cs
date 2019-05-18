using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.CognitiveModels;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.BotBuilderSamples.Tests.Utils;
using Moq;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    /// <summary>
    /// This sample uses the current classes and approach for testing bot conversations.
    /// Note: this is included just as a reference.
    /// </summary>
    public class MainDialogTestFlowTests : DialogTestsBase
    {
        [Fact]
        public async Task WholeEnchilada()
        {
            var intentsAndDialogs = new IntentDialogMap
            {
                { FlightBooking.Intent.BookFlight, new Mock<BookingDialog>().Object },
                { FlightBooking.Intent.GetWeather, new Mock<Dialog>("mockweather").Object },
            };
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object, null, intentsAndDialogs);

            var testFlow = BuildTestFlow(sut);

            await testFlow.Send("hi")
                .AssertReply("What can I help you with today?")
                .Send("hi")
                .AssertReply("Where would you like to travel to?")
                .Send("Bahamas")
                .AssertReply("Where are you traveling from?")
                .Send("New York")
                .AssertReply("When would you like to travel?")
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
                .AssertReply("I have you booked to Bahamas from New York on tomorrow 5PM")
                .StartTestAsync();
        }
    }
}
