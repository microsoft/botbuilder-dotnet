// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        /// Gets or sets a value indicating whether to log personal information that came from the user to telemetry.
        /// </summary>
        /// <value>If true, personal information is logged to Telemetry; otherwise the properties will be filtered.</value>
        public bool LogPersonalInformation { get; set; } = false;

        /// <inheritdoc/>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var wrapper = new LuisRecognizer(RecognizerOptions(dialogContext), HttpClient);

            // temp clone of turn context because luisrecognizer always pulls activity from turn context.
            var tempContext = new TurnContext(dialogContext.Context.Adapter, activity);
            foreach (var keyValue in dialogContext.Context.TurnState)
            {
                tempContext.TurnState[keyValue.Key] = keyValue.Value;
            }

            var result = await wrapper.RecognizeAsync(tempContext, cancellationToken).ConfigureAwait(false);

            this.TelemetryClient.TrackEvent("LuisResult", this.FillRecognizerResultTelemetryProperties(result, telemetryProperties, dialogContext.Context), telemetryMetrics);

            return result;
        }

        /// <summary>
        /// Construct V3 recognizer options from the current dialog context.
        /// </summary>
        /// <param name="dialogContext">Context.</param>
        /// <returns>LUIS Recognizer options.</returns>
        public LuisRecognizerOptionsV3 RecognizerOptions(DialogContext dialogContext)
        {
            var options = PredictionOptions;
            if (DynamicLists != null)
            {
                options = new AI.LuisV3.LuisPredictionOptions(options);
                var list = new List<AI.LuisV3.DynamicList>();
                foreach (var listEntity in DynamicLists.GetValue(dialogContext.State))
                {
                    list.Add(new AI.LuisV3.DynamicList(listEntity.Entity, listEntity.List));
                }

                options.DynamicLists = list;
            }

            var application = new LuisApplication(ApplicationId.GetValue(dialogContext.State), EndpointKey.GetValue(dialogContext.State), Endpoint.GetValue(dialogContext.State));
            return new LuisRecognizerOptionsV3(application)
            {
                ExternalEntityRecognizer = ExternalEntityRecognizer,
                PredictionOptions = options,
                TelemetryClient = TelemetryClient
            };
        }

        protected override Dictionary<string, string> FillRecognizerResultTelemetryProperties(RecognizerResult recognizerResult, Dictionary<string, string> telemetryProperties, ITurnContext turnContext)
        {
            var topTwoIntents = (recognizerResult.Intents.Count > 0) ? recognizerResult.Intents.OrderByDescending(x => x.Value.Score).Take(2).ToArray() : null;

            // Add the intent score and conversation id properties
            var properties = new Dictionary<string, string>()
            {
                { LuisTelemetryConstants.ApplicationIdProperty, ApplicationId.ExpressionText },
                { LuisTelemetryConstants.IntentProperty, topTwoIntents?[0].Key ?? string.Empty },
                { LuisTelemetryConstants.IntentScoreProperty, topTwoIntents?[0].Value.Score?.ToString("N2") ?? "0.00" },
                { LuisTelemetryConstants.Intent2Property, (topTwoIntents?.Count() > 1) ? topTwoIntents?[1].Key ?? string.Empty : string.Empty },
                { LuisTelemetryConstants.IntentScore2Property, (topTwoIntents?.Count() > 1) ? topTwoIntents?[1].Value.Score?.ToString("N2") ?? "0.00" : "0.00" },
                { LuisTelemetryConstants.FromIdProperty, turnContext.Activity.From.Id },
            };

            if (recognizerResult.Properties.TryGetValue("sentiment", out var sentiment) && sentiment is JObject)
            {
                if (((JObject)sentiment).TryGetValue("label", out var label))
                {
                    properties.Add(LuisTelemetryConstants.SentimentLabelProperty, label.Value<string>());
                }

                if (((JObject)sentiment).TryGetValue("score", out var score))
                {
                    properties.Add(LuisTelemetryConstants.SentimentScoreProperty, score.Value<string>());
                }
            }

            var entities = recognizerResult.Entities?.ToString();
            properties.Add(LuisTelemetryConstants.EntitiesProperty, entities);

            // Use the LogPersonalInformation flag to toggle logging PII data, text is a common example
            if (LogPersonalInformation && !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                properties.Add(LuisTelemetryConstants.QuestionProperty, turnContext.Activity.Text);
            }

            // Additional Properties can override "stock" properties.
            if (telemetryProperties != null)
            {
                return telemetryProperties.Concat(properties)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value);
            }

            return properties;
        }
    }
}
