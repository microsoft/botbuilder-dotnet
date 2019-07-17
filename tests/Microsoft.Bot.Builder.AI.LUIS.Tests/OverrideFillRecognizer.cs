// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    public class OverrideFillRecognizer : LuisRecognizer
    {
        public OverrideFillRecognizer(IBotTelemetryClient telemetryClient, LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, bool logPersonalInformation = false, HttpClientHandler clientHandler = null)
           : base(application, predictionOptions, includeApiResults, clientHandler)
        {
            LogPersonalInformation = logPersonalInformation;
        }

        protected override async Task OnRecognizerResultAsync(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var properties = await FillLuisEventPropertiesAsync(recognizerResult, turnContext, telemetryProperties, cancellationToken).ConfigureAwait(false);

            properties.TryAdd("MyImportantProperty", "myImportantValue");

            // Log event
            TelemetryClient.TrackEvent(
                            LuisTelemetryConstants.LuisResult,
                            properties,
                            telemetryMetrics);

            // Create second event.
            var secondEventProperties = new Dictionary<string, string>();
            secondEventProperties.Add(
                "MyImportantProperty2",
                "myImportantValue2");
            TelemetryClient.TrackEvent(
                            "MySecondEvent",
                            secondEventProperties);
        }
    }
}
