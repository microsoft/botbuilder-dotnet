using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Form.Tests
{


    public class FormTests
    {
        private static readonly string SchemaFile = @"resources\sandwich-schema.json";

        // Tests needed:
        // Simple mapping
        // Clarify
        // SingletonChoice
        // SlotChoice
        // Pending changes with updated singleton slot in each category
        [Fact]
        public async Task TestGeneration()
        {
            await CreateFlow("TestGeneration")
                .Send("order a ham sandwich")
                .StartTestAsync();
        }

        private TestFlow CreateFlow(string test, string locale = "en-us")
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var resourceExplorer = new ResourceExplorer();
            resourceExplorer.AddFolder(@"resources\");
            var adapter = new TestAdapter(TestAdapter.CreateConversation(test));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration(resourceExplorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            adapter.Locale = locale;

            var obj = (JObject)new JsonSerializer().Deserialize(new JsonTextReader(new StreamReader(SchemaFile)));
            var schema = new DialogSchema(obj);
            var dialog = new FormDialog(schema);
            dialog.Recognizer = new LuisRecognizer(
                "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/ec5be598-b4c5-4adb-9272-9bfb52595dec?verbose=true&timezoneOffset=-360&subscription-key=0f43266ab91447ec8d705897381478c5&q=",
                new LuisPredictionOptions
                {
                    IncludeInstanceData = true,
                });
            var dm = new DialogManager(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
