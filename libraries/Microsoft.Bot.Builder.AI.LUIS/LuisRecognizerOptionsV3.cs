// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis
{
    public class LuisRecognizerOptionsV3 : LuisRecognizerOptions
    {
        /// <summary>
        /// The value type for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";

        /// <summary>
        /// The context label for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceLabel = "LuisV3 Trace";

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRecognizerOptionsV3"/> class.
        /// </summary>
        /// <param name="application">The LUIS application to use to recognize text.</param>
        public LuisRecognizerOptionsV3(LuisApplication application)
        : base(application)
        {
        }

        /// <summary>
        /// Gets or sets entity recognizer to recognize external entities to pass to LUIS.
        /// </summary>
        /// <value>External entity recognizer.</value>
        [JsonProperty("externalEntityRecognizer")]
        public Recognizer ExternalEntityRecognizer { get; set; }

        /// <summary>
        /// Gets or sets the Luis Prediction Options for the V3 endpoint.
        /// </summary>
        /// <value> This settings will be used to call Luis.</value>
        public LuisV3.LuisPredictionOptions PredictionOptions { get; set; } = new LuisV3.LuisPredictionOptions();

        internal override async Task<RecognizerResult> RecognizeInternalAsync(DialogContext dialogContext, HttpClient httpClient, CancellationToken cancellationToken)
        {
            var utterance = dialogContext.Context.Activity?.AsMessageActivity()?.Text;
            var options = PredictionOptions;
            if (ExternalEntityRecognizer != null)
            {
                var matches = await ExternalEntityRecognizer.RecognizeAsync(dialogContext, dialogContext.Context.Activity, cancellationToken).ConfigureAwait(false);
                if (matches.Entities != null && matches.Entities.Count > 2)
                {
                    options = new LuisV3.LuisPredictionOptions(options);
                    options.ExternalEntities = new List<LuisV3.ExternalEntity>();
                    var entities = matches.Entities;
                    var instance = entities["$instance"].ToObject<JObject>();
                    if (instance != null)
                    {
                        foreach (var child in entities)
                        {
                            // TODO: Checking for "text" because we get an extra non-real entity from the text recognizers
                            if (child.Key != "text" && child.Key != "$instance")
                            {
                                var instances = instance[child.Key]?.ToObject<JArray>();
                                var values = child.Value.ToObject<JArray>();
                                if (instances != null && values != null
                                    && instances.Count == values.Count)
                                {
                                    for (var i = 0; i < values.Count; ++i)
                                    {
                                        var childInstance = instances[i].ToObject<JObject>();
                                        if (childInstance != null
                                            && childInstance.ContainsKey("startIndex")
                                            && childInstance.ContainsKey("endIndex"))
                                        {
                                            var start = childInstance["startIndex"].Value<int>();
                                            var end = childInstance["endIndex"].Value<int>();
                                            options.ExternalEntities.Add(new LuisV3.ExternalEntity(child.Key, start, end - start, child.Value));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return await RecognizeAsync(dialogContext.Context, utterance, options, httpClient, cancellationToken).ConfigureAwait(false);
        }

        internal override async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, HttpClient httpClient, CancellationToken cancellationToken)
        {
            return await RecognizeAsync(turnContext, turnContext?.Activity?.AsMessageActivity()?.Text, PredictionOptions, httpClient, cancellationToken).ConfigureAwait(false);
        }

        private async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, string utterance, LuisV3.LuisPredictionOptions options, HttpClient httpClient, CancellationToken cancellationToken)
        {
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
                var uri = BuildUri(options);
                var content = BuildRequestBody(utterance, options);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Application.EndpointKey);
                var response = await httpClient.PostAsync(uri.Uri, new StringContent(content.ToString(), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                httpClient.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
                luisResponse = (JObject)JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                var prediction = (JObject)luisResponse["prediction"];
                recognizerResult = new RecognizerResult();

                recognizerResult.Text = utterance;
                recognizerResult.AlteredText = prediction["alteredQuery"]?.Value<string>();
                recognizerResult.Intents = LuisV3.LuisUtil.GetIntents(prediction);
                recognizerResult.Entities = LuisV3.LuisUtil.ExtractEntitiesAndMetadata(prediction);

                LuisV3.LuisUtil.AddProperties(prediction, recognizerResult);
                if (IncludeAPIResults)
                {
                    recognizerResult.Properties.Add("luisResult", luisResponse);
                }

                if (PredictionOptions.IncludeInstanceData)
                {
                    var instanceObject = recognizerResult.Entities["$instance"];
                    if (instanceObject == null)
                    {
                        recognizerResult.Entities.Add("$instance", new JObject());
                    }
                }
            }

            var traceInfo = JObject.FromObject(
                new
                {
                    recognizerResult,
                    luisModel = new
                    {
                        ModelID = Application.ApplicationId,
                    },
                    luisOptions = options,
                    luisResult = luisResponse,
                });

            await turnContext.TraceActivityAsync("LuisRecognizer", traceInfo, LuisTraceType, LuisTraceLabel, cancellationToken).ConfigureAwait(false);
            return recognizerResult;
        }

        private UriBuilder BuildUri(LuisV3.LuisPredictionOptions options)
        {
            var path = new StringBuilder(Application.Endpoint);
            path.Append($"/luis/prediction/v3.0/apps/{Application.ApplicationId}");

            if (options.Version == null)
            {
                path.Append($"/slots/{options.Slot}/predict");
            }
            else
            {
                path.Append($"/versions/{options.Version}/predict");
            }

            var uri = new UriBuilder(path.ToString());

            var query = HttpUtility.ParseQueryString(uri.Query);
            query["verbose"] = options.IncludeInstanceData.ToString();
            query["log"] = options.Log.ToString();
            query["show-all-intents"] = options.IncludeAllIntents.ToString();
            uri.Query = query.ToString();
            return uri;
        }

        private JObject BuildRequestBody(string utterance, LuisV3.LuisPredictionOptions options)
        {
            var content = new JObject
                {
                    { "query", utterance },
                };
            var queryOptions = new JObject
                {
                    { "preferExternalEntities", options.PreferExternalEntities },
                };

            if (!string.IsNullOrEmpty(options.DateTimeReference))
            {
                queryOptions.Add("datetimeReference", options.DateTimeReference);
            }

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

            return content;
        }
    }
}
