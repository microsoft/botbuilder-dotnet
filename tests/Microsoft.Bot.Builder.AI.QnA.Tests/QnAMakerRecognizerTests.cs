// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    public class QnAMakerRecognizerTests : IClassFixture<QnAMakerRecognizerFixture>
    {
        private const string KnowledgeBaseId = "dummy-id";
        private const string EndpointKey = "dummy-key";
        private const string Hostname = "https://dummy-hostname.azurewebsites.net/qnamaker";
        private const string QnAReturnsAnswerText = "QnaMaker_ReturnsAnswer";

        private readonly QnAMakerRecognizerFixture _qnaMakerRecognizerFixture;

        public QnAMakerRecognizerTests(QnAMakerRecognizerFixture qnaMakerRecognizerFixture)
        {
            _qnaMakerRecognizerFixture = qnaMakerRecognizerFixture;
        }

        public AdaptiveDialog QnAMakerRecognizer_DialogBase()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .WithContent("{\"question\":\"QnaMaker_ReturnsAnswer\",\"top\":3,\"strictFilters\":[{\"name\":\"dialogName\",\"value\":\"outer\"}],\"scoreThreshold\":0.3,\"context\":null,\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .WithContent("{\"question\":\"QnaMaker_ReturnsNoAnswer\",\"top\":3,\"strictFilters\":[{\"name\":\"dialogName\",\"value\":\"outer\"}],\"scoreThreshold\":0.3,\"context\":null,\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnsNoAnswer.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .WithContent("{\"question\":\"QnaMaker_TopNAnswer\",\"top\":3,\"strictFilters\":[{\"name\":\"dialogName\",\"value\":\"outer\"}],\"scoreThreshold\":0.3,\"context\":null,\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_TopNAnswer.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .WithContent("{\"question\":\"QnaMaker_ReturnsAnswerWithIntent\",\"top\":3,\"strictFilters\":[{\"name\":\"dialogName\",\"value\":\"outer\"}],\"scoreThreshold\":0.3,\"context\":null,\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswerWithIntent.json"));

            return CreateQnAMakerActionDialog(mockHttp);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task LogsTelemetry(bool logPersonalInformation)
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();
            var response = JsonConvert.DeserializeObject<QueryResults>(await File.ReadAllTextAsync(GetFilePath("QnaMaker_ReturnsAnswer.json")));
            var recognizer = (QnAMakerRecognizer)rootDialog.Recognizer;
            var telemetryClient = new Mock<IBotTelemetryClient>();
            recognizer.TelemetryClient = telemetryClient.Object;
            recognizer.LogPersonalInformation = logPersonalInformation;

            await CreateFlow(rootDialog, nameof(QnAMakerRecognizer_WithAnswer))
                .Send(QnAReturnsAnswerText)
                    .AssertReply(response.Answers[0].Answer)
                    .AssertReply("done")
                .StartTestAsync();

            ValidateTelemetry(QnAReturnsAnswerText, telemetryClient, logPersonalInfo: logPersonalInformation, callCount: 1);
        }

        [Fact]
        public void LogPiiIsFalseByDefault()
        {
            var recognizer = new QnAMakerRecognizer()
            {
                HostName = Hostname,
                EndpointKey = EndpointKey,
                KnowledgeBaseId = KnowledgeBaseId
            };
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(new TestAdapter(), activity);
            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var (logPersonalInfo, _) = recognizer.LogPersonalInformation.TryGetValue(dc.State);

            // Should be false by default, when not specified by user.
            Assert.False(logPersonalInfo);
        }

        [Fact]
        public async Task QnAMakerRecognizer_WithTopNAnswer()
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(await File.ReadAllTextAsync(GetFilePath("QnaMaker_TopNAnswer.json")));

            await CreateFlow(rootDialog, nameof(QnAMakerRecognizer_WithTopNAnswer))
            .Send("QnaMaker_TopNAnswer")
                .AssertReply(response.Answers[0].Answer)
                .AssertReply("done")
            .StartTestAsync();
        }

        [Fact]
        public async Task QnAMakerRecognizer_WithAnswer()
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(await File.ReadAllTextAsync(GetFilePath("QnaMaker_ReturnsAnswer.json")));

            await CreateFlow(rootDialog, nameof(QnAMakerRecognizer_WithAnswer))
            .Send("QnaMaker_ReturnsAnswer")
                .AssertReply(response.Answers[0].Answer)
                .AssertReply("done")
            .Send("QnaMaker_ReturnsNoAnswer")
                .AssertReply("Wha?")
            .StartTestAsync();
        }

        [Fact]
        public async Task QnAMakerRecognizer_WithNoAnswer()
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();

            await CreateFlow(rootDialog, nameof(QnAMakerRecognizer_WithNoAnswer))
            .Send("QnaMaker_ReturnsNoAnswer")
                .AssertReply("Wha?")
            .StartTestAsync();
        }

        [Fact]
        public async Task QnAMakerRecognizer_WithIntent()
        {
            var rootDialog = QnAMakerRecognizer_DialogBase();

            await CreateFlow(rootDialog, nameof(QnAMakerRecognizer_WithIntent))
            .Send("QnaMaker_ReturnsAnswerWithIntent")
                .AssertReply("DeferToRecognizer_xxx")
            .StartTestAsync();
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

            var dm = new DialogManager(rootDialog)
                .UseResourceExplorer(_qnaMakerRecognizerFixture.ResourceExplorer)
                .UseLanguageGeneration();

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken).ConfigureAwait(false);
            });
        }

        private AdaptiveDialog CreateQnAMakerActionDialog(MockHttpMessageHandler mockHttp)
        {
            var client = new HttpClient(mockHttp);

            var rootDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Recognizer = new QnAMakerRecognizer
                {
                    KnowledgeBaseId = KnowledgeBaseId,
                    HostName = Hostname,
                    EndpointKey = EndpointKey,
                    HttpClient = client
                },
                Triggers = new List<OnCondition>
                {
                    new OnQnAMatch
                    {
                        Actions = new List<Dialog>
                        {
                            new SendActivity
                            {
                                Activity = new ActivityTemplate("${@answer}")
                            },
                            new AssertCondition
                            {
                                Condition = "count(turn.recognized.entities.answer) == 1",
                                Description = "If there is a match there should only be 1 answer"
                            },
                            new AssertCondition
                            {
                                Condition = "turn.recognized.entities.$instance.answer[0].startIndex == 0",
                                Description = "startIndex should be 0",
                            },
                            new AssertCondition
                            {
                                Condition = "turn.recognized.entities.$instance.answer[0].endIndex != null",
                                Description = "endIndex should not be null",
                            },
                            new AssertCondition
                            {
                                Condition = "turn.recognized.answers[0].answer != null",
                                Description = "There should be answers object"
                            },
                            new SendActivity
                            {
                                Activity = new ActivityTemplate("done")
                            }
                        }
                    },
                    new OnIntent
                    {
                        Intent = "DeferToRecognizer_xxx",
                        Actions = new List<Dialog>
                        {
                            new SendActivity
                            {
                                Activity = new ActivityTemplate("DeferToRecognizer_xxx")
                            }
                        }
                    },
                    new OnUnknownIntent
                    {
                        Actions = new List<Dialog>
                        {
                            new SendActivity("Wha?")
                        }
                    }
                }
            };

            return rootDialog;
        }

        private string GetV2LegacyRequestUrl() => $"{Hostname}/v2.0/knowledgebases/{KnowledgeBaseId}/generateanswer";

        private string GetV3LegacyRequestUrl() => $"{Hostname}/v3.0/knowledgebases/{KnowledgeBaseId}/generateanswer";

        private string GetRequestUrl() => $"{Hostname}/knowledgebases/{KnowledgeBaseId}/generateanswer";

        private string GetTrainRequestUrl() => $"{Hostname}/knowledgebases/{KnowledgeBaseId}/train";

        private Stream GetResponse(string fileName)
        {
            var path = GetFilePath(fileName);
            return File.OpenRead(path);
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
        }

        private void ValidateTelemetry(string text, Mock<IBotTelemetryClient> telemetryClient, bool logPersonalInfo, int callCount)
        {
            var eventName = $"{nameof(QnAMakerRecognizer)}Result";
            var expectedTelemetryProps = GetExpectedTelemetryProps(text, logPersonalInfo);
            var actualTelemetryProps = (Dictionary<string, string>)telemetryClient.Invocations[callCount].Arguments[1];

            telemetryClient.Verify(
                client => client.TrackEvent(
                    eventName,
                    It.Is<Dictionary<string, string>>(d => HasValidTelemetryProps(expectedTelemetryProps, actualTelemetryProps)),
                    null),
                Times.Exactly(callCount));
        }

        private Dictionary<string, string> GetExpectedTelemetryProps(string text, bool logPersonalInformation)
        {
            var props = new Dictionary<string, string>()
            {
                { "TopIntent", "QnAMatch" },
                { "TopIntentScore", "1.0" },
                { "Intents", "{\"QnAMatch\":{\"score\":1.0}}" },
                { "Entities", "{\r\n  \"answer\": [\r\n    \"BaseCamp: You can use a damp rag to clean around the Power Pack\"\r\n  ],\r\n  \"$instance\": {\r\n    \"answer\": [\r\n      {\r\n        \"questions\": [\r\n          \"how do I clean the stove?\"\r\n        ],\r\n        \"answer\": \"BaseCamp: You can use a damp rag to clean around the Power Pack\",\r\n        \"score\": 1.0,\r\n        \"metadata\": [],\r\n        \"source\": \"Editorial\",\r\n        \"id\": 5,\r\n        \"context\": {\r\n          \"prompts\": [\r\n            {\r\n              \"displayOrder\": 0,\r\n              \"qnaId\": 55,\r\n              \"displayText\": \"Where can I buy?\",\r\n              \"qna\": null\r\n            }\r\n          ]\r\n        },\r\n        \"startIndex\": 0,\r\n        \"endIndex\": 22\r\n      }\r\n    ]\r\n  }\r\n}" },
                { "AdditionalProperties", "{\"answers\":[{\"questions\":[\"how do I clean the stove?\"],\"answer\":\"BaseCamp: You can use a damp rag to clean around the Power Pack\",\"score\":1.0,\"metadata\":[],\"source\":\"Editorial\",\"id\":5,\"context\":{\"prompts\":[{\"displayOrder\":0,\"qnaId\":55,\"displayText\":\"Where can I buy?\",\"qna\":null}]}}]}" }
            };

            if (logPersonalInformation == true)
            {
                // Out-of-the-box, QnAMakerRecognizer does not alter text
                // So for these tests, we always expect it to be null
                props.Add("AlteredText", null);
                props.Add("Text", text);
            }

            return props;
        }

        private bool HasValidTelemetryProps(IDictionary<string, string> expected, IDictionary<string, string> actual)
        {
            if (expected.Count != actual.Count)
            {
                return false;
            }

            foreach (var property in actual)
            {
                if (!expected.ContainsKey(property.Key))
                {
                    return false;
                }

                if (property.Key == "Entities")
                {
                    if (!property.Value.Contains("answer"))
                    {
                        return false;
                    }
                }
                else
                {
                    if (property.Value != expected[property.Key])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
