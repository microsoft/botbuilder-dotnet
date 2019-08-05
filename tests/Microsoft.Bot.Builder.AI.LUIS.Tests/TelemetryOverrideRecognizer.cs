// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    public class TelemetryOverrideRecognizer : LuisRecognizer
    {
        public TelemetryOverrideRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, bool logPersonalInformation = false, HttpClientHandler clientHandler = null)
           : base(application, predictionOptions, includeApiResults, clientHandler)
        {
            LogPersonalInformation = logPersonalInformation;
        }

        protected override Task OnRecognizerResultAsync(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null, CancellationToken cancellationToken = default)
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
                { "MyImportantProperty2", "myImportantValue2" },
            };
            TelemetryClient.TrackEvent(
                            "MySecondEvent",
                            secondEventProperties);
            return Task.CompletedTask;
        }
    }
}
