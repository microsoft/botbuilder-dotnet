// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class HttpRecognizer : IRecognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.HttpRecognizer";

        private static readonly HttpClient Client = new HttpClient();

        public HttpRecognizer()
        {
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("method")]
        public HttpRequest.HttpMethod Method { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty("body")]
        public JToken Body { get; set; }

        // TODO Add XML etc.
        //[JsonProperty("responseType")]
        //public ResponseTypes ResponseType { get; set; } = ResponseTypes.Json;

        [JsonProperty("intents")]
        public string Intents { get; set; }

        [JsonProperty("entities")]
        public string Entities { get; set; }

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Process only messages
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return await Task.FromResult(new RecognizerResult() { Text = turnContext.Activity.Text });
            }

            // Identify matched intents
            var utterance = turnContext.Activity.Text ?? string.Empty;

            var result = new RecognizerResult()
            {
                Text = utterance,
                Intents = new Dictionary<string, IntentScore>(),
            };

            // Single command running with a copy of the original data
            Client.DefaultRequestHeaders.Clear();

            JToken instanceBody = null;
            if (this.Body != null)
            {
                instanceBody = (JToken)this.Body.DeepClone();
            }

            var instanceHeaders = Headers == null ? null : new Dictionary<string, string>(Headers);

            // TODO add other valid memories
            var data = new { turn = turnContext.TurnState[ScopePath.TURN] };

            // TODO add function for http encoding
            var instanceUrl = await new TextTemplate(this.Url).BindToData(turnContext, data).ConfigureAwait(false);

            // Bind each string token to the data in state
            if (instanceBody != null)
            {
                await ReplaceJTokenRecursively(turnContext, instanceBody, data);
            }

            // Set headers
            if (instanceHeaders != null)
            {
                foreach (var unit in instanceHeaders)
                {
                    Client.DefaultRequestHeaders.Add(
                        await new TextTemplate(unit.Key).BindToData(turnContext, data),
                        await new TextTemplate(unit.Value).BindToData(turnContext, data));
                }
            }

            dynamic traceInfo = new JObject();

            traceInfo.request = new JObject();
            traceInfo.request.method = this.Method.ToString();
            traceInfo.request.url = instanceUrl;

            HttpResponseMessage response = null;

            switch (this.Method)
            {
                case HttpRequest.HttpMethod.POST:
                    if (instanceBody == null)
                    {
                        response = await Client.PostAsync(instanceUrl, null);
                    }
                    else
                    {
                        var postContent = new StringContent(instanceBody.ToString(), Encoding.UTF8, "application/json");
                        traceInfo.request.content = instanceBody.ToString();
                        traceInfo.request.headers = JObject.FromObject(postContent?.Headers.ToDictionary(t => t.Key, t => (object)t.Value?.FirstOrDefault()));
                        response = await Client.PostAsync(instanceUrl, postContent);
                    }

                    break;

                case HttpRequest.HttpMethod.PATCH:
                    if (instanceBody == null)
                    {
                        var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"), instanceUrl);
                        response = await Client.SendAsync(request);
                    }
                    else
                    {
                        var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"), instanceUrl);
                        request.Content = new StringContent(instanceBody.ToString(), Encoding.UTF8, "application/json");
                        traceInfo.request.content = instanceBody.ToString();
                        traceInfo.request.headers = JObject.FromObject(request.Content.Headers.ToDictionary(t => t.Key, t => (object)t.Value?.FirstOrDefault()));
                        response = await Client.SendAsync(request);
                    }

                    break;

                case HttpRequest.HttpMethod.PUT:
                    if (instanceBody == null)
                    {
                        response = await Client.PutAsync(instanceUrl, null);
                    }
                    else
                    {
                        var putContent = new StringContent(instanceBody.ToString(), Encoding.UTF8, "application/json");
                        traceInfo.request.content = instanceBody.ToString();
                        traceInfo.request.headers = JObject.FromObject(putContent.Headers.ToDictionary(t => t.Key, t => (object)t.Value?.FirstOrDefault()));
                        response = await Client.PutAsync(instanceUrl, putContent);
                    }

                    break;

                case HttpRequest.HttpMethod.DELETE:
                    response = await Client.DeleteAsync(instanceUrl);
                    break;

                case HttpRequest.HttpMethod.GET:
                    response = await Client.GetAsync(instanceUrl);
                    break;
            }

            // TODO Check StatusCode etc.
            //Result requestResult = new Result(response.Headers)
            //{
            //    StatusCode = (int)response.StatusCode,
            //    ReasonPhrase = response.ReasonPhrase,
            //};

            object content = (object)await response.Content.ReadAsStringAsync();

            try
            {
                content = JToken.Parse((string)content);
            }
            catch
            {
                // TODO Throw error
                content = content.ToString();
            }

            var intents = await new TextTemplate(this.Intents).BindToData(turnContext, content).ConfigureAwait(false);
            result.Intents = JsonConvert.DeserializeObject<IDictionary<string, IntentScore>>(intents);

            if (!string.IsNullOrEmpty(this.Entities))
            {
                var entities = await new TextTemplate(this.Entities).BindToData(turnContext, content).ConfigureAwait(false);
                result.Entities = JObject.Parse(entities);
            }

            // TODO Write Trace Activity for the http request and response values
            // await turnContext.TraceActivityAsync("HttpRecognizer", (object)traceInfo, valueType: "Microsoft.HttpRecognizer", label: this.Id).ConfigureAwait(false);

            return result;
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
    where T : IRecognizerConvert, new()
        {
            var result = await this.RecognizeAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return JObject.FromObject(result).ToObject<T>();
        }

        private async Task ReplaceJTokenRecursively(ITurnContext turnContext, JToken token, object data)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var child in token.Children<JProperty>())
                    {
                        await ReplaceJTokenRecursively(turnContext, child, data);
                    }

                    break;

                case JTokenType.Array:
                    foreach (var child in token.Children())
                    {
                        await ReplaceJTokenRecursively(turnContext, child, data);
                    }

                    break;

                case JTokenType.Property:
                    await ReplaceJTokenRecursively(turnContext, ((JProperty)token).Value, data);
                    break;

                default:
                    if (token.Type == JTokenType.String)
                    {
                        var text = token.ToString();

                        // if it is a "{bindingpath}" then run through expression engine and treat as a value
                        if (text.StartsWith("{") && text.EndsWith("}"))
                        {
                            text = text.Trim('{', '}');
                            var (val, error) = new ExpressionEngine().Parse(text).TryEvaluate(data);
                            token.Replace(new JValue(val));
                        }
                        else
                        {
                            // use text template binding to bind in place to a string
                            var temp = await new TextTemplate(text).BindToData(turnContext, data);
                            token.Replace(new JValue(temp));
                        }
                    }

                    break;
            }
        }
    }
}
