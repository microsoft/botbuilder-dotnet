// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    [TestClass]
    public class QnAMakerDialogTests
    {
        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(AdaptiveDialog ruleDialog, ConversationState convoState, UserState userState)
        {
            var explorer = new ResourceExplorer();
            var lg = new LGLanguageGenerator(explorer);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new RegisterClassMiddleware<IStorage>(new MemoryStorage()))
                .Use(new RegisterClassMiddleware<IExpressionParser>(new ExpressionEngine()))
                .Use(new RegisterClassMiddleware<ResourceExplorer>(explorer))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            ruleDialog.BotState = convoState.CreateProperty<Microsoft.Bot.Builder.Dialogs.Adaptive.BotState>("bot");
            ruleDialog.UserState = userState.CreateProperty<Dictionary<string, object>>("user"); ;

            var dialogs = new DialogSet(dialogState);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await ruleDialog.OnTurnAsync(turnContext, null).ConfigureAwait(false);
            });
        }

        [TestMethod]
        public async Task QnAMakerDialog_Answers()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var rootDialog = new AdaptiveDialog("root")
            {
                Steps = new List<IDialog>()
                {
                    new BeginDialog()
                    {
                        Dialog = new AdaptiveDialog("outer")
                        {
                            AutoEndDialog = false,
                            Recognizer = new RegexRecognizer()
                            {
                                Intents = new Dictionary<string, string>()
                                {
                                    { "CowboyIntent" , "moo" }
                                }
                            },
                            Rules = new List<IRule>()
                            {
                                new IntentRule(intent: "CowboyIntent")
                                {
                                    Steps = new List<IDialog>()
                                    {
                                        new SendActivity("Yippee ki-yay!")
                                    }
                                },
                                new UnknownIntentRule()
                                {
                                    Steps = new List<IDialog>()
                                    {
                                        new QnAMakerDialog()
                                        {
                                            Endpoint = new QnAMakerEndpoint()
                                            {
                                                EndpointKey = "c2d35859-4c6b-4b18-b776-ed625ac95215",
                                                KnowledgeBaseId = "24d34beb-1be5-466c-861d-97711191595d",
                                                Host = "https://vk-test-qna.azurewebsites.net/qnamaker",
                                            },
                                            OutputProperty = "turn.LastResult"
                                        },
                                        new IfCondition()
                                        {
                                             Condition = new ExpressionEngine().Parse("turn.LastResult == false"),
                                             Steps =   new List<IDialog>()
                                             {
                                                 new SendActivity("I didn't understand that.") 
                                             }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Rules = new List<IRule>()
                {
                    new EventRule()
                    {
                        Events = new List<string>() { "UnhandledUnknownIntent"},
                        Steps = new List<IDialog>()
                        {
                            new EditArray(),
                            new SendActivity("magenta")
                        }
                    }
                }
            };


            await CreateFlow(rootDialog, convoState, userState)
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .Send("what is your hours?")
                .AssertReply("Most cafe locations are open 8AM through 10PM local Pacific time.")
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .Send("qqqweqewqweqw")
                .AssertReply("I didn't understand that.")
            .StartTestAsync();
        }
    }
}
