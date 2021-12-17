// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Microsoft.Bot.Builder.AI.QnA.Tests.Dialogs
{
    public class QnAMakerDialogTests
    {
        [Fact]
        public async Task QnAMakerAction_ActiveLearningDialog_WithProperResponse()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, "QnAMakerAction_ActiveLearningDialog_WithProperResponse")
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Q1")
                .AssertReply("A1")
            .StartTestAsync();
        }

        [Fact]
        public async Task QnAMakerAction_ActiveLearningDialog_WithNoResponse()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            const string noAnswerActivity = "No match found, please ask another question.";

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, "QnAMakerAction_ActiveLearningDialog_WithNoResponse")
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Q12")
                .AssertReply(noAnswerActivity)
            .StartTestAsync();
        }

        [Fact]
        public async Task QnAMakerAction_ActiveLearningDialog_WithNoneOfAboveQuery()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, "QnAMakerAction_ActiveLearningDialog_WithNoneOfAboveQuery")
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("None of the above.")
                .AssertReply("Thanks for the feedback.")
            .StartTestAsync();
        }

        [Fact]
        public async Task QnAMakerAction_MultiTurnDialogBase_WithAnswer()
        {
            var rootDialog = QnAMakerAction_MultiTurnDialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_ReturnAnswer_withPrompts.json")));
            var promptsActivity = QnACardBuilder.GetQnAPromptsCard(response.Answers[0], "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, nameof(QnAMakerAction_MultiTurnDialogBase_WithAnswer))
            .Send("I have issues related to KB")
                .AssertReply(promptsActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Accidently deleted KB")
                .AssertReply("All deletes are permanent, including question and answer pairs, files, URLs, custom questions and answers, knowledge bases, or Azure resources. Make sure you export your knowledge base from the Settings**page before deleting any part of your knowledge base.")
            .StartTestAsync();
        }

        [Fact]
        public async Task QnAMakerAction_MultiTurnDialogBase_WithNoAnswer()
        {
            var rootDialog = QnAMakerAction_MultiTurnDialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_ReturnAnswer_withPrompts.json")));
            var promptsActivity = QnACardBuilder.GetQnAPromptsCard(response.Answers[0], "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, nameof(QnAMakerAction_MultiTurnDialogBase_WithNoAnswer))
            .Send("I have issues related to KB")
                .AssertReply(promptsActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("None of the above.")
                .AssertReply("Thanks for the feedback.")
            .StartTestAsync();
        }

        private AdaptiveDialog QnAMakerAction_ActiveLearningDialogBase()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Q11\",\"top\":3,\"strictFilters\":[],\"scoreThreshold\":0.3,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_TopNAnswer.json"));
            mockHttp.When(HttpMethod.Post, GetTrainRequestUrl())
                .Respond(HttpStatusCode.NoContent, "application/json", "{ }");
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Q12\",\"top\":3,\"strictFilters\":[],\"scoreThreshold\":0.3,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer_WhenNoAnswerFoundInKb.json"));

            return CreateQnAMakerActionDialog(mockHttp);
        }
        
        private AdaptiveDialog QnAMakerAction_MultiTurnDialogBase()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"I have issues related to KB\",\"top\":3,\"strictFilters\":[],\"scoreThreshold\":0.3,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnAnswer_withPrompts.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Accidently deleted KB\",\"top\":3,\"strictFilters\":[],\"scoreThreshold\":0.3,\"context\":{\"previousQnAId\":27,\"previousUserQuery\":\"\"},\"qnaId\":1,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnAnswer_MultiTurnLevel1.json"));

            return CreateQnAMakerActionDialog(mockHttp);
        }

        private AdaptiveDialog CreateQnAMakerActionDialog(MockHttpMessageHandler mockHttp)
        {
            var client = new HttpClient(mockHttp);

            var noAnswerActivity = new ActivityTemplate("No match found, please ask another question.");
            const string host = "https://dummy-hostname.azurewebsites.net/qnamaker";
            const string knowledgeBaseId = "dummy-id";
            const string endpointKey = "dummy-key";
            const string activeLearningCardTitle = "QnAMaker Active Learning";

            var outerDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>
                {
                    new OnBeginDialog
                    {
                        Actions = new List<Dialog>
                        {
                            new QnAMakerDialog
                            {
                                KnowledgeBaseId = knowledgeBaseId,
                                HostName = host,
                                EndpointKey = endpointKey,
                                HttpClient = client,
                                NoAnswer = noAnswerActivity,
                                ActiveLearningCardTitle = activeLearningCardTitle,
                                CardNoMatchText = "None of the above.",
                            }
                        }
                    }
                }
            };

            var rootDialog = new AdaptiveDialog("root")
            {
                Triggers = new List<OnCondition>
                {
                    new OnBeginDialog
                    {
                        Actions = new List<Dialog>
                        {
                            new BeginDialog(outerDialog.Id)
                        }
                    },
                    new OnDialogEvent
                    {
                        Event = "UnhandledUnknownIntent",
                        Actions = new List<Dialog>
                        {
                            new EditArray(),
                            new SendActivity("magenta")
                        }
                    }
                }
            };
            rootDialog.Dialogs.Add(outerDialog);
            return rootDialog;
        }
        
        private TestFlow CreateFlow(Dialog rootDialog, string testName)
        {
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(testName));
            adapter
                .UseStorage(storage)
                .UseBotState(userState, conversationState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(false)));

            var dm = new DialogManager(rootDialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken).ConfigureAwait(false);
            });
        }

        private class QnaMakerTestDialog : ComponentDialog, IDialogDependencies
        {
            public QnaMakerTestDialog(string knowledgeBaseId, string endpointKey, string hostName, HttpClient httpClient)
                : base(nameof(QnaMakerTestDialog))
            {
                AddDialog(new QnAMakerDialog(knowledgeBaseId, endpointKey, hostName, httpClient: httpClient));
            }

            public override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
            {
                return ContinueDialogAsync(outerDc, cancellationToken);
            }

            public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                if (dc.Context.Activity.Text == "moo")
                {
                    await dc.Context.SendActivityAsync("Yippee ki-yay!");
                    return EndOfTurn;
                }

                return await dc.BeginDialogAsync("qnaDialog");
            }

            public IEnumerable<Dialog> GetDependencies()
            {
                return Dialogs.GetDialogs();
            }

            public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
            {
                if ((bool)result == false)
                {
                    await dc.Context.SendActivityAsync("I didn't understand that.");
                }

                return await base.ResumeDialogAsync(dc, reason, result, cancellationToken);
            }
        }
    }
}
