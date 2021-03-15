// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class RecognizerTest
    {
        [Fact]
        public async Task LogsTelemetry()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();

            var recognizer = new MyRecognizerSubclass { TelemetryClient = telemetryClient.Object };
            var adapter = new TestAdapter(TestAdapter.CreateConversation("RecognizerLogsTelemetry"));
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(adapter, activity);
            var dc = new DialogContext(new DialogSet(), context, new DialogState());

            var result = await recognizer.RecognizeAsync(dc, activity);

            var actualTelemetryProps = (IDictionary<string, string>)telemetryClient.Invocations[0].Arguments[1];

            Assert.NotNull(result);
            Assert.Equal("hi", actualTelemetryProps["Text"]);
            Assert.Null(actualTelemetryProps["AlteredText"]);
            actualTelemetryProps.TryGetValue("TopIntent", out var intent);
            Assert.True(intent == "myTestIntent");
            Assert.Equal("1.0", actualTelemetryProps["TopIntentScore"]);
            var hasMyTestIntent = actualTelemetryProps["Intents"].Contains("myTestIntent");
            Assert.True(hasMyTestIntent);
            Assert.Equal("{}", actualTelemetryProps["Entities"]);
            Assert.Null(actualTelemetryProps["AdditionalProperties"]);

            telemetryClient.Verify(
                client => client.TrackEvent(
                    "MyRecognizerSubclassResult",
                    It.IsAny<IDictionary<string, string>>(),
                    null),
                Times.Once());
        }

        /// <summary>
        /// Subclass to test <see cref="Recognizer.FillRecognizerResultTelemetryProperties(RecognizerResult, Dictionary{string,string}, DialogContext)"/> functionality.
        /// </summary>
        private class MyRecognizerSubclass : Recognizer
        {
            public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
            {
                var text = activity.Text ?? string.Empty;

                var recognizerResult = await Task.FromResult(new RecognizerResult
                {
                    Text = text,
                    AlteredText = null,
                    Intents = new Dictionary<string, IntentScore>
                    {
                        {
                            "myTestIntent", new IntentScore
                            {
                                Score = 1.0,
                                Properties = new Dictionary<string, object>()
                            }
                        }
                    },
                    Entities = new JObject(),
                    Properties = new Dictionary<string, object>()
                });

                TrackRecognizerResult(dialogContext, $"{nameof(MyRecognizerSubclass)}Result", FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties, dialogContext), telemetryMetrics);

                return recognizerResult;
            }
        }
    }
}
