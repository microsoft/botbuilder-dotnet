// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
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
        public async Task Test_EnUsFallback_ActivityLocaleCasing()
        {
            await CreateFlow("en-us")
                .Send(new Activity() { Type = ActivityTypes.Message, Text = "howdy", Locale = "en-US" })
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
            await CreateFlow(string.Empty)
                .Send("salve")
                    .AssertReply("greeting intent")
                .Send("hello") // should not recognize as this is in the en recognizer
                    .AssertReply("default rule")
                .Send("vale dicere")
                    .AssertReply("goodbye intent")
                .StartTestAsync();
        }

        private static MultiLanguageRecognizer GetMultiLingualRecognizer()
        {
            return new MultiLanguageRecognizer()
            {
                Recognizers = new Dictionary<string, IRecognizer>()
                {
                     {
                        "en-us",
                        new RegexRecognizer()
                        {
                            Intents = new List<IntentPattern>()
                            {
                                new IntentPattern("Greeting", "(?i)howdy"),
                                new IntentPattern("Goodbye", "(?i)bye"),
                            }
                        }
                     },
                     {
                        "en-gb",
                        new RegexRecognizer()
                        {
                            Intents = new List<IntentPattern>()
                            {
                                new IntentPattern("Greeting", "(?i)hiya"),
                                new IntentPattern("Goodbye", "(?i)cheerio"),
                            }
                        }
                     },
                     {
                        "en",
                        new RegexRecognizer()
                        {
                            Intents = new List<IntentPattern>()
                            {
                                new IntentPattern("Greeting", "(?i)hello"),
                                new IntentPattern("Goodbye", "(?i)goodbye"),
                            }
                        }
                     },
                     {
                        string.Empty,
                        new RegexRecognizer()
                        {
                            Intents = new List<IntentPattern>()
                            {
                                new IntentPattern("Greeting", "(?i)salve"),
                                new IntentPattern("Goodbye", "(?i)vale dicere"),
                            }
                        }
                     },
                }
            };
        }

        private TestFlow CreateFlow(string locale)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var resourceExplorer = new ResourceExplorer();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(resourceExplorer)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer)
                .UseAdaptiveDialogs()
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            adapter.Locale = locale;

            var dialog = new AdaptiveDialog();
            dialog.Recognizer = GetMultiLingualRecognizer();
            dialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnIntent("Greeting", actions:
                    new List<Dialog>()
                    {
                        new SendActivity("greeting intent"),
                    }),
                new OnIntent("Goodbye", actions:
                    new List<Dialog>()
                    {
                        new SendActivity("goodbye intent"),
                    }),
                new OnUnknownIntent(actions:
                    new List<Dialog>()
                    {
                        new SendActivity("default rule"),
                    }),
            });
            DialogManager dm = new DialogManager(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
