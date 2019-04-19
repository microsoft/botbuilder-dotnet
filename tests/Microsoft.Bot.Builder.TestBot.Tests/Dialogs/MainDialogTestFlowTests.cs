using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Utils;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class MainDialogTestFlowTests : DialogTestsBase
    {
        [Fact]
        public async Task WholeEnchilada()
        {
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object);

            var testFlow = BuildTestFlow(sut);

            await testFlow.Send("hi")
                .AssertReply(activity =>
                {
                    var message = (IMessageActivity)activity;
                    Assert.Equal(
                        "What can I help you with today?",
                        message.Text);
                })
                .Send("irrelevant")
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
