// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.LanguageGeneration.Renderer;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class MiscTests
    {
        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(AdaptiveDialog planningDialog, ConversationState convoState, UserState userState, bool sendTrace = false)
        {
            var botResourceManager = new ResourceExplorer();
            var lg = new LGLanguageGenerator(botResourceManager);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace)
                .Use(new RegisterClassMiddleware<ResourceExplorer>(botResourceManager))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IStorage>(new MemoryStorage()))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);


            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await planningDialog.OnTurnAsync(turnContext, null).ConfigureAwait(false);
            });
        }


        [TestMethod]
        public async Task Rule_Reprompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var testDialog = new AdaptiveDialog("testDialog")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        {  "SetName", @"my name is (?<name>.*)" }
                    }
                },
                Steps = new List<IDialog>()
                {
                    new TextInput() { Prompt = new ActivityTemplate("Hello, what is your name?"), OutputBinding = "user.name" },
                    new SendActivity("Hello {user.name}, nice to meet you!"),
                    new NumberInput() { MinValue=1, MaxValue = 110, Prompt = new ActivityTemplate("What is your age?"), OutputBinding = "user.age" },
                    new SendActivity("{user.age} is a good age to be!"),
                    new SendActivity("your name is {user.name}!"),
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("SetName", new List<string>() { "name" })
                    {
                        Steps = new List<IDialog>()
                        {
                            new SaveEntity("@name", "user.name"),
                        }
                    }
                }

            };

            await CreateFlow(testDialog, convoState, userState)
                .Send("hi")
                    .AssertReply("Hello, what is your name?")
                .Send("my name is Carlos")
                    .AssertReply("Hello Carlos, nice to meet you!")
                    .AssertReply("What is your age?")
                .Send("my name is Joe")
                    .AssertReply("What is your age?")
                .Send("15")
                    .AssertReply("15 is a good age to be!")
                    .AssertReply("your name is Joe!")
                .StartTestAsync();
        }


    }
}
