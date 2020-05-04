// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.AI.Tests
{
    [TestClass]
    public class QnAMakerRecognizerTests
    {
        private const string _knowledgeBaseId = "dummy-id";
        private const string _endpointKey = "dummy-key";
        private const string _hostname = "https://dummy-hostname.azurewebsites.net/qnamaker";

        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = Path.GetDirectoryName(parent);
                }
            }

            ResourceExplorer = new ResourceExplorer()
                .AddFolder(parent, monitorChanges: false);
        }

        public AdaptiveDialog QnAMakerRecognizer_DialogBase()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .WithContent("{\"question\":\"QnaMaker_ReturnsAnswer\",\"top\":3,\"strictFilters\":[{\"name\":\"dialogName\",\"value\":\"outer\"}],\"scoreThreshold\":0.3,\"context\":null,\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\"}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .WithContent("{\"question\":\"QnaMaker_ReturnsNoAnswer\",\"top\":3,\"strictFilters\":[{\"name\":\"dialogName\",\"value\":\"outer\"}],\"scoreThreshold\":0.3,\"context\":null,\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\"}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnsNoAnswer.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .WithContent("{\"question\":\"QnaMaker_TopNAnswer\",\"top\":3,\"strictFilters\":[{\"name\":\"dialogName\",\"value\":\"outer\"}],\"scoreThreshold\":0.3,\"context\":null,\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\"}")
                .Respond("application/json", GetResponse("QnaMaker_TopNAnswer.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .WithContent("{\"question\":\"QnaMaker_ReturnsAnswerWithIntent\",\"top\":3,\"strictFilters\":[{\"name\":\"dialogName\",\"value\":\"outer\"}],\"scoreThreshold\":0.3,\"context\":null,\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\"}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswerWithIntent.json"));

            return CreateQnAMakerActionDialog(mockHttp);
        }

        [TestMethod]
        public async Task QnAMakerRecognizer_WithTopNAnswer()
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_TopNAnswer.json")));

            await CreateFlow(rootDialog)
            .Send("QnaMaker_TopNAnswer")
                .AssertReply(response.Answers[0].Answer)
                .AssertReply("done")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task QnAMakerRecognizer_WithAnswer()
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_ReturnsAnswer.json")));

            await CreateFlow(rootDialog)
            .Send("QnaMaker_ReturnsAnswer")
                .AssertReply(response.Answers[0].Answer)
                .AssertReply("done")
            .Send("QnaMaker_ReturnsNoAnswer")
                .AssertReply("Wha?")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task QnAMakerRecognizer_WithNoAnswer()
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();
            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_ReturnsAnswer_WhenNoAnswerFoundInKb.json")));

            await CreateFlow(rootDialog)
            .Send("QnaMaker_ReturnsNoAnswer")
                .AssertReply("Wha?")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task QnAMakerRecognizer_WithIntent()
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();

            await CreateFlow(rootDialog)
            .Send("QnaMaker_ReturnsAnswerWithIntent")
                .AssertReply("DeferToRecognizer_xxx")
            .StartTestAsync();
        }

        private TestFlow CreateFlow(Dialog rootDialog)
        {
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseBotState(userState, conversationState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            DialogManager dm = new DialogManager(rootDialog)
                .UseResourceExplorer(ResourceExplorer)
                .UseLanguageGeneration();

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }

        private AdaptiveDialog CreateQnAMakerActionDialog(MockHttpMessageHandler mockHttp)
        {
            var client = new HttpClient(mockHttp);

            var host = "https://dummy-hostname.azurewebsites.net/qnamaker";
            var knowlegeBaseId = "dummy-id";
            var endpointKey = "dummy-key";

            var rootDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Recognizer = new QnAMakerRecognizer()
                {
                    KnowledgeBaseId = knowlegeBaseId,
                    HostName = host,
                    EndpointKey = endpointKey,
                    HttpClient = client
                },
                Triggers = new List<OnCondition>()
                {
                    new OnQnAMatch()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("${@answer}")
                            },
                            new AssertCondition()
                            {
                                Condition = "count(turn.recognized.entities.answer) == 1",
                                Description = "If there is a match there should only be 1 answer"
                            },
                            new AssertCondition()
                            {
                                Condition = "turn.recognized.answers[0].answer != null",
                                Description = "There should be answers object"
                            },
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("done")
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "DeferToRecognizer_xxx",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("DeferToRecognizer_xxx")
                            }
                        }
                    },
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Wha?")
                        }
                    }
                }
            };

            return rootDialog;
        }

        private string GetV2LegacyRequestUrl() => $"{_hostname}/v2.0/knowledgebases/{_knowledgeBaseId}/generateanswer";

        private string GetV3LegacyRequestUrl() => $"{_hostname}/v3.0/knowledgebases/{_knowledgeBaseId}/generateanswer";

        private string GetRequestUrl() => $"{_hostname}/knowledgebases/{_knowledgeBaseId}/generateanswer";

        private string GetTrainRequestUrl() => $"{_hostname}/knowledgebases/{_knowledgeBaseId}/train";

        private Stream GetResponse(string fileName)
        {
            var path = GetFilePath(fileName);
            return File.OpenRead(path);
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
        }

        private class CapturedRequest
        {
            public string[] Questions { get; set; }

            public int Top { get; set; }

            public Metadata[] StrictFilters { get; set; }

            public Metadata[] MetadataBoost { get; set; }

            public float ScoreThreshold { get; set; }
        }
    }
}
