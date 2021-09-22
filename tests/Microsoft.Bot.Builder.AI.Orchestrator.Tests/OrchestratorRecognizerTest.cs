// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.AI.Orchestrator.Tests
{
    public class OrchestratorRecognizerTest
    {
        [Fact]
        public async Task LogsTelemetryThrowsArgumentNullExcetionOnNullDialogContext()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();

            var recognizer = new MyRecognizerSubclass { TelemetryClient = telemetryClient.Object };
            var activity = MessageFactory.Text("hi");

            await Assert.ThrowsAsync<ArgumentNullException>(() => recognizer.RecognizeAsync(null, activity));
        }

        /// <summary>
        /// Subclass to test <see cref="OrchestratorRecognizer.FillRecognizerResultTelemetryProperties(RecognizerResult, Dictionary{string,string}, DialogContext)"/> functionality.
        /// </summary>
        private class MyRecognizerSubclass : OrchestratorRecognizer
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
