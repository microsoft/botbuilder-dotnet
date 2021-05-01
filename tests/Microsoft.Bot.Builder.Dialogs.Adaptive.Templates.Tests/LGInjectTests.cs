#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable SA1402
using System;
using System.Reflection;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    public class LGInjectTests
    {
        public LGInjectTests()
        {
            Expression.Functions.Clear();
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
        }

        [Fact]
        public async Task TestLGInjectionAll()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);
            DialogManager dm = new DialogManager()
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration();
            dm.RootDialog = (AdaptiveDialog)resourceExplorer.LoadType<Dialog>("injectAll.dialog");

            await CreateFlow(
                async (turnContext, cancellationToken) =>
                {
                    await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
                }, locale: "en-GB")
            .Send("hello")
                .AssertReply("en-GB: 3")
                .AssertReply("3") // builtin function
                .AssertReply("my length function in lg") // lg template
            .StartTestAsync();
        }

        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        private TestFlow CreateFlow(BotCallbackHandler handler, string locale = null)
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(MethodBase.GetCurrentMethod().ToString()));
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            if (!string.IsNullOrEmpty(locale))
            {
                adapter.Locale = locale;
            }

            return new TestFlow(adapter, handler);
        }
    }
}
