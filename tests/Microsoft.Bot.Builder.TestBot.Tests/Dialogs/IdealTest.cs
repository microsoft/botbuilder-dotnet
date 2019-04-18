using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class IdealTest : DialogTestsBase
    {
        [Fact]
        public async Task MainDialogTest()
        {
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object);

            var testBot = BuildTestBot(sut);
            var reply = await testBot.SendAsync<IMessageActivity>("Hi");
            Assert.Equal("What can I help you with today?", reply.Text);

            reply = await testBot.SendAsync<IMessageActivity>("irrelevant");
            Assert.Equal("Where would you like to travel to?", reply.Text);

            reply = await testBot.SendAsync<IMessageActivity>("Bahamas");
            Assert.Equal("Where are you traveling from?", reply.Text);

            reply = await testBot.SendAsync<IMessageActivity>("New York");
            Assert.Equal("When would you like to travel?", reply.Text);
        }

        [Fact(Skip = "This is just a sample, we need to build a bot that sends two responses")]
        public async Task MainDialogTestWithWait()
        {
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object);

            var testBot = BuildTestBot(sut);
            var reply = await testBot.SendAsync<IMessageActivity>("Hi");
            Assert.Equal("Hi, how are you", ((IMessageActivity)reply).Text);

            reply = await testBot.SendAsync<IMessageActivity>("Show me my to dos");
            Assert.Equal("Sure thing, looking into it...", reply.Text);

            reply = testBot.GetNextReplyAsync<IMessageActivity>();
            Assert.Equal("And here are your to dos", reply.Text);
        }

        private TestBot BuildTestBot(MainDialog sut)
        {
            return new TestBot(sut);
        }
    }

    public class TestBot
    {
        private readonly BotCallbackHandler _callback;
        private readonly TestAdapter _testAdapter;

        public TestBot(Dialog targetDialog)
        {
            var convoState = new ConversationState(new MemoryStorage());
            _testAdapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));
            var dialogState = convoState.CreateProperty<DialogState>("DialogState");
            _callback = async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState(), cancellationToken);
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(targetDialog);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                switch (results.Status)
                {
                    case DialogTurnStatus.Empty:
                        await dc.BeginDialogAsync(targetDialog.Id, null, cancellationToken);
                        break;
                    case DialogTurnStatus.Complete:
                    {
                        // TODO: Dialog has ended, figure out a way of asserting that this is the case.
                        break;
                    }
                }
            };
        }

        public Task<T> SendAsync<T>(string text, CancellationToken cancellationToken = default)
        {
            var task = _testAdapter.SendTextToBotAsync(text, _callback, cancellationToken);
            task.Wait(cancellationToken);
            return Task.FromResult(GetNextReplyAsync<T>());
        }

        public T GetNextReplyAsync<T>()
        {
            return (T)_testAdapter.GetNextReply();
        }
    }
}
