// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
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

            var mockScore = new List<Result> { mockResult }.AsReadOnly();
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = "fakePath",
                SnapshotFile = "fakePath"
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

        [Fact]
        public async Task TestIntentNoneRecognize()
        {
            var mockResult1 = new Result
            {
                Score = 0.3,
                Label = new Label { Name = "mockLabel" }
            };
            var mockResultNone = new Result
            {
                Score = 0.8,
                Label = new Label { Name = "None" }
            };
            var mockResult2 = new Result
            {
                Score = 0.6,
                Label = new Label { Name = "mockLabel2" }
            };

            var mockScore = new List<Result> { mockResult1, mockResultNone, mockResult2 }.AsReadOnly();
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = "fakePath",
                SnapshotFile = "fakePath"
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.Equal(3, result.Intents.Count);
            var results = (List<Result>)result.Properties[OrchestratorRecognizer.ResultProperty];
            Assert.Equal(Recognizer.NoneIntent, results[0].Label.Name);
            Assert.Equal(3, results.Count);
            Assert.True(result.Intents.ContainsKey("mockLabel"));
            Assert.Equal(0.3, result.Intents["mockLabel"].Score);
            Assert.True(result.Intents.ContainsKey(Recognizer.NoneIntent));
            Assert.Equal(1.0, result.Intents[Recognizer.NoneIntent].Score);
            Assert.True(result.Intents.ContainsKey("mockLabel2"));
            Assert.Equal(0.6, result.Intents["mockLabel2"].Score);
        }

        [Fact]
        public async Task TestIntentRecognizeLowScore()
        {
            var mockResult = new Result
            {
                Score = 0.1,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult };
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = "fakePath",
                SnapshotFile = "fakePath"
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.Equal(1.0, result.Intents["None"].Score);
        }

        [Fact]
        public async Task TestIntentRecognizeEmptyMessage()
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
                ModelFolder = "fakePath",
                SnapshotFile = "fakePath"
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text(string.Empty);
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.Empty(result.Text);
        }

        [Fact]
        public void TestOrchestratorRecognizerNullModelFolder()
        {
            Assert.Throws<ArgumentNullException>(() => new OrchestratorRecognizer(null, null));
        }

        [Fact]
        public void TestOrchestratorRecognizerNullSnapshotFile()
        {
            Assert.Throws<ArgumentNullException>(() => new OrchestratorRecognizer(string.Empty, null));
        }

        [Fact]
        public void TestOrchestratorRecognizerWithInvalidPath()
        {
            Assert.Throws<InvalidOperationException>(() => new OrchestratorRecognizer("C:/", "C:/"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestIntentRecognizeLogsTelemetry(bool logPersonalInformation)
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

            var mockScore = new List<Result> { mockResult1, mockResult2 }.AsReadOnly();
            var mockResolver = new MockResolver(mockScore);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = "fakePath",
                SnapshotFile = "fakePath",
                TelemetryClient = telemetryClient.Object,
            };

            recognizer.LogPersonalInformation = logPersonalInformation;

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);

            Assert.Equal(logPersonalInformation, recognizer.LogPersonalInformation);
            Assert.Equal(2, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("mockLabel"));
            Assert.Equal(0.9, result.Intents["mockLabel"].Score);
            ValidateTelemetry(recognizer, telemetryClient, dc, activity, result, callCount: 1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestIntentRecognizeNoneIntentTelemetry(bool logPersonalInformation)
        {
            var mockResult1 = new Result
            {
                Score = 0.3,
                Label = new Label { Name = "FOOBAR", Type = LabelType.Intent },
            };

            var mockScore = new List<Result> { mockResult1 }.AsReadOnly();
            var mockResolver = new MockResolver(mockScore);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = "fakePath",
                SnapshotFile = "fakePath",
                TelemetryClient = telemetryClient.Object,
                LogPersonalInformation = logPersonalInformation
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);

            Assert.Equal(logPersonalInformation, recognizer.LogPersonalInformation);
            Assert.Equal(2, result.Intents.Count);
            Assert.True(result.Intents.ContainsKey("None"));
            Assert.Equal(1.0, result.Intents["None"].Score);
            ValidateNoneTelemetry(recognizer, telemetryClient, dc, activity, result, callCount: 1);
        }

        [Fact]
        public async Task TestExternalEntityRecognition()
        {
            var mockResult = new Result
            {
                Score = 0.9,
                Label = new Label { Name = "mockLabel" }
            };

            var mockScore = new List<Result> { mockResult }.AsReadOnly();
            var mockEntityResult = new Result
            {
                Score = 0.75,
                Label = new Label { Name = "mockEntityLabel", Type = LabelType.Entity, Span = new Span { Offset = 17, Length = 7 } },
            };

            var mockEntityScore = new List<Result> { mockEntityResult }.AsReadOnly();
            var mockResolver = new MockResolver(mockScore, mockEntityScore);
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = "fakePath",
                SnapshotFile = "fakePath",
                ExternalEntityRecognizer = new TestExternalEntityRecognizer()
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("turn on light in room 12");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.NotNull(result.Entities);
            Assert.Equal(new JValue("12"), result.Entities["number"][0]);
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
            }.AsReadOnly();
            var mockResolver = new MockResolver(mockScore);
            var recognizer = new OrchestratorRecognizer(string.Empty, string.Empty, mockResolver)
            {
                ModelFolder = "fakePath",
                SnapshotFile = "fakePath",
                DetectAmbiguousIntents = true,
                DisambiguationScoreThreshold = 0.5
            };

            var adapter = new TestAdapter(TestAdapter.CreateConversation("ds"));
            var activity = MessageFactory.Text("12");
            var context = new TurnContext(adapter, activity);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            var result = await recognizer.RecognizeAsync(dc, activity, default);
            Assert.True(result.Intents.ContainsKey("ChooseIntent"));
        }

        private static void ValidateTelemetry(OrchestratorRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, RecognizerResult result, int callCount)
        {
            var eventName = GetEventName(recognizer.GetType().Name);
            var logPersonalInfo = recognizer.LogPersonalInformation;
            var expectedTelemetryProps = GetExpectedTelemetryProps(activity, result, logPersonalInfo);
            var actualTelemetryProps = (Dictionary<string, string>)telemetryClient.Invocations[callCount - 1].Arguments[1];

            telemetryClient.Verify(
                client => client.TrackEvent(
                    eventName,
                    It.Is<Dictionary<string, string>>(d => HasValidTelemetryProps(expectedTelemetryProps, actualTelemetryProps)),
                    null),
                Times.Exactly(callCount));
        }

        private static void ValidateNoneTelemetry(OrchestratorRecognizer recognizer, Mock<IBotTelemetryClient> telemetryClient, DialogContext dc, IActivity activity, RecognizerResult result, int callCount)
        {
            var eventName = GetEventName(recognizer.GetType().Name);
            var logPersonalInfo = recognizer.LogPersonalInformation;
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

        /// <summary>
        /// A recognizer to test that the external entity recognizer is called.
        /// </summary>
        private class TestExternalEntityRecognizer : Recognizer
        {
            public override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
            {
                var result = new RecognizerResult();
                result.Entities.Merge(JObject.Parse("{ 'number': ['12'] }"));
                return Task.FromResult(result);
            }
        }
    }
}
