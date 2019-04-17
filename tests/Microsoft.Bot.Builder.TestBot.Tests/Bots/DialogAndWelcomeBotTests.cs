using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Bots
{
    public class DialogAndWelcomeBotTests
    {
        [Fact]
        public async Task ReturnsWelcomeCardOnConversationUpdate()
        {
            var mockRootDialog = new Mock<Dialog>("mockRootDialog");
            mockRootDialog.Setup(x => x.ContinueDialogAsync(It.IsAny<DialogContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new DialogTurnResult(DialogTurnStatus.Empty)));

            // TODO: do we need state here?
            var memoryStorage = new MemoryStorage();
            var sut = new DialogAndWelcomeBot<Dialog>(new ConversationState(memoryStorage), new UserState(memoryStorage), mockRootDialog.Object, null);
            var testAdapter = new TestAdapter();
            var testFlow = new TestFlow(testAdapter, sut);
            await testFlow.Send(new Activity
                {
                    Type = ActivityTypes.ConversationUpdate,
                    MembersAdded = new List<ChannelAccount>
                    {
                        new ChannelAccount { Id = "theUser" },
                    },
                    Recipient = new ChannelAccount { Id = "theBot" },
                })
                .AssertReply(activity =>
                {
                    var message = (IMessageActivity)activity;
                    Assert.Equal(1, message.Attachments.Count);
                    Assert.Equal("application/vnd.microsoft.card.adaptive", message.Attachments.FirstOrDefault()?.ContentType);
                })
                .StartTestAsync();
        }
    }
}
