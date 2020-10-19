// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Class that represents an adaptive LUIS recognizer.
    /// </summary>
    public class LuisAdaptiveRecognizer : Recognizer
    {
        /// <summary>
        /// The Kind value for this recognizer.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.LuisRecognizer";

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
        /// Gets or sets LUIS version.
        /// </summary>
        /// <value>application version.</value>
        [JsonProperty("version")]
        public StringExpression Version { get; set; }

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
        /// <value>Prediction options for backward compat code.</value>
        [JsonIgnore]
        [Obsolete("You should use Options instead as it supports expression properties.")]
        public AI.LuisV3.LuisPredictionOptions PredictionOptions { get; set; }

        /// <summary>
        /// Gets or sets LUIS Prediction options (with expressions).
        /// </summary>
        /// <value>Predictions options (Declarative with expression support).</value>
        [JsonProperty("predictionOptions")]
        public LuisAdaptivePredictionOptions Options { get; set; }

        /// <summary>
        /// Gets or sets HTTP client handler.
        /// </summary>
        /// <value>HTTP client handler.</value>
        [JsonIgnore]
        public HttpClientHandler HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the flag to determine if personal information should be logged in telemetry.
        /// </summary>
        /// <value>
        /// The flag to indicate in personal information should be logged in telemetry.
        /// </value>
        [JsonProperty("logPersonalInformation")]
        public BoolExpression LogPersonalInformation { get; set; } = "=settings.telemetry.logPersonalInformation";

        /// <inheritdoc/>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var recognizer = new LuisRecognizer(RecognizerOptions(dialogContext), HttpClient);

            RecognizerResult result = await recognizer.RecognizeAsync(dialogContext, activity, cancellationToken).ConfigureAwait(false);

            TrackRecognizerResult(dialogContext, "LuisResult", FillRecognizerResultTelemetryProperties(result, telemetryProperties, dialogContext), telemetryMetrics);

            return result;
        }

        /// <summary>
        /// Construct V3 recognizer options from the current dialog context.
        /// </summary>
        /// <param name="dialogContext">Context.</param>
        /// <returns>LUIS Recognizer options.</returns>
        public LuisRecognizerOptionsV3 RecognizerOptions(DialogContext dialogContext)
        {
            AI.LuisV3.LuisPredictionOptions options = new LuisV3.LuisPredictionOptions();
            if (this.PredictionOptions != null)
            {
                options = new LuisV3.LuisPredictionOptions(this.PredictionOptions);
            }
            else if (this.Options != null)
            {
                options.DateTimeReference = this.Options.DateTimeReference?.GetValue(dialogContext);
                options.ExternalEntities = this.Options.ExternalEntities?.GetValue(dialogContext);
                options.IncludeAllIntents = this.Options.IncludeAllIntents?.GetValue(dialogContext) ?? false;
                options.IncludeInstanceData = this.Options.IncludeInstanceData?.GetValue(dialogContext) ?? true;
                options.IncludeAPIResults = this.Options.IncludeAPIResults?.GetValue(dialogContext) ?? false;
                options.Log = this.Options.Log?.GetValue(dialogContext) ?? true;
                options.PreferExternalEntities = this.Options.PreferExternalEntities?.GetValue(dialogContext) ?? true;
                options.Slot = this.Options.Slot?.GetValue(dialogContext);
            }

            if (this.Version != null)
            {
                options.Version = this.Version?.GetValue(dialogContext);
            }

            if (DynamicLists != null)
            {
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
                TelemetryClient = TelemetryClient,
                IncludeAPIResults = options.IncludeAPIResults,
            };
        }

        /// <summary>
        /// Uses the <see cref="RecognizerResult"/> returned from the <see cref="LuisRecognizer"/> and populates a dictionary of string
        /// with properties to be logged into telemetry.  Including any additional properties that were passed into the method.
        /// </summary>
        /// <param name="recognizerResult">An instance of <see cref="RecognizerResult"/> to extract the telemetry properties from.</param>
        /// <param name="telemetryProperties">A collection of additional properties to be added to the returned dictionary of properties.</param>
        /// <param name="dc">An instance of <see cref="DialogContext"/>.</param>
        /// <returns>The dictionary of properties to be logged with telemetry for the recongizer result.</returns>
        protected override Dictionary<string, string> FillRecognizerResultTelemetryProperties(RecognizerResult recognizerResult, Dictionary<string, string> telemetryProperties, DialogContext dc)
        {
            var (logPersonalInfo, error) = this.LogPersonalInformation.TryGetValue(dc.State);
            var (applicationId, error2) = this.ApplicationId.TryGetValue(dc.State);

            var topTwoIntents = (recognizerResult.Intents.Count > 0) ? recognizerResult.Intents.OrderByDescending(x => x.Value.Score).Take(2).ToArray() : null;

            // Add the intent score and conversation id properties
            var properties = new Dictionary<string, string>()
            {
                { LuisTelemetryConstants.ApplicationIdProperty, applicationId },
                { LuisTelemetryConstants.IntentProperty, topTwoIntents?[0].Key ?? string.Empty },
                { LuisTelemetryConstants.IntentScoreProperty, topTwoIntents?[0].Value.Score?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00" },
                { LuisTelemetryConstants.Intent2Property, (topTwoIntents?.Length > 1) ? topTwoIntents?[1].Key ?? string.Empty : string.Empty },
                { LuisTelemetryConstants.IntentScore2Property, (topTwoIntents?.Length > 1) ? topTwoIntents?[1].Value.Score?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00" : "0.00" },
                { LuisTelemetryConstants.FromIdProperty, dc.Context.Activity.From.Id },
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
            if (logPersonalInfo && !string.IsNullOrEmpty(dc.Context.Activity.Text))
            {
                properties.Add(LuisTelemetryConstants.QuestionProperty, dc.Context.Activity.Text);
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
