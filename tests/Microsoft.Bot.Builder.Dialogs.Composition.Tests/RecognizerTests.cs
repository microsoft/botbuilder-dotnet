using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Tests
{
    [TestClass]
    public class RecognizerTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        public TestContext TestContext { get; set; }

        private Activity CreateLocaleActivity(string locale)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = locale;
            activity.Locale = locale;
            return (Activity)activity;
        }
 
        /// <summary>
        /// Create test flow.
        /// </summary>
        private TestAdapter CreateTestAdapter(string initialDialog, out DialogSet dialogs, out BotCallbackHandler botHandler)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));
            var dlgs = new DialogSet(dialogState);
            dialogs = dlgs;
            botHandler = async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

                var dialogContext = await dlgs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    results = await dialogContext.BeginDialogAsync(initialDialog, null, cancellationToken);
                }
            };

            return adapter;
        }

        public class RecognizerDialog : Dialog, IDialog
        {
            public IRecognizer Recognizer { get; set; }

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                var result = await Recognizer.RecognizeAsync(dc.Context, cancellationToken);
                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(result.Text));
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }

            public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
            {
                var result = await Recognizer.RecognizeAsync(dc.Context, cancellationToken);
                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(result.Text));
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
        }
    }

    public class TestRecognizer : IRecognizer
    {
        public TestRecognizer(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new RecognizerResult() { Text = this.Id });
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            return await Task.FromResult(new T());
        }
    }
}
