// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;

namespace Microsoft.Bot.Builder.AI.LuisV3.Tests
{
    public class OverrideFillRecognizer : LuisRecognizer
    {
        public OverrideFillRecognizer(LuisApplication application, LuisRecognizerOptions recognizerOptions = null)
           : base(application, recognizerOptions)
        {
        }

        protected override void OnRecognizerResult(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var properties = FillLuisEventProperties(recognizerResult, turnContext, telemetryProperties);

            properties.TryAdd("MyImportantProperty", "myImportantValue");

            // Log event
            TelemetryClient.TrackEvent(
                            LuisTelemetryConstants.LuisResult,
                            properties,
                            telemetryMetrics);

            // Create second event.
            var secondEventProperties = new Dictionary<string, string>
            {
                {
                    "MyImportantProperty2",
                    "myImportantValue2"
                },
            };
            TelemetryClient.TrackEvent(
                            "MySecondEvent",
                            secondEventProperties);
        }
    }
}
