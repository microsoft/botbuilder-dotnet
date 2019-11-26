using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

        public LuisRecognizerOptionsV3(LuisApplication application)
        : base(application)
        {
        }

        public LuisV3.LuisPredictionOptions PredictionOptions { get; set; } = new LuisV3.LuisPredictionOptions();

        internal override async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, HttpClient httpClient, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);
            if (turnContext.Activity.Type != ActivityTypes.Message)
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
                var uri = new UriBuilder(Application.Endpoint);

                // TODO: When the endpoint GAs, we will need to change this.  I could make it an option, but other code is likely to need to change.
                uri.Path += $"luis/prediction/v3.0/apps/{Application.ApplicationId}";

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

        private string AddParam(string query, string prop, bool? val)
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
    }
}
