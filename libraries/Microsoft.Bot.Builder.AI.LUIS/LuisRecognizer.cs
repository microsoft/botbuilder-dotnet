// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
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
        /// The declarative type for this recognizer.
        /// </summary>
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.LuisRecognizer";

        /// <summary>
        /// The value type for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";

        /// <summary>
        /// The context label for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceLabel = "Luis Trace";
        
        private readonly string _cacheKey;

        private readonly LuisRecognizerOptions _luisRecognizerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="recognizerOptions"> The LUIS recognizer version options.</param>
        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        public LuisRecognizer(LuisRecognizerOptions recognizerOptions, HttpClientHandler clientHandler = null)
        {
            _luisRecognizerOptions = recognizerOptions;

            TelemetryClient = recognizerOptions.TelemetryClient;
            LogPersonalInformation = recognizerOptions.LogPersonalInformation;

            var delegatingHandler = new LuisDelegatingHandler();
            var httpClientHandler = clientHandler ?? CreateRootHandler();
#pragma warning disable CA2000 // Dispose objects before losing scope (suppressing this warning, for now! we will address this once we implement HttpClientFactory in a future release)
            var currentHandler = CreateHttpHandlerPipeline(httpClientHandler, delegatingHandler);
#pragma warning restore CA2000 // Dispose objects before losing scope

            HttpClient = new HttpClient(currentHandler, false)
            {
                Timeout = TimeSpan.FromMilliseconds(recognizerOptions.Timeout),
            };

#pragma warning disable 618 // Reference to obsolete property, this is here only for backward compat and should be removed when DefaultHttpClient is removed.
            DefaultHttpClient = HttpClient;
#pragma warning restore 618

            _cacheKey = _luisRecognizerOptions.Application.Endpoint + _luisRecognizerOptions.Application.ApplicationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="application">The LUIS application to use to recognize text.</param>
        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        [Obsolete("Constructor is deprecated, please use LuisRecognizer(LuisRecognizerOptions recognizer).")]
        public LuisRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null)
#pragma warning disable CS0618 // Type or member is obsolete
        : this(application, telemetryClient: null, logPersonalInformation: false, predictionOptions: predictionOptions, includeApiResults: includeApiResults, clientHandler: clientHandler)
#pragma warning restore CS0618 // Type or member is obsolete
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="application">The LUIS application to use to recognize text.</param>
        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        /// <param name="telemetryClient">The IBotTelemetryClient used to log the LuisResult event.</param>
        /// <param name="logPersonalInformation">TRUE to include personally indentifiable information.</param>
        [Obsolete("Constructor is deprecated, please use LuisRecognizer(LuisRecognizerOptions recognizer).")]
        public LuisRecognizer(LuisApplication application, IBotTelemetryClient telemetryClient, bool logPersonalInformation, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null)
        : this(BuildLuisRecognizerOptionsV2(application, predictionOptions, includeApiResults), clientHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizer"/> class.
        /// </summary>
        /// <param name="service">The LUIS service from configuration.</param>
        /// <param name="predictionOptions">(Optional) The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">(Optional) TRUE to include raw LUIS API response.</param>
        /// <param name="clientHandler">(Optional) Custom handler for LUIS API calls to allow mocking.</param>
        [Obsolete("Constructor is deprecated, please use LuisRecognizer(LuisRecognizerOptions recognizer).")]
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
        [Obsolete("Constructor is deprecated, please use LuisRecognizer(LuisRecognizerOptions recognizer).")]
        public LuisRecognizer(string applicationEndpoint, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, HttpClientHandler clientHandler = null)
            : this(new LuisApplication(applicationEndpoint), predictionOptions, includeApiResults, clientHandler)
        {
        }

        /// <summary>
        /// Gets the default <see cref="System.Net.Http.HttpClient"/> to use when calling the LUIS API.
        /// </summary>
        /// <value>
        /// The default HTTP client to use.
        /// </value>
        [Obsolete("This property is deprecated and will be removed in future versions of the SDK.")]
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
        /// Gets the HttpClient used when calling the LUIS API.
        /// </summary>
        /// <value>
        /// A <see cref="HttpClient"/>.
        /// </value>
        /// <remarks>
        /// This property is internal only and intended to support unit tests. 
        /// </remarks>
        [JsonIgnore]
        internal HttpClient HttpClient { get; }

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
        /// <param name="dialogContext">dialogcontext.</param>
        /// <param name="activity">activity.</param>
        /// <param name="cancellationToken">cancellationtoken.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken)
            => await RecognizeInternalAsync(dialogContext, activity, null, null, null, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Runs an utterance through a recognizer and returns a generic recognizer result.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="predictionOptions">A <see cref="LuisPredictionOptions"/> instance to be used by the call.
        /// This parameter gets merged with the default <see cref="LuisPredictionOptions"/> passed in the constructor. This will call Luis V2 endpoint if passed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        [Obsolete("Method is deprecated, please use RecognizeAsync(ITurnContext turnContext, LuisRecognizerOptions recognizerOptions, CancellationToken cancellationToken).")]
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, LuisPredictionOptions predictionOptions, CancellationToken cancellationToken)
        {
            var overrideV2 = MergeDefaultOptionsWithProvidedOptionsV2(predictionOptions);
            return await RecognizeInternalAsync(turnContext, overrideV2, null, null, cancellationToken).ConfigureAwait(false);
        }

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
        /// <typeparam name="T">type of result.</typeparam>
        /// <param name="dialogContext">dialogContext.</param>
        /// <param name="activity">activity.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<T> RecognizeAsync<T>(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(dialogContext, activity, null, null, null, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly-typed recognizer result.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="predictionOptions">A <see cref="LuisPredictionOptions"/> instance to be used by the call.
        /// This parameter gets merged with the default <see cref="LuisPredictionOptions"/> passed in the constructor. This will call Luis V2 endpoint if passed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        [Obsolete("Method is deprecated, please use RecognizeAsync<T>(ITurnContext turnContext, LuisRecognizerOptions recognizerOptions, CancellationToken cancellationToken).")]
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, LuisPredictionOptions predictionOptions, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            var overrideV2 = MergeDefaultOptionsWithProvidedOptionsV2(predictionOptions);
            result.Convert(await RecognizeInternalAsync(turnContext, overrideV2, null, null, cancellationToken).ConfigureAwait(false));
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
        /// <param name="dialogContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        => await RecognizeInternalAsync(dialogContext, activity, null, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="predictionOptions">A <see cref="LuisPredictionOptions"/> instance to be used by the call.
        /// This parameter gets merged with the default <see cref="LuisPredictionOptions"/> passed in the constructor. This will call Luis V2 endpoint if passed.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        [Obsolete("Method is deprecated, please use RecognizeAsync(ITurnContext turnContext, LuisRecognizerOptions recognizerOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken)).")]
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, LuisPredictionOptions predictionOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var overrideV2 = MergeDefaultOptionsWithProvidedOptionsV2(predictionOptions);
            return await RecognizeInternalAsync(turnContext, overrideV2, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);
        }

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
        /// <param name="dialogContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<T> RecognizeAsync<T>(DialogContext dialogContext, Activity activity, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(dialogContext, activity, null, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="predictionOptions">A <see cref="LuisPredictionOptions"/> instance to be used by the call.
        /// This parameter gets merged with the default <see cref="LuisPredictionOptions"/> passed in the constructor. This will use Luis V2 endpoint if passed.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        [Obsolete("Method is deprecated, please use RecognizeAsync(ITurnContext turnContext, LuisRecognizerOptions recognizerOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken)).")]
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, LuisPredictionOptions predictionOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            var overrideV2 = MergeDefaultOptionsWithProvidedOptionsV2(predictionOptions);
            result.Convert(await RecognizeInternalAsync(turnContext, overrideV2, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Runs an utterance through a recognizer and returns a generic recognizer result.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, LuisRecognizerOptions recognizerOptions, CancellationToken cancellationToken)
        {
            return await RecognizeInternalAsync(turnContext, recognizerOptions, null, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs an utterance through a recognizer and returns a generic recognizer result.
        /// </summary>
        /// <param name="dialogContext">dialog context.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, LuisRecognizerOptions recognizerOptions, CancellationToken cancellationToken)
        {
            return await RecognizeInternalAsync(dialogContext, activity, recognizerOptions, null, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly-typed recognizer result.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, LuisRecognizerOptions recognizerOptions, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, recognizerOptions, null, null, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly-typed recognizer result.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="dialogContext">dialog context.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        public virtual async Task<T> RecognizeAsync<T>(DialogContext dialogContext, Activity activity, LuisRecognizerOptions recognizerOptions, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(dialogContext, activity, recognizerOptions, null, null, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, LuisRecognizerOptions recognizerOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await RecognizeInternalAsync(turnContext, recognizerOptions, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <param name="dialogContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, LuisRecognizerOptions recognizerOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await RecognizeInternalAsync(dialogContext, activity, recognizerOptions, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, LuisRecognizerOptions recognizerOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, recognizerOptions, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="dialogContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<T> RecognizeAsync<T>(DialogContext dialogContext, Activity activity, LuisRecognizerOptions recognizerOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(dialogContext, activity, recognizerOptions, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <remarks>No telemetry is provided when using this method.</remarks>
        /// <param name="utterance">utterance to recognize.</param>
        /// <param name="recognizerOptions">A <see cref="LuisRecognizerOptions"/> instance to be used by the call.
        /// This parameter overrides the default <see cref="LuisRecognizerOptions"/> passed in the constructor.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(string utterance, LuisRecognizerOptions recognizerOptions = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            recognizerOptions ??= _luisRecognizerOptions;
            return await RecognizeInternalAsync(utterance, recognizerOptions, cancellationToken).ConfigureAwait(false);
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
            _luisRecognizerOptions.TelemetryClient.TrackEvent(LuisTelemetryConstants.LuisResult, properties, telemetryMetrics);
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
#pragma warning disable CA1801 // Review unused parameters (we can't remove cancellationToken without breaking binary compat).
        protected Task<Dictionary<string, string>> FillLuisEventPropertiesAsync(RecognizerResult recognizerResult, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, CancellationToken cancellationToken = default(CancellationToken))
#pragma warning restore CA1801 // Review unused parameters
        {
            var topTwoIntents = (recognizerResult.Intents.Count > 0) ? recognizerResult.Intents.OrderByDescending(x => x.Value.Score).Take(2).ToArray() : null;

            // Add the intent score and conversation id properties
            var properties = new Dictionary<string, string>()
            {
                { LuisTelemetryConstants.ApplicationIdProperty, _luisRecognizerOptions.Application.ApplicationId },
                { LuisTelemetryConstants.IntentProperty, topTwoIntents?[0].Key ?? string.Empty },
                { LuisTelemetryConstants.IntentScoreProperty, topTwoIntents?[0].Value.Score?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00" },
                { LuisTelemetryConstants.Intent2Property, (topTwoIntents?.Length > 1) ? topTwoIntents?[1].Key ?? string.Empty : string.Empty },
                { LuisTelemetryConstants.IntentScore2Property, (topTwoIntents?.Length > 1) ? topTwoIntents?[1].Value.Score?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00" : "0.00" },
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

        /// <summary>
        /// Returns a LuisRecognizerOptions object with the correspondig version.
        /// If no V2 LuisPredictionoption passed it sets the recognizer to use Luis V3 endpoint.
        /// This exists to maintain backwards compatibility with existing constructors.
        /// </summary>
        /// <param name="application">LuisAplication object to initialize LuisReognizerOptions.</param>
        /// <param name="options">If passed a LuisRecognizerOptionsV2 will be returned initialized with this object.</param>
        /// <param name="includeAPIResults">Boolean value to initialize IncludeAPIResults .</param>
        /// <returns>LuisRecognizerOptions object.</returns>
        private static LuisRecognizerOptions BuildLuisRecognizerOptionsV2(LuisApplication application, LuisPredictionOptions options, bool includeAPIResults)
        {
            if (options == null)
            {
                return new LuisRecognizerOptionsV3(application)
                {
                    IncludeAPIResults = includeAPIResults,
                };
            }

            var luisVersionOptions = new LuisRecognizerOptionsV2(application)
            {
                PredictionOptions = options,
#pragma warning disable CS0618 // Type or member is obsolete
                TelemetryClient = options.TelemetryClient,
                Timeout = options.Timeout,
                LogPersonalInformation = options.LogPersonalInformation,
#pragma warning restore CS0618 // Type or member is obsolete
                IncludeAPIResults = includeAPIResults
            };

            return luisVersionOptions;
        }

        /// <summary>
        /// Returns a RecognizerResult object.
        /// </summary>
        /// <param name="turnContext">Dialog turn Context.</param>
        /// <param name="predictionOptions">LuisRecognizerOptions implementation to override current properties.</param>
        /// <param name="telemetryProperties"> Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>RecognizerResult object.</returns>
        private async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, LuisRecognizerOptions predictionOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics, CancellationToken cancellationToken)
        {
            var recognizer = predictionOptions ?? _luisRecognizerOptions;
            var cached = turnContext.TurnState.Get<RecognizerResult>(_cacheKey);
            if (cached == null)
            {
                var result = await recognizer.RecognizeInternalAsync(turnContext, HttpClient, cancellationToken).ConfigureAwait(false);
                await OnRecognizerResultAsync(result, turnContext, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);
                turnContext.TurnState.Set(_cacheKey, result);
                _luisRecognizerOptions.TelemetryClient.TrackEvent("Luis result cached", telemetryProperties, telemetryMetrics);
                
                return result;
            }

            _luisRecognizerOptions.TelemetryClient.TrackEvent("Read from cached Luis result", telemetryProperties, telemetryMetrics);
            return cached;
        }

        /// <summary>
        /// Returns a RecognizerResult object.
        /// </summary>
        /// <param name="dialogContext">Dialog turn Context.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="predictionOptions">LuisRecognizerOptions implementation to override current properties.</param>
        /// <param name="telemetryProperties"> Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>RecognizerResult object.</returns>
        private async Task<RecognizerResult> RecognizeInternalAsync(DialogContext dialogContext, Activity activity, LuisRecognizerOptions predictionOptions, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics, CancellationToken cancellationToken)
        {
            var recognizer = predictionOptions ?? _luisRecognizerOptions;
            var turnContext = dialogContext.Context;
            var cached = turnContext.TurnState.Get<RecognizerResult>(_cacheKey);
            if (cached == null)
            {
                var result = await recognizer.RecognizeInternalAsync(dialogContext, activity, HttpClient, cancellationToken).ConfigureAwait(false);
                await OnRecognizerResultAsync(result, dialogContext.Context, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);
                turnContext.TurnState.Set(_cacheKey, result);
                _luisRecognizerOptions.TelemetryClient.TrackEvent("Luis result cached", telemetryProperties, telemetryMetrics);
                return result;
            }

            _luisRecognizerOptions.TelemetryClient.TrackEvent("Read from cached Luis result", telemetryProperties, telemetryMetrics);
            return cached;
        }

        /// <summary>
        /// Returns a RecognizerResult object.
        /// </summary>
        /// <param name="utterance">utterance to recognize.</param>
        /// <param name="predictionOptions">LuisRecognizerOptions implementation to override current properties.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>RecognizerResult object.</returns>
        private async Task<RecognizerResult> RecognizeInternalAsync(string utterance, LuisRecognizerOptions predictionOptions, CancellationToken cancellationToken)
        {
            var recognizer = predictionOptions ?? _luisRecognizerOptions;
            var result = await recognizer.RecognizeInternalAsync(utterance, HttpClient, cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Returns a LuisRecognizerOptionsV2.
        /// This exists to maintain backwards compatibility with existing constructors.
        /// </summary>
        /// <param name="overridenOptions">LuisPredictionOptions object to initialize or merge with existing LuisReognizerOptions.</param>
        /// <returns>LuisRecognizerOptions object.</returns>
        private LuisRecognizerOptions MergeDefaultOptionsWithProvidedOptionsV2(LuisPredictionOptions overridenOptions)
        {
            var instanceLuisRecognizer = _luisRecognizerOptions as LuisRecognizerOptionsV2;
            var defaultOptions = instanceLuisRecognizer != null ? instanceLuisRecognizer.PredictionOptions : new LuisPredictionOptions();
            var overrideV2 = new LuisPredictionOptions()
            {
                BingSpellCheckSubscriptionKey = overridenOptions.BingSpellCheckSubscriptionKey ?? defaultOptions.BingSpellCheckSubscriptionKey,
                IncludeAllIntents = overridenOptions.IncludeAllIntents ?? defaultOptions.IncludeAllIntents,
                IncludeInstanceData = overridenOptions.IncludeInstanceData ?? defaultOptions.IncludeInstanceData,
                Log = overridenOptions.Log ?? defaultOptions.Log,
                SpellCheck = overridenOptions.SpellCheck ?? defaultOptions.SpellCheck,
                Staging = overridenOptions.Staging ?? defaultOptions.Staging,
                TimezoneOffset = overridenOptions.TimezoneOffset ?? defaultOptions.TimezoneOffset,
            };
            return BuildLuisRecognizerOptionsV2(_luisRecognizerOptions.Application, overrideV2, _luisRecognizerOptions.IncludeAPIResults);
        }

        private DelegatingHandler CreateHttpHandlerPipeline(HttpClientHandler httpClientHandler, params DelegatingHandler[] handlers)
        {
            // Now, the RetryAfterDelegatingHandler should be the absolute outermost handler
            // because it's extremely lightweight and non-interfering
            DelegatingHandler currentHandler =
#pragma warning disable CA2000 // Dispose objects before losing scope (suppressing this warning, for now! we will address this once we implement HttpClientFactory in a future release)
                new RetryDelegatingHandler(new RetryAfterDelegatingHandler { InnerHandler = httpClientHandler });
#pragma warning restore CA2000 // Dispose objects before losing scope

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
