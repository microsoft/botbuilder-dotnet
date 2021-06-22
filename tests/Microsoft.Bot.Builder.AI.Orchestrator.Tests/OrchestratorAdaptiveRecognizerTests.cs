// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.BotFramework.Orchestrator;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.AI.Orchestrator.Tests
{
    public class OrchestratorAdaptiveRecognizerTests
    {
        [Fact]
        public async Task TestIntentRecognize()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = new StringExpression("fakePath"),
                SnapshotFile = new StringExpression("fakePath")
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.Equal(1, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("mockLabel"));
            Assert.Equal(0.9, result.Intents["mockLabel"].Score);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public async Task TestIntentRecognizeLogsTelemetry(bool? logPersonalInformation)
        {
            var mockResult1 = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };
            var mockResult2 = new Result
            {
                Score = 0.8,
                Label = new Label { Name = "mockLabel2" }
            };

            var mockScore = new List<Result> { mockResult1, mockResult2 };
            var mockResolver = new MockResolver(mockScore);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = new StringExpression("fakePath"),
                SnapshotFile = new StringExpression("fakePath"),
                TelemetryClient = telemetryClient.Object,
            };

            if (logPersonalInformation != null)
            {
                recognizer.LogPersonalInformation = logPersonalInformation;
            }

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);

            if (logPersonalInformation == null)
            {
                // Should be false by default, when not specified by user.
                var (logPersonalInfo, _) = recognizer.LogPersonalInformation.TryGetValue(dc.State);
                Assert.False(logPersonalInfo);
            }

            Assert.Equal(2, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("mockLabel"));
            Assert.Equal(0.9, result.Intents["mockLabel"].Score);
            ValidateTelemetry(recognizer, telemetryClient, dc, activity, result, callCount: 1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public async Task TestIntentRecognizeNoneIntentTelemetry(bool? logPersonalInformation)
        {
            var mockResult1 = new Result
            {
                Score = 0.3,
                Label = new Label { Name = "FOOBAR", Type = LabelType.Intent },
            };

            var mockScore = new List<Result> { mockResult1 };
            var mockResolver = new MockResolver(mockScore);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = new StringExpression("fakePath"),
                SnapshotFile = new StringExpression("fakePath"),
                TelemetryClient = telemetryClient.Object,
            };

            if (logPersonalInformation != null)
            {
                recognizer.LogPersonalInformation = logPersonalInformation;
            }

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);

            if (logPersonalInformation == null)
            {
                // Should be false by default, when not specified by user.
                var (logPersonalInfo, _) = recognizer.LogPersonalInformation.TryGetValue(dc.State);
                Assert.False(logPersonalInfo);
            }

            Assert.Equal(2, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("None"));
            Assert.Equal(1.0, result.Intents["None"].Score);
            ValidateNoneTelemetry(recognizer, telemetryClient, dc, activity, result, callCount: 1);
        }

        [Fact]
        public async Task TestEntityRecognize()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult };
            var mockEntityResult = new Result
            {
                Score = 0.75,
                Label = new Label { Name = "mockEntityLabel", Type = LabelType.Entity, Span = new Span { Offset = 17, Length = 7 } },
            };

            var mockEntityScore = new List<Result> { mockEntityResult };
            var mockResolver = new MockResolver(mockScore, mockEntityScore);
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = new StringExpression("fakePath"),
                SnapshotFile = new StringExpression("fakePath"),
                ScoreEntities = true,
                ExternalEntityRecognizer = new NumberEntityRecognizer()
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("turn on light in room 12");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.NotNull(result.Entities);
            Assert.Equal(new JValue("12"), result.Entities["number"][0]);
            var resolution = result.Entities["$instance"]["number"][0]["resolution"];
            Assert.Equal(new JValue("integer"), resolution["subtype"]);
            Assert.Equal(new JValue("12"), resolution["value"]);

            Assert.True(result.Entities.ContainsKey("mockEntityLabel"));
            Assert.Equal(0.75, result.Entities["mockEntityLabel"][0]["score"]);
            Assert.Equal("room 12", result.Entities["mockEntityLabel"][0]["text"]);
            Assert.Equal(17, result.Entities["mockEntityLabel"][0]["start"]);
            Assert.Equal(24, result.Entities["mockEntityLabel"][0]["end"]);
        }

        [Fact]
        public async Task TestAmbiguousResults()
        {
            var mockResult1 = new Result
            {
                Score = 0.61,
                Label = new Label { Name = "mockLabel1" }
            };

            var mockResult2 = new Result
            {
                Score = 0.62,
                Label = new Label { Name = "mockLabel2" }
            };

            var mockScore = new List<Result>
            {
                mockResult1,
                mockResult2
            };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = new StringExpression("fakePath"),
                SnapshotFile = new StringExpression("fakePath"),
                DetectAmbiguousIntents = new BoolExpression(true),
                DisambiguationScoreThreshold = new NumberExpression(0.5)
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("12");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.True(result.Intents.ContainsKey("ChooseIntent"));
        }

        private static void ValidateTelemetry(AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, RecognizerResult result, int callCount)
        {
            var eventName = GetEventName(recognizer.GetType().Name);
            var (logPersonalInfo, error) = recognizer.LogPersonalInformation.TryGetValue(dc.State);
            var expectedTelemetryProps = GetExpectedTelemetryProps(activity, result, logPersonalInfo);
            var actualTelemetryProps = (Dictionary<string, string>)telemetryClient.Invocations[callCount - 1].Arguments[1];

            telemetryClient.Verify(
                client => client.TrackEvent(
                    eventName,
                    It.Is<Dictionary<string, string>>(d => HasValidTelemetryProps(expectedTelemetryProps, actualTelemetryProps)),
                    null),
                Times.Exactly(callCount));
        }

        private static void ValidateNoneTelemetry(AdaptiveRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, RecognizerResult result, int callCount)
        {
            var eventName = GetEventName(recognizer.GetType().Name);
            var (logPersonalInfo, error) = recognizer.LogPersonalInformation.TryGetValue(dc.State);
            var expectedTelemetryProps = GetExpectedNoneTelemetryProps(activity, result, logPersonalInfo);
            var actualTelemetryProps = (Dictionary<string, string>)telemetryClient.Invocations[callCount - 1].Arguments[1];

            telemetryClient.Verify(
                client => client.TrackEvent(
                    eventName,
                    It.Is<Dictionary<string, string>>(d => HasValidTelemetryProps(expectedTelemetryProps, actualTelemetryProps)),
                    null),
                Times.Exactly(callCount));
        }

        private static Dictionary<string, string> GetExpectedTelemetryProps(IActivity activity, RecognizerResult result, bool logPersonalInformation)
        {
            var props = new Dictionary<string, string>
            {
                { "TopIntent", "mockLabel" },
                { "TopIntentScore", "0.9" },
                { "NextIntent", "mockLabel2" },
                { "NextIntentScore", "0.8" },
                { "Intents", "{\"mockLabel\":{\"score\":0.9},\"mockLabel2\":{\"score\":0.8}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", "{\"result\":[{\"Label\":{\"Type\":0,\"Name\":\"mockLabel\",\"Span\":{\"Offset\":0,\"Length\":0}},\"Score\":0.9,\"ClosestText\":null},{\"Label\":{\"Type\":0,\"Name\":\"mockLabel2\",\"Span\":{\"Offset\":0,\"Length\":0}},\"Score\":0.8,\"ClosestText\":null}]}" }
            };

            if (logPersonalInformation)
            {
                props.Add("Text", activity.AsMessageActivity().Text);
                props.Add("AlteredText", result.AlteredText);
            }

            return props;
        }

        private static Dictionary<string, string> GetExpectedNoneTelemetryProps(IActivity activity, RecognizerResult result, bool logPersonalInformation)
        {
            var props = new Dictionary<string, string>
            {
                { "TopIntent", "None" },
                { "TopIntentScore", "1.0" },
                { "NextIntent", "FOOBAR" },
                { "NextIntentScore", "0.3" },
                { "Intents", "{\"None\":{\"score\":1.0},\"FOOBAR\":{\"score\":0.3}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", "{\"result\":[{\"Label\":{\"Type\":1,\"Name\":\"None\",\"Span\":{\"Offset\":0,\"Length\":0}},\"Score\":1.0,\"ClosestText\":null},{\"Label\":{\"Type\":1,\"Name\":\"FOOBAR\",\"Span\":{\"Offset\":0,\"Length\":0}},\"Score\":0.3,\"ClosestText\":null}]}" }
            };

            if (logPersonalInformation)
            {
                props.Add("Text", activity.AsMessageActivity().Text);
                props.Add("AlteredText", result.AlteredText);
            }

            return props;
        }

        private static string GetEventName(string recognizerName)
        {
            return $"{recognizerName}Result";
        }

        private static bool HasValidTelemetryProps(IDictionary<string, string> expected, IDictionary<string, string> actual)
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

                if (property.Value != expected[property.Key])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
