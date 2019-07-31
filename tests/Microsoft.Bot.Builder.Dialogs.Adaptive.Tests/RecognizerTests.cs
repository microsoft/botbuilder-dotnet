// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
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
                .UseLanguageGeneration(resourceExplorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            adapter.Locale = locale;

            var dialog = new AdaptiveDialog();
            dialog.Recognizer = GetMultiLingualRecognizer();
            dialog.AddRules(new List<IRule>()
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
                new UnknownIntentRule(steps:
                    new List<IDialog>()
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
