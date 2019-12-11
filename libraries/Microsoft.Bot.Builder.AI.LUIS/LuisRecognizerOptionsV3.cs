// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
        /// Gets or sets the Luis Prediction Options for the V3 endpoint.
        /// </summary>
        /// <value> This settings will be used to call Luis.</value>
        public LuisV3.LuisPredictionOptions PredictionOptions { get; set; } = new LuisV3.LuisPredictionOptions();

        internal override async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, HttpClient httpClient, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);
            if (turnContext.Activity == null || turnContext.Activity.Type != ActivityTypes.Message)
            {
                return null;
            }

            var options = PredictionOptions;
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
                var uri = BuildUri();
                var content = BuildRequestBody(utterance);
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

        private UriBuilder BuildUri()
        {
            var options = PredictionOptions;
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

        private JObject BuildRequestBody(string utterance)
        {
            var options = PredictionOptions;
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

            return content;
        }
    }
}
