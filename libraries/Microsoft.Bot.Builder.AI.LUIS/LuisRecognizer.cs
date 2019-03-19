// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <inheritdoc />
    /// <summary>
    /// A LUIS based implementation of <see cref="IRecognizer"/>.
    /// </summary>
    public class LuisRecognizer : IRecognizer, ITelemetryRecognizer
    {
        /// <summary>
        /// The value type for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";

        /// <summary>
        /// The context label for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceLabel = "Luis Trace";

        private readonly ILUISRuntimeClient _runtime;
        private readonly LuisApplication _application;
        private readonly LuisPredictionOptions _options;
        private readonly bool _includeApiResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="application">The LUIS application to use to recognize text.</param>
        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        /// <param name="telemetryClient">The IBotTelemetryClient used to log the LuisResult event.</param>
        public LuisRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null, IBotTelemetryClient telemetryClient = null, bool logPersonalInformation = false)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _options = predictionOptions ?? new LuisPredictionOptions();
            _includeApiResults = includeApiResults;

            TelemetryClient = telemetryClient ?? new NullBotTelemetryClient();
            LogPersonalInformation = logPersonalInformation;

            var credentials = new ApiKeyServiceClientCredentials(application.EndpointKey);
            var delegatingHandler = new LuisDelegatingHandler();

            // LUISRuntimeClient requires that we explicitly bind to the appropriate constructor.
            _runtime = clientHandler == null
                    ?
                new LUISRuntimeClient(credentials, delegatingHandler)
                    :
                new LUISRuntimeClient(credentials, clientHandler, delegatingHandler);

            _runtime.Endpoint = application.Endpoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="service">The LUIS service from configuration.</param>
        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        public LuisRecognizer(LuisService service, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null)
            : this(new LuisApplication(service), predictionOptions, includeApiResults, clientHandler, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="applicationEndpoint">The LUIS endpoint as shown in https://luis.ai .</param>
        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        public LuisRecognizer(string applicationEndpoint, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null)
            : this(new LuisApplication(applicationEndpoint), predictionOptions, includeApiResults, clientHandler, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="telemetryClient">The IBotTelemetryClient used to log the LuisResult event.</param>
        /// <param name="application">The LUIS application to use to recognize text.</param>
        /// <param name="predictionOptions">The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">TRUE to include raw LUIS API response.</param>
        /// <param name="logPersonalInformation">TRUE to include personally indentifiable information.</param>
        public LuisRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, IBotTelemetryClient telemetryClient = null, bool logPersonalInformation = false)
            : this(application, predictionOptions, includeApiResults, null, telemetryClient)
        {
            LogPersonalInformation = logPersonalInformation;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log personal information that came from the user to telemetry.
        /// </summary>
        /// <value>If true, personal information is logged to Telemetry; otherwise the properties will be filtered.</value>
        public bool LogPersonalInformation { get; set; }

        /// <summary>
        /// Gets the currently configured <see cref="IBotTelemetryClient"/> that logs the LuisResult event.
        /// </summary>
        /// <value>The <see cref=IBotTelemetryClient"/> being used to log events.</value>
        public IBotTelemetryClient TelemetryClient { get; }

        /// <summary>
        /// Returns the name of the top scoring intent from a set of LUIS results.
        /// </summary>
        /// <param name="results">Result set to be searched.</param>
        /// <param name="defaultIntent">(Optional) Intent name to return should a top intent be found. Defaults to a value of "None".</param>
        /// <param name="minScore">(Optional) Minimum score needed for an intent to be considered as a top intent. If all intents in the set are below this threshold then the `defaultIntent` will be returned.  Defaults to a value of `0.0`.</param>
        /// <returns>The top scoring intent name.</returns>
        public static string TopIntent(RecognizerResult results, string defaultIntent = "None", double minScore = 0.0)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            string topIntent = null;
            double topScore = -1.0;
            if (results.Intents.Count > 0)
            {
                foreach (var intent in results.Intents)
                {
                    var score = (double)intent.Value.Score;
                    if (score > topScore && score >= minScore)
                    {
                        topIntent = intent.Key;
                        topScore = score;
                    }
                }
            }

            return !string.IsNullOrEmpty(topIntent) ? topIntent : defaultIntent;
        }

        /// <inheritdoc />
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            => await RecognizeInternalAsync(turnContext, null, null, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, null, null, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await RecognizeInternalAsync(turnContext, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, telemetryProperties, telemetryMetrics,  cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Invoked prior to a LuisResult being logged.
        /// </summary>
        /// <param name="recognizerResult">The Luis Results for the call.</param>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns><see cref="Task"/>.</returns>
        protected virtual async Task OnRecognizerResultAsync(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var properties = await FillLuisEventPropertiesAsync(recognizerResult, turnContext, telemetryProperties, cancellationToken).ConfigureAwait(false);

            // Track the event
            TelemetryClient.TrackEvent(LuisTelemetryConstants.LuisResult, properties, telemetryMetrics);

            return;
        }

        /// <summary>
        /// Fills the event properties for LuisResult event for telemetry.
        /// These properties are logged when the recognizer is called.
        /// </summary>
        /// <param name="recognizerResult">Last activity sent from user.</param>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// additionalProperties
        /// <returns>A dictionary that is sent as "Properties" to IBotTelemetryClient.TrackEvent method for the BotMessageSend event.</returns>
        protected Task<Dictionary<string, string>> FillLuisEventPropertiesAsync(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var topLuisIntent = recognizerResult.GetTopScoringIntent();
            var intentScore = topLuisIntent.score.ToString("N2");
            var topTwoIntents = (recognizerResult.Intents.Count > 0) ? recognizerResult.Intents.OrderByDescending(x => x.Value.Score).Take(2).ToArray() : null;

            // Add the intent score and conversation id properties
            var properties = new Dictionary<string, string>()
            {
                { LuisTelemetryConstants.ApplicationIdProperty, _application.ApplicationId },
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
                return Task.FromResult(telemetryProperties.Concat(properties)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value));
            }

            return Task.FromResult(properties);
        }

        private async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);

            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return null;
            }

            var utterance = turnContext.Activity?.AsMessageActivity()?.Text;

            if (string.IsNullOrWhiteSpace(utterance))
            {
                throw new ArgumentNullException(nameof(utterance));
            }

            var luisResult = await _runtime.Prediction.ResolveAsync(
                _application.ApplicationId,
                utterance,
                timezoneOffset: _options.TimezoneOffset,
                verbose: _options.IncludeAllIntents,
                staging: _options.Staging,
                spellCheck: _options.SpellCheck,
                bingSpellCheckSubscriptionKey: _options.BingSpellCheckSubscriptionKey,
                log: _options.Log ?? true,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var recognizerResult = new RecognizerResult
            {
                Text = utterance,
                AlteredText = luisResult.AlteredQuery,
                Intents = LuisUtil.GetIntents(luisResult),
                Entities = LuisUtil.ExtractEntitiesAndMetadata(luisResult.Entities, luisResult.CompositeEntities, _options.IncludeInstanceData ?? true),
            };
            LuisUtil.AddProperties(luisResult, recognizerResult);
            if (_includeApiResults)
            {
                recognizerResult.Properties.Add("luisResult", luisResult);
            }

            // Log telemetry
            await OnRecognizerResultAsync(recognizerResult, turnContext, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);

            var traceInfo = JObject.FromObject(
                new
                {
                    recognizerResult,
                    luisModel = new
                    {
                        ModelID = _application.ApplicationId,
                    },
                    luisOptions = _options,
                    luisResult,
                });

            await turnContext.TraceActivityAsync("LuisRecognizer", traceInfo, LuisTraceType, LuisTraceLabel, cancellationToken).ConfigureAwait(false);
            return recognizerResult;
        }
    }
}
