// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Class that represents an adaptive LUIS recognizer.
    /// </summary>
    public class LuisAdaptiveRecognizer : Recognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.LuisRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisAdaptiveRecognizer"/> class.
        /// </summary>
        public LuisAdaptiveRecognizer()
        {
        }

        /// <summary>
        /// Gets or sets LUIS application ID.
        /// </summary>
        /// <value>Application ID.</value>
        [JsonProperty("applicationId")]
        public StringExpression ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets LUIS endpoint like https://westus.api.cognitive.microsoft.com to query.
        /// </summary>
        /// <value>LUIS Endpoint.</value>
        [JsonProperty("endpoint")]
        public StringExpression Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the key used to talk to a LUIS endpoint.
        /// </summary>
        /// <value>Endpoint key.</value>
        [JsonProperty("endpointKey")]
        public StringExpression EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets an external entity recognizer.
        /// </summary>
        /// <remarks>This recognizer is run before calling LUIS and the results are passed to LUIS.</remarks>
        /// <value>Recognizer.</value>
        [JsonProperty("externalEntityRecognizer")]
        public Recognizer ExternalEntityRecognizer { get; set; }

        /// <summary>
        /// Gets or sets an expression or constant LUIS dynamic list.
        /// </summary>
        /// <value>Dynamic lists.</value>
        [JsonProperty("dynamicLists")]
        public ArrayExpression<Luis.DynamicList> DynamicLists { get; set; }

        /// <summary>
        /// Gets or sets LUIS prediction options.
        /// </summary>
        /// <value>Prediction options.</value>
        [JsonProperty("predictionOptions")]
        public AI.LuisV3.LuisPredictionOptions PredictionOptions { get; set; } = new AI.LuisV3.LuisPredictionOptions();

        /// <summary>
        /// Gets or sets HTTP client handler.
        /// </summary>
        /// <value>HTTP client handler.</value>
        [JsonIgnore]
        public HttpClientHandler HttpClient { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IBotTelemetryClient"/>.
        /// </summary>
        /// <value>Telemetry client.</value>
        [JsonIgnore]
        public IBotTelemetryClient TelemetryClient { get; set; } = new NullBotTelemetryClient();

        /// <inheritdoc/>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default)
        {
            var wrapper = new LuisRecognizer(RecognizerOptions(dialogContext), HttpClient);

            // temp clone of turn context because luisrecognizer always pulls activity from turn context.
            var tempContext = new TurnContext(dialogContext.Context.Adapter, activity);
            foreach (var keyValue in dialogContext.Context.TurnState)
            {
                tempContext.TurnState[keyValue.Key] = keyValue.Value;
            }

            return await wrapper.RecognizeAsync(tempContext, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Construct V3 recognizer options from the current dialog context.
        /// </summary>
        /// <param name="dialogContext">Context.</param>
        /// <returns>LUIS Recognizer options.</returns>
        public LuisRecognizerOptionsV3 RecognizerOptions(DialogContext dialogContext)
        {
            var options = PredictionOptions;
            var dcState = dialogContext.GetState();
            if (DynamicLists != null)
            {
                options = new AI.LuisV3.LuisPredictionOptions(options);
                var list = new List<AI.LuisV3.DynamicList>();
                foreach (var listEntity in DynamicLists.GetValue(dcState))
                {
                    list.Add(new AI.LuisV3.DynamicList(listEntity.Entity, listEntity.List));
                }

                options.DynamicLists = list;
            }

            var application = new LuisApplication(ApplicationId.GetValue(dcState), EndpointKey.GetValue(dcState), Endpoint.GetValue(dcState));
            return new LuisRecognizerOptionsV3(application)
            {
                ExternalEntityRecognizer = ExternalEntityRecognizer,
                PredictionOptions = options,
                TelemetryClient = TelemetryClient
            };
        }
    }
}
