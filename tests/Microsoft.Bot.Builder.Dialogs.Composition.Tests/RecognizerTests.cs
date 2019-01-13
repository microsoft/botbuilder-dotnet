using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Composition.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Tests
{
    public class TestRecognizer : IRecognizer
    {
        public TestRecognizer(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return new RecognizerResult() { Text = this.Id };
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            return new T();
        }
    }

    [TestClass]
    public class RecognizerTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Create test flow
        /// </summary>
        private static TestAdapter CreateTestAdapter(string initialDialog, out DialogSet dialogs, out BotCallbackHandler botHandler)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState));
            var dlgs = new DialogSet(dialogState);
            dialogs = dlgs;
            botHandler = async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

                var dialogContext = await dlgs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                    results = await dialogContext.BeginDialogAsync(initialDialog, null, cancellationToken);
            };

            return adapter;
        }

        public class RecognizerDialog : Dialog, IDialog, IRecognizerDialog
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

        private Activity CreateLocaleActivity(string locale)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = locale;
            activity.Locale = locale;
            return (Activity)activity;
        }

        [TestMethod]
        public async Task LanguageRecognizerSetTets()
        {
            var recognizerSet = new LanguageRecognizerSet();
            recognizerSet.Recognizers.Add("en-uk", new TestRecognizer("en-uk"));
            recognizerSet.Recognizers.Add("en", new TestRecognizer("en"));
            recognizerSet.Recognizers.Add("fr", new TestRecognizer("fr"));
            recognizerSet.Recognizers.Add("default", new TestRecognizer("default"));

            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            dialogs.Add(new RecognizerDialog()
            {
                Id = "TestDialog",
                Recognizer = recognizerSet
            });

            await new TestFlow(testAdapter, botHandler)
                .Send(CreateLocaleActivity("en-uk"))
                    .AssertReply("en-uk")
                .Send(CreateLocaleActivity("en-us"))
                    .AssertReply("en")
                .Send(CreateLocaleActivity("en"))
                    .AssertReply("en")
                .Send(CreateLocaleActivity("fr"))
                    .AssertReply("fr")
                .Send(CreateLocaleActivity("de"))
                    .AssertReply("default")
                .StartTestAsync();

        }
    }
}
