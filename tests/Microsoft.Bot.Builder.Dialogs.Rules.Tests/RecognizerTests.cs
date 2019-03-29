// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Rules.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Rules.Rules;
using Microsoft.Bot.Builder.Dialogs.Rules.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Tests
{
    [TestClass]
    public class RecognizerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task Test_EnUsFallback()
        {
            await CreateFlow("en-us")
                .Send("howdy") 
                    .AssertReply("greeting intent")
                .Send("cheerio") // should not recognize as this is in the en-gb recognizer
                    .AssertReply("default rule")
                .Send("bye") 
                    .AssertReply("goodbye intent")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_EnGbFallback()
        {
            await CreateFlow("en-gb")
                .Send("hiya") 
                    .AssertReply("greeting intent")
                .Send("howdy") // should not recognize as this is in the en-us recognizer, not en-gb
                    .AssertReply("default rule")
                .Send("cheerio")
                    .AssertReply("goodbye intent")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_EnFallback()
        {
            await CreateFlow("en")
                .Send("hello")
                    .AssertReply("greeting intent")
                .Send("howdy") // should not recognize as this is in the en-us recognizer, not en
                    .AssertReply("default rule")
                .Send("goodbye")
                    .AssertReply("goodbye intent")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_DefaultFallback()
        {
            await CreateFlow("")
                .Send("salve")
                    .AssertReply("greeting intent")
                .Send("hello") // should not recognize as this is in the en recognizer
                    .AssertReply("default rule")
                .Send("vale dicere")
                    .AssertReply("goodbye intent")
                .StartTestAsync();
        }

        private TestFlow CreateFlow(string locale)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());
            var planningDialog = new AdaptiveDialog();
            planningDialog.Recognizer = GetMultiLingualRecognizer();
            planningDialog.AddRules(new List<IRule>()
            {
                new IntentRule("Greeting", steps:
                    new List<IDialog>()
                    {
                        new SendActivity("greeting intent"),
                    }),
                new IntentRule("Goodbye", steps:
                    new List<IDialog>()
                    {
                        new SendActivity("goodbye intent"),
                    }),
                new DefaultRule(steps:
                    new List<IDialog>()
                    {
                        new SendActivity("default rule"),
                    }),
            });

            var botResourceManager = new ResourceExplorer();
            var lg = new LGLanguageGenerator(botResourceManager);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new RegisterClassMiddleware<ResourceExplorer>(botResourceManager))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IStorage>(new MemoryStorage()))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            adapter.Locale = locale;

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);


            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await planningDialog.OnTurnAsync(turnContext, null).ConfigureAwait(false);
            });
        }

        private static MultiLanguageRecognizer GetMultiLingualRecognizer()
        {
            return new MultiLanguageRecognizer()
            {
                Recognizers = new Dictionary<string, IRecognizer>()
                {
                     {
                        "en-us",
                        new RegexRecognizer() {
                            Intents = new Dictionary<string, string>()
                            {
                                { "Greeting", "(?i)howdy" },
                                { "Goodbye", "(?i)bye" },
                            }
                         }
                     },
                     {
                        "en-gb",
                        new RegexRecognizer() {
                            Intents = new Dictionary<string, string>()
                            {
                                { "Greeting", "(?i)hiya" },
                                { "Goodbye", "(?i)cheerio" },
                            }
                         }
                     },
                     {
                        "en",
                        new RegexRecognizer() {
                            Intents = new Dictionary<string, string>()
                            {
                                { "Greeting", "(?i)hello" },
                                { "Goodbye", "(?i)goodbye" },
                            }
                         }
                     },
                     {
                        "",
                        new RegexRecognizer() {
                            Intents = new Dictionary<string, string>()
                            {
                                { "Greeting", "(?i)salve" },
                                { "Goodbye", "(?i)vale dicere" },
                            }
                         }
                     },
                }
            };
        }
    }
}
