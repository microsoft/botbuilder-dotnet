// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;

namespace Microsoft.Bot.Builder.AI.LuisV3.Tests
{
    public class TelemetryOverrideRecognizer : LuisRecognizer
    {
        public TelemetryOverrideRecognizer(LuisApplication application, LuisRecognizerOptions recognizerOptions = null)
           : base(application, recognizerOptions)
        {
        }

        protected override void OnRecognizerResult(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            properties.TryAdd("MyImportantProperty", "myImportantValue");

            // Log event
            TelemetryClient.TrackEvent(
                            LuisTelemetryConstants.LuisResult,
                            properties,
                            metrics);

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
