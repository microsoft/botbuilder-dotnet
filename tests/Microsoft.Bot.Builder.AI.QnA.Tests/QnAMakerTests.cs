// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    [TestClass]
    public class QnAMakerTests
    {
        private const string _knowlegeBaseId = "dummy-id";
        private const string _endpointKey = "dummy-key";
        private const string _hostname = "https://dummy-hostname.azurewebsites.net/qnamaker";

        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(AdaptiveDialog ruleDialog)
        {
            var resourceExplorer = new ResourceExplorer();
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, conversationState)
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration(resourceExplorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(ruleDialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }


        [TestMethod]
        public async Task QnAMakerDialog_Answers()
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var rootDialog = createDialog(mockHttp);

            await CreateFlow(rootDialog)
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .Send("how do I clean the stove?")
                .AssertReply("BaseCamp: You can use a damp rag to clean around the Power Pack")
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task QnAMakerDialog_NoAnswers()
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_TestThreshold.json"));

            var rootDialog = createDialog(mockHttp);

            await CreateFlow(rootDialog)
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .Send("how do I clean the stove?")
                .AssertReply("I didn't understand that.")
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .StartTestAsync();
        }

        private AdaptiveDialog createDialog(MockHttpMessageHandler mockHttp)
        {
            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });

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
                                        new QnAMakerDialog(qnamaker:qna )
                                        {
                                            OutputBinding = "turn.LastResult"
                                        },
                                        new IfCondition()
                                        {
                                             Condition = "turn.LastResult == false",
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
            return rootDialog;
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TraceActivity()
        {
            // Mock Qna
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));
            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });

            // Invoke flow which uses mock
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                // Simulate Qna Lookup
                if (context?.Activity?.Text.CompareTo("how do I clean the stove?") == 0)
                {
                    var results = await qna.GetAnswersAsync(context);
                    Assert.IsNotNull(results);
                    Assert.AreEqual(results.Length, 1, "should get one result");
                    StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
                }

                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("how do I clean the stove?")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:how do I clean the stove?")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Validate Trace Activity created
            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(7, pagedResult.Items.Length);
            Assert.AreEqual("how do I clean the stove?", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.IsTrue(pagedResult.Items[1].Type.CompareTo(ActivityTypes.Trace) == 0);
            QnAMakerTraceInfo traceInfo = ((JObject)((ITraceActivity)pagedResult.Items[1]).Value).ToObject<QnAMakerTraceInfo>();
            Assert.IsNotNull(traceInfo);
            Assert.IsNotNull(pagedResult.Items[2].AsTypingActivity());
            Assert.AreEqual("echo:how do I clean the stove?", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.AreEqual("bar", pagedResult.Items[4].AsMessageActivity().Text);
            Assert.IsNotNull(pagedResult.Items[5].AsTypingActivity());
            Assert.AreEqual("echo:bar", pagedResult.Items[6].AsMessageActivity().Text);
            foreach (var activity in pagedResult.Items)
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(activity.Id));
                Assert.IsTrue(activity.Timestamp > default(DateTimeOffset));
            }

        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QnaMaker_TraceActivity_EmptyText()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "",
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount()
            };
            var context = new TurnContext(adapter, activity);


            var results = await qna.GetAnswersAsync(context);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QnaMaker_TraceActivity_NullText()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = null,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount()
            };
            var context = new TurnContext(adapter, activity);

            var results = await qna.GetAnswersAsync(context);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QnaMaker_TraceActivity_NullContext()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            var results = await qna.GetAnswersAsync(null);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QnaMaker_TraceActivity_BadMessage()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var activity = new Activity
            {
                Type = ActivityTypes.Trace,
                Text = "My Text",
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount()
            };
            var context = new TurnContext(adapter, activity);


            var results = await qna.GetAnswersAsync(context);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QnaMaker_TraceActivity_NullActivity()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var context = new MyTurnContext(adapter, null);


            var results = await qna.GetAnswersAsync(context);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswer()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswer_Configuration()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var service = new QnAMakerService
            {
                KbId = _knowlegeBaseId,
                EndpointKey = _endpointKey,
                Hostname = _hostname
            };

            var options = new QnAMakerOptions
            {
                Top = 1
            };

            var client = new HttpClient(mockHttp);
            var qna = new QnAMaker(service, options, client);

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswerWithFiltering()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_UsesStrictFilters_ToReturnAnswer.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(interceptHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                });

            var options = new QnAMakerOptions
            {
                StrictFilters = new Metadata[]
                {
                    new Metadata() { Name = "topic", Value = "value" }
                },
                Top = 1
            };

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"), options);
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
            Assert.AreEqual("topic", results[0].Metadata[0].Name);
            Assert.AreEqual("value", results[0].Metadata[0].Value);

            // verify we are actually passing on the options
            var obj = JObject.Parse(interceptHttp.Content);
            Assert.AreEqual(1, obj["top"].Value<int>());
            Assert.AreEqual("topic", obj["strictFilters"][0]["name"].Value<string>());
            Assert.AreEqual("value", obj["strictFilters"][0]["value"].Value<string>());
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_SetScoreThresholdWhenThresholdIsZero()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var qnaWithZeroValueThreshold = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions()
                {
                    ScoreThreshold = 0.0F
                });

            var results = await qnaWithZeroValueThreshold
                .GetAnswersAsync(GetContext("how do I clean the stove?"), new QnAMakerOptions() { Top = 1 });

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestThreshold()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_TestThreshold.json"));

            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1,
                    ScoreThreshold = 0.99F
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 0, "should get zero result because threshold");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void QnaMaker_Test_ScoreThresholdTooLarge_OutOfRange()
        {
            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowlegeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname
            };

            var tooLargeThreshold = new QnAMakerOptions
            {
                ScoreThreshold = 1.1F,
                Top = 1
            };

            var qnaWithLargeThreshold = new QnAMaker(endpoint, tooLargeThreshold);

        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void QnaMaker_Test_ScoreThresholdTooSmall_OutOfRange()
        {
            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowlegeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname
            };

            var tooSmallThreshold = new QnAMakerOptions
            {
                ScoreThreshold = -9000.0F,
                Top = 1
            };

            var qnaWithSmallThreshold = new QnAMaker(endpoint, tooSmallThreshold);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void QnaMaker_Test_Top_OutOfRange()
        {
            var qna = new QnAMaker(
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = -1,
                    ScoreThreshold = 0.5F
                });
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public void QnaMaker_Test_Endpoint_EmptyKbId()
        {
            var qnaNullEndpoint = new QnAMaker(
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = "",
                    EndpointKey = _endpointKey,
                    Host = _hostname
                }
            );
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public void QnaMaker_Test_Endpoint_EmptyEndpointKey()
        {
            var qnaNullEndpoint = new QnAMaker(
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = "",
                    Host = _hostname
                }
            );
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public void QnaMaker_Test_Endpoint_EmptyHost()
        {
            var qnaNullEndpoint = new QnAMaker(
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = ""
                }
            );
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_UserAgent()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(interceptHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));

            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");

            // Verify that we added the bot.builder package details.
            Assert.IsTrue(interceptHttp.UserAgent.Contains("Microsoft.Bot.Builder.AI.QnA/4"));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task QnaMaker_V2LegacyEndpoint_ConvertsToHaveIdPropertyInResult()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetV2LegacyRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_LegacyEndpointAnswer.json"));

            var v2LegacyEndpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowlegeBaseId,
                EndpointKey = _endpointKey,
                Host = $"{_hostname}/v2.0"
            };

            var v2Qna = GetQnAMaker(mockHttp, v2LegacyEndpoint);

            var v2legacyResult = await v2Qna.GetAnswersAsync(GetContext("How do I be the best?"));

            Assert.IsNotNull(v2legacyResult);
            Assert.IsTrue(v2legacyResult[0]?.Id != null);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_V3LegacyEndpoint_ConvertsToHaveIdPropertyInResult()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetV3LegacyRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_LegacyEndpointAnswer.json"));

            var v3LegacyEndpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowlegeBaseId,
                EndpointKey = _endpointKey,
                Host = $"{_hostname}/v3.0"
            };

            var v3Qna = GetQnAMaker(mockHttp, v3LegacyEndpoint);

            var v3legacyResult = await v3Qna.GetAnswersAsync(GetContext("How do I be the best?"));

            Assert.IsNotNull(v3legacyResult);
            Assert.IsTrue(v3legacyResult[0]?.Id != null);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswerWithMetadataBoost()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswersWithMetadataBoost.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(
                interceptHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                });

            var options = new QnAMakerOptions
            {
                MetadataBoost = new Metadata[]
                {
                    new Metadata() { Name = "artist", Value = "drake" }
                },
                Top = 1
            };

            var results = await qna.GetAnswersAsync(GetContext("who loves me?"), options);

            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "Kiki");


        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestThresholdInQueryOption()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer_GivenScoreThresholdQueryOption.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(
                interceptHttp,
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                });

            var queryOptionsWithScoreThreshold = new QnAMakerOptions()
            {
                ScoreThreshold = 0.5F,
                Top = 2
            };

            var result = await qna.GetAnswersAsync(
                    GetContext("What happens when you hug a porcupine?"),
                    queryOptionsWithScoreThreshold
            );

            Assert.IsNotNull(result);

            var obj = JObject.Parse(interceptHttp.Content);
            Assert.AreEqual(2, obj["top"].Value<int>());
            Assert.AreEqual(0.5F, obj["scoreThreshold"].Value<float>());
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task QnaMaker_Test_UnsuccessfulResponse()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond(System.Net.HttpStatusCode.BadGateway);

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
        }

        private string GetV2LegacyRequestUrl() => $"{_hostname}/v2.0/knowledgebases/{_knowlegeBaseId}/generateanswer";
        private string GetV3LegacyRequestUrl() => $"{_hostname}/v3.0/knowledgebases/{_knowlegeBaseId}/generateanswer";

        private string GetRequestUrl() => $"{_hostname}/knowledgebases/{_knowlegeBaseId}/generateanswer";

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
            return File.OpenRead(path);
        }

        /// <summary>
        /// Return a stock Mocked Qna thats loaded with QnaMaker_ReturnsAnswer.json
        /// 
        /// Used for tests that just require any old qna instance
        /// </summary>
        /// <returns></returns>
        private QnAMaker QnaReturnsAnswer()
        {
            // Mock Qna
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                    .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));
            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });
            return qna;
        }

        private QnAMaker GetQnAMaker(HttpMessageHandler messageHandler, QnAMakerEndpoint endpoint, QnAMakerOptions options = null)
        {
            var client = new HttpClient(messageHandler);
            return new QnAMaker(endpoint, options, client);
        }

        private TurnContext GetContext(string utterance)
        {
            var b = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var a = new Activity
            {
                Type = ActivityTypes.Message,
                Text = utterance,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount()
            };
            return new TurnContext(b, a);

        }
    }

    class MyTurnContext : ITurnContext
    {

        public MyTurnContext(BotAdapter adapter, Activity activity)
        {
            Activity = activity;
            Adapter = adapter;
        }
        public BotAdapter Adapter { get; }

        public TurnContextStateCollection TurnState => throw new NotImplementedException();

        public Activity Activity { get; }

        public bool Responded => throw new NotImplementedException();

        public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }


}
