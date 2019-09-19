// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <inheritdoc />
    /// <summary>
    /// A LUIS based implementation of <see cref="ITelemetryRecognizer"/>.
    /// </summary>
    public class LuisRecognizer : ITelemetryRecognizer
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
        /// <param name="logPersonalInformation">TRUE to include personally indentifiable information.</param>
        public LuisRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null, IBotTelemetryClient telemetryClient = null, bool logPersonalInformation = false)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _options = predictionOptions ?? new LuisPredictionOptions();
            _includeApiResults = includeApiResults;

            TelemetryClient = _options.TelemetryClient;
            LogPersonalInformation = _options.LogPersonalInformation;

            var credentials = new ApiKeyServiceClientCredentials(application.EndpointKey);
            var delegatingHandler = new LuisDelegatingHandler();
            var httpClientHandler = clientHandler ?? CreateRootHandler();
            var currentHandler = CreateHttpHandlerPipeline(httpClientHandler, delegatingHandler);

            DefaultHttpClient = new HttpClient(currentHandler, false)
            {
                Timeout = TimeSpan.FromMilliseconds(_options.Timeout),
            };

            _runtime = new LUISRuntimeClient(credentials, DefaultHttpClient, false)
            {
                Endpoint = application.Endpoint,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="service">The LUIS service from configuration.</param>
        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        public LuisRecognizer(LuisService service, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null)
            : this(new LuisApplication(service), predictionOptions, includeApiResults, clientHandler)
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
            : this(new LuisApplication(applicationEndpoint), predictionOptions, includeApiResults, clientHandler)
        {
        }

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

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly-typed recognizer result.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="predictionOptions">A <see cref="LuisPredictionOptions"/> instance to be used by the call.
        /// This parameter gets merged with the default <see cref="LuisPredictionOptions"/> passed in the constructor.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, LuisPredictionOptions predictionOptions, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, predictionOptions, null, null, cancellationToken).ConfigureAwait(false));
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
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        => await RecognizeInternalAsync(turnContext, null, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);

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
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, LuisPredictionOptions predictionOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        => await RecognizeInternalAsync(turnContext, predictionOptions, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, null, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false));
            return result;
        }

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
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, LuisPredictionOptions predictionOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
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
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns><see cref="Task"/>.</returns>
        protected virtual async Task OnRecognizerResultAsync(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var properties = await FillLuisEventPropertiesAsync(recognizerResult, turnContext, telemetryProperties, cancellationToken).ConfigureAwait(false);

            // Track the event
            TelemetryClient.TrackEvent(LuisTelemetryConstants.LuisResult, properties, telemetryMetrics);
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

        private async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, LuisPredictionOptions predictionOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics, CancellationToken cancellationToken)
        {
            var luisPredictionOptions = predictionOptions == null ? _options : MergeDefaultOptionsWithProvidedOptions(_options, predictionOptions);

            BotAssert.ContextNotNull(turnContext);

            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return null;
            }

            var utterance = turnContext.Activity?.AsMessageActivity()?.Text;
            RecognizerResult recognizerResult;
            LuisResult luisResult = null;

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
                luisResult = await _runtime.Prediction.ResolveAsync(
                    _application.ApplicationId,
                    utterance,
                    timezoneOffset: luisPredictionOptions.TimezoneOffset,
                    verbose: luisPredictionOptions.IncludeAllIntents,
                    staging: luisPredictionOptions.Staging,
                    spellCheck: luisPredictionOptions.SpellCheck,
                    bingSpellCheckSubscriptionKey: luisPredictionOptions.BingSpellCheckSubscriptionKey,
                    log: luisPredictionOptions.Log ?? true,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                recognizerResult = new RecognizerResult
                {
                    Text = utterance,
                    AlteredText = luisResult.AlteredQuery,
                    Intents = LuisUtil.GetIntents(luisResult),
                    Entities = LuisUtil.ExtractEntitiesAndMetadata(luisResult.Entities, luisResult.CompositeEntities, luisPredictionOptions.IncludeInstanceData ?? true, utterance),
                };
                LuisUtil.AddProperties(luisResult, recognizerResult);
                if (_includeApiResults)
                {
                    recognizerResult.Properties.Add("luisResult", luisResult);
                }
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
                    luisOptions = luisPredictionOptions,
                    luisResult,
                });

            await turnContext.TraceActivityAsync("LuisRecognizer", traceInfo, LuisTraceType, LuisTraceLabel, cancellationToken).ConfigureAwait(false);
            return recognizerResult;
        }

        private LuisPredictionOptions MergeDefaultOptionsWithProvidedOptions(LuisPredictionOptions defaultOptions, LuisPredictionOptions overridenOptions)
            => new LuisPredictionOptions()
            {
                BingSpellCheckSubscriptionKey = overridenOptions.BingSpellCheckSubscriptionKey ?? defaultOptions.BingSpellCheckSubscriptionKey,
                IncludeAllIntents = overridenOptions.IncludeAllIntents ?? defaultOptions.IncludeAllIntents,
                IncludeInstanceData = overridenOptions.IncludeInstanceData ?? defaultOptions.IncludeInstanceData,
                Log = overridenOptions.Log ?? defaultOptions.Log,
                SpellCheck = overridenOptions.SpellCheck ?? defaultOptions.SpellCheck,
                Staging = overridenOptions.Staging ?? defaultOptions.Staging,
                TimezoneOffset = overridenOptions.TimezoneOffset ?? defaultOptions.TimezoneOffset,
            };

        private DelegatingHandler CreateHttpHandlerPipeline(HttpClientHandler httpClientHandler, params DelegatingHandler[] handlers)
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

        private HttpClientHandler CreateRootHandler() => new HttpClientHandler();
    }
}
