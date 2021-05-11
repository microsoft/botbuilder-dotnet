// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LuisV2 = Microsoft.Bot.Builder.AI.Luis;

namespace Microsoft.Bot.Builder.AI.LuisV3
{
    /// <inheritdoc />
    /// <summary>
    /// A LUIS based implementation of <see cref="LuisV2.ITelemetryRecognizer"/> for the V3 endpoint.
    /// </summary>
    [Obsolete("Class is deprecated, please use Microsoft.Bot.Builder.AI.Luis.LuisRecognizer(LuisRecognizerOptions recognizer).")]
    public class LuisRecognizer : LuisV2.ITelemetryRecognizer
    {
#pragma warning disable CA2000 // Dispose objects before losing scope (class is deprecated, we won't be fixing dispose issues here)
        /// <summary>
        /// The value type for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";

        /// <summary>
        /// The context label for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceLabel = "LuisV3 Trace";

        private readonly LuisApplication _application;
        private readonly LuisPredictionOptions _predictionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="application">The LUIS application to use to recognize text.</param>
        /// <param name="recognizerOptions">(Optional) Options for the created recognizer.</param>
        /// <param name="predictionOptions">(Optional) The default LUIS prediction options to use.</param>
        public LuisRecognizer(LuisApplication application, LuisRecognizerOptions recognizerOptions = null, LuisPredictionOptions predictionOptions = null)
        {
            recognizerOptions = recognizerOptions ?? new LuisRecognizerOptions();
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _predictionOptions = predictionOptions ?? new LuisPredictionOptions();

            TelemetryClient = recognizerOptions.TelemetryClient;
            LogPersonalInformation = recognizerOptions.LogPersonalInformation;

            var delegatingHandler = new LuisV2.LuisDelegatingHandler();
            var httpClientHandler = recognizerOptions.HttpClient ?? new HttpClientHandler();
            var currentHandler = CreateHttpHandlerPipeline(httpClientHandler, delegatingHandler);

            DefaultHttpClient = new HttpClient(currentHandler, false)
            {
                Timeout = recognizerOptions.Timeout,
            };

            DefaultHttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _application.EndpointKey);
        }

        /// <summary>
        /// Gets client to use for http.
        /// </summary>
        /// <value>
        /// Client to use for http.
        /// </value>
        public static HttpClient DefaultHttpClient { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log personal information that came from the user to telemetry.
        /// </summary>
        /// <value>If true, personal information is logged to Telemetry; otherwise the properties will be filtered.</value>
        public bool LogPersonalInformation { get; set; }

        /// <summary>
        /// Gets the currently configured <see cref="IBotTelemetryClient"/> that logs the LuisResult event.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> being used to log events.</value>
        [JsonIgnore]
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
            var topScore = -1.0;
            foreach (var intent in results.Intents)
            {
                var score = (double)intent.Value.Score;
                if (score > topScore && score >= minScore)
                {
                    topIntent = intent.Key;
                    topScore = score;
                }
            }

            return !string.IsNullOrEmpty(topIntent) ? topIntent : defaultIntent;
        }

        /* IRecognizer */

        /// <inheritdoc />
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            => await RecognizeInternalAsync(turnContext, null, null, null, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Runs an utterance through a recognizer and returns a generic recognizer result.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="predictionOptions">A <see cref="LuisPredictionOptions"/> instance to be used by the call.
        /// This parameter gets merged with the default <see cref="LuisPredictionOptions"/> passed in the constructor.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, LuisPredictionOptions predictionOptions, CancellationToken cancellationToken)
            => await RecognizeInternalAsync(turnContext, predictionOptions, null, null, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, null, null, null, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /* ITelemetryRecognizer */

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default)
        => await RecognizeInternalAsync(turnContext, null, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, null, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /* LUIS specific methods */

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="predictionOptions">A <see cref="LuisPredictionOptions"/> instance to be used by the call.
        /// This parameter gets merged with the default <see cref="LuisPredictionOptions"/> passed in the constructor.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(
            ITurnContext turnContext,
            LuisPredictionOptions predictionOptions = null,
            Dictionary<string, string> telemetryProperties = null,
            Dictionary<string, double> telemetryMetrics = null,
            CancellationToken cancellationToken = default)
        => await RecognizeInternalAsync(turnContext, predictionOptions, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="predictionOptions">A <see cref="LuisPredictionOptions"/> instance to be used by the call.
        /// This parameter gets merged with the default <see cref="LuisPredictionOptions"/> passed in the constructor.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<T> RecognizeAsync<T>(
            ITurnContext turnContext,
            LuisPredictionOptions predictionOptions = null,
            Dictionary<string, string> telemetryProperties = null,
            Dictionary<string, double> telemetryMetrics = null,
            CancellationToken cancellationToken = default)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, predictionOptions, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Invoked prior to a LuisResult being logged.
        /// </summary>
        /// <param name="recognizerResult">The Luis Results for the call.</param>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        protected virtual void OnRecognizerResult(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var properties = FillLuisEventProperties(recognizerResult, turnContext, telemetryProperties);

            // Track the event
            TelemetryClient.TrackEvent(LuisV2.LuisTelemetryConstants.LuisResult, properties, telemetryMetrics);
        }

        /// <summary>
        /// Fills the event properties for LuisResult event for telemetry.
        /// These properties are logged when the recognizer is called.
        /// </summary>
        /// <param name="recognizerResult">Last activity sent from user.</param>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <returns>A dictionary that is sent as "Properties" to IBotTelemetryClient.TrackEvent method for the BotMessageSend event.</returns>
        protected Dictionary<string, string> FillLuisEventProperties(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null)
        {
            var topTwoIntents = (recognizerResult.Intents.Count > 0) ? recognizerResult.Intents.OrderByDescending(x => x.Value.Score).Take(2).ToArray() : null;

            // Add the intent score and conversation id properties
            var properties = new Dictionary<string, string>()
            {
                { LuisV2.LuisTelemetryConstants.ApplicationIdProperty, _application.ApplicationId },
                { LuisV2.LuisTelemetryConstants.IntentProperty, topTwoIntents?[0].Key ?? string.Empty },
                { LuisV2.LuisTelemetryConstants.IntentScoreProperty, topTwoIntents?[0].Value.Score?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00" },
                { LuisV2.LuisTelemetryConstants.Intent2Property, (topTwoIntents?.Length > 1) ? topTwoIntents?[1].Key ?? string.Empty : string.Empty },
                { LuisV2.LuisTelemetryConstants.IntentScore2Property, (topTwoIntents?.Length > 1) ? topTwoIntents?[1].Value.Score?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00" : "0.00" },
                { LuisV2.LuisTelemetryConstants.FromIdProperty, turnContext.Activity.From.Id },
            };

            if (recognizerResult.Properties.TryGetValue("sentiment", out var sentiment) && sentiment is JObject)
            {
                if (((JObject)sentiment).TryGetValue("label", out var label))
                {
                    properties.Add(LuisV2.LuisTelemetryConstants.SentimentLabelProperty, label.Value<string>());
                }

                if (((JObject)sentiment).TryGetValue("score", out var score))
                {
                    properties.Add(LuisV2.LuisTelemetryConstants.SentimentScoreProperty, score.Value<string>());
                }
            }

            var entities = recognizerResult.Entities?.ToString();
            properties.Add(LuisV2.LuisTelemetryConstants.EntitiesProperty, entities);

            // Use the LogPersonalInformation flag to toggle logging PII data, text is a common example
            if (LogPersonalInformation && !string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                properties.Add(LuisV2.LuisTelemetryConstants.QuestionProperty, turnContext.Activity.Text);
            }

            // Additional Properties can override "stock" properties.
            if (telemetryProperties != null)
            {
                properties = telemetryProperties.Concat(properties)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value);
            }

            return properties;
        }

        private static DelegatingHandler CreateHttpHandlerPipeline(HttpClientHandler httpClientHandler, params DelegatingHandler[] handlers)
        {
            // Now, the RetryAfterDelegatingHandler should be the absolute outermost handler
            // because it's extremely lightweight and non-interfering
            DelegatingHandler currentHandler =
                new RetryDelegatingHandler(new RetryAfterDelegatingHandler { InnerHandler = httpClientHandler });

            if (handlers != null)
            {
                for (var i = handlers.Length - 1; i >= 0; --i)
                {
                    var handler = handlers[i];

                    // Non-delegating handlers are ignored since we always
                    // have RetryDelegatingHandler as the outer-most handler
                    while (handler.InnerHandler is DelegatingHandler)
                    {
                        handler = handler.InnerHandler as DelegatingHandler;
                    }

                    handler.InnerHandler = currentHandler;
                    currentHandler = handlers[i];
                }
            }

            return currentHandler;
        }

        private static string AddParam(string query, string prop, bool? val)
        {
            var result = query;
            if (val.HasValue)
            {
                if (query == null)
                {
                    result = $"{prop}={val.Value}";
                }
                else
                {
                    result += $"&{prop}={val.Value}";
                }
            }

            return result;
        }

        private async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, LuisPredictionOptions predictionOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return null;
            }

            var options = predictionOptions ?? _predictionOptions;
            var utterance = turnContext.Activity?.AsMessageActivity()?.Text;
            RecognizerResult recognizerResult;
            JObject luisResponse = null;

            if (string.IsNullOrWhiteSpace(utterance))
            {
                recognizerResult = new RecognizerResult
                {
                    Text = utterance,
                    Intents = new Dictionary<string, IntentScore>() { { string.Empty, new IntentScore() { Score = 1.0 } } },
                    Entities = new JObject(),
                };
            }
            else
            {
                var uri = new UriBuilder(_application.Endpoint);
                uri.Path += $"luis/prediction/v3.0/apps/{_application.ApplicationId}";

                var query = AddParam(null, "verbose", options.IncludeInstanceData);
                query = AddParam(query, "log", options.Log);
                query = AddParam(query, "show-all-intents", options.IncludeAllIntents);
                uri.Query = query;

                var content = new JObject
                {
                    { "query", utterance },
                };
                var queryOptions = new JObject
                {
                    { "overridePredictions", options.PreferExternalEntities },
                };
                content.Add("options", queryOptions);

                var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                if (options.DynamicLists != null)
                {
                    foreach (var list in options.DynamicLists)
                    {
                        list.Validate();
                    }

                    content.Add("dynamicLists", (JArray)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(options.DynamicLists, settings)));
                }

                if (options.ExternalEntities != null)
                {
                    foreach (var entity in options.ExternalEntities)
                    {
                        entity.Validate();
                    }

                    content.Add("externalEntities", (JArray)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(options.ExternalEntities, settings)));
                }

                if (options.Version == null)
                {
                    uri.Path += $"/slots/{options.Slot}/predict";
                }
                else
                {
                    uri.Path += $"/versions/{options.Version}/predict";
                }

                var response = await DefaultHttpClient.PostAsync(uri.Uri, new StringContent(content.ToString(), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                luisResponse = (JObject)JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                var prediction = (JObject)luisResponse["prediction"];
                recognizerResult = new RecognizerResult
                {
                    Text = utterance,
                    AlteredText = prediction["alteredQuery"]?.Value<string>(),
                    Intents = LuisUtil.GetIntents(prediction),
                    Entities = LuisUtil.ExtractEntitiesAndMetadata(prediction),
                };
                LuisUtil.AddProperties(prediction, recognizerResult);
                if (options.IncludeAPIResults)
                {
                    recognizerResult.Properties.Add("luisResult", luisResponse);
                }
            }

            // Log telemetry
            OnRecognizerResult(recognizerResult, turnContext, telemetryProperties, telemetryMetrics);

            var traceInfo = JObject.FromObject(
                new
                {
                    recognizerResult,
                    luisModel = new
                    {
                        ModelID = _application.ApplicationId,
                    },
                    luisOptions = options,
                    luisResult = luisResponse,
                });

            await turnContext.TraceActivityAsync("LuisRecognizer", traceInfo, LuisTraceType, LuisTraceLabel, cancellationToken).ConfigureAwait(false);
            return recognizerResult;
        }
#pragma warning restore CA2000 // Dispose objects before losing scope
    }
}
