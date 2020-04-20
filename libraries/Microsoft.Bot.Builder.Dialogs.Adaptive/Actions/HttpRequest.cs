// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action for performing an HttpRequest.
    /// </summary>
    public class HttpRequest : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.HttpRequest";

        public HttpRequest(HttpMethod method, string url, Dictionary<string, StringExpression> headers = null, object body = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Method = method;
            this.Url = url ?? throw new ArgumentNullException(nameof(url));
            this.Headers = headers;
            this.Body = JToken.FromObject(body);
        }

        [JsonConstructor]
        public HttpRequest([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public enum ResponseTypes
        {
            /// <summary>
            /// No response expected
            /// </summary>
            None,

            /// <summary>
            /// Plain JSON response 
            /// </summary>
            Json,

            /// <summary>
            /// JSON Activity object to send to the user
            /// </summary>
            Activity,

            /// <summary>
            /// Json Array of activity objects to send to the user
            /// </summary>
            Activities
        }

        /// <summary>
        /// Http methods.
        /// </summary>
        public enum HttpMethod
        {
            /// <summary>
            /// Http GET.
            /// </summary>
            /// 
            GET,

            /// <summary>
            /// Http POST.
            /// </summary>
            POST,

            /// <summary>
            /// Http PATCH.
            /// </summary>
            PATCH,

            /// <summary>
            /// Http PUT.
            /// </summary>
            PUT,

            /// <summary>
            /// Http DELETE.
            /// </summary>
            DELETE
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the HttpMethod to use.
        /// </summary>
        /// <value>
        /// HttpMethod.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("method")]
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the content type for the body of the http operation.
        /// </summary>
        /// <value>Content type such as "application/json" or "test/plain".  Default is "application/json".</value>
        [DefaultValue("application/json")]
        [JsonProperty("contentType")]
        public StringExpression ContentType { get; set; } = "application/json";

        /// <summary>
        /// Gets or sets the Url.
        /// </summary>
        /// <value>url.</value>
        [JsonProperty("url")]
        public StringExpression Url { get; set; }

        /// <summary>
        /// Gets or sets headers.
        /// </summary>
        /// <value>
        /// Headers.
        /// </value>
        [JsonProperty("headers")]
        public Dictionary<string, StringExpression> Headers { get; set; }

        /// <summary>
        /// Gets or sets body payload.
        /// </summary>
        /// <value>
        /// Body payload.
        /// </value>
        [JsonProperty("body")]
        public ValueExpression Body { get; set; }

        /// <summary>
        /// Gets or sets the ResponseType.
        /// </summary>
        /// <value>
        /// The ResponseType.
        /// </value>
        [JsonProperty("responseType")]
        public EnumExpression<ResponseTypes> ResponseType { get; set; } = ResponseTypes.Json;

        /// <summary>
        /// Gets or sets the property expression to store the HTTP response in. 
        /// </summary>
        /// <remarks>
        /// The result will have 4 properties from the http response: 
        /// [statusCode|reasonPhrase|content|headers]
        /// If the content is json it will be an deserialized object, otherwise it will be a string.
        /// </remarks>
        /// <value>
        /// The property expression to store the HTTP response in. 
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var client = new HttpClient();

            // Single command running with a copy of the original data
            client.DefaultRequestHeaders.Clear();

            JToken instanceBody = null;
            if (this.Body != null)
            {
                var (body, err) = this.Body.TryGetValue(dc.State);
                if (err != null)
                {
                    throw new ArgumentException(err);
                }

                instanceBody = (JToken)JToken.FromObject(body).DeepClone();
            }

            var instanceHeaders = Headers == null ? null : Headers.ToDictionary(kv => kv.Key, kv => kv.Value.GetValue(dc.State));

            var (instanceUrl, instanceUrlError) = this.Url.TryGetValue(dc.State);
            if (instanceUrlError != null)
            {
                throw new ArgumentException(instanceUrlError);
            }

            // Bind each string token to the data in state
            if (instanceBody != null)
            {
                await ReplaceJTokenRecursively(dc, instanceBody);
            }

            // Set headers
            if (instanceHeaders != null)
            {
                foreach (var unit in instanceHeaders)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(unit.Key, unit.Value);
                }
            }

            dynamic traceInfo = new JObject();

            traceInfo.request = new JObject();
            traceInfo.request.method = this.Method.ToString();
            traceInfo.request.url = instanceUrl;

            HttpResponseMessage response = null;
            string contentType = ContentType?.GetValue(dc.State) ?? "application/json";

            switch (this.Method)
            {
                case HttpMethod.POST:
                    if (instanceBody == null)
                    {
                        response = await client.PostAsync(instanceUrl, null);
                    }
                    else
                    {
                        var postContent = new StringContent(instanceBody.ToString(), Encoding.UTF8, contentType);
                        traceInfo.request.content = instanceBody.ToString();
                        traceInfo.request.headers = JObject.FromObject(postContent?.Headers.ToDictionary(t => t.Key, t => (object)t.Value?.FirstOrDefault()));
                        response = await client.PostAsync(instanceUrl, postContent);
                    }

                    break;

                case HttpMethod.PATCH:
                    if (instanceBody == null)
                    {
                        var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"), instanceUrl);
                        response = await client.SendAsync(request);
                    }
                    else
                    {
                        var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"), instanceUrl);
                        request.Content = new StringContent(instanceBody.ToString(), Encoding.UTF8, contentType);
                        traceInfo.request.content = instanceBody.ToString();
                        traceInfo.request.headers = JObject.FromObject(request.Content.Headers.ToDictionary(t => t.Key, t => (object)t.Value?.FirstOrDefault()));
                        response = await client.SendAsync(request);
                    }

                    break;

                case HttpMethod.PUT:
                    if (instanceBody == null)
                    {
                        response = await client.PutAsync(instanceUrl, null);
                    }
                    else
                    {
                        var putContent = new StringContent(instanceBody.ToString(), Encoding.UTF8, contentType);
                        traceInfo.request.content = instanceBody.ToString();
                        traceInfo.request.headers = JObject.FromObject(putContent.Headers.ToDictionary(t => t.Key, t => (object)t.Value?.FirstOrDefault()));
                        response = await client.PutAsync(instanceUrl, putContent);
                    }

                    break;

                case HttpMethod.DELETE:
                    response = await client.DeleteAsync(instanceUrl);
                    break;

                case HttpMethod.GET:
                    response = await client.GetAsync(instanceUrl);
                    break;
            }

            Result requestResult = new Result(response.Headers)
            {
                StatusCode = (int)response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
            };

            object content = (object)await response.Content.ReadAsStringAsync();

            switch (this.ResponseType.GetValue(dc.State))
            {
                case ResponseTypes.Activity:
                    var activity = JsonConvert.DeserializeObject<Activity>((string)content);
                    requestResult.Content = JObject.FromObject(activity);
                    await dc.Context.SendActivityAsync(activity, cancellationToken: cancellationToken).ConfigureAwait(false);
                    break;

                case ResponseTypes.Activities:
                    var activities = JsonConvert.DeserializeObject<Activity[]>((string)content);
                    requestResult.Content = JObject.FromObject(activities);
                    await dc.Context.SendActivitiesAsync(activities, cancellationToken: cancellationToken).ConfigureAwait(false);
                    break;

                case ResponseTypes.Json:
                    // Try set with JOjbect for further retreiving
                    try
                    {
                        content = JToken.Parse((string)content);
                    }
                    catch
                    {
                        content = content.ToString();
                    }

                    requestResult.Content = content;
                    break;

                case ResponseTypes.None:
                default:
                    break;
            }

            traceInfo.response = JObject.FromObject(requestResult);

            // Write Trace Activity for the http request and response values
            await dc.Context.TraceActivityAsync("HttpRequest", (object)traceInfo, valueType: "Microsoft.HttpRequest", label: this.Id).ConfigureAwait(false);

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), requestResult);
            }

            // return the actionResult as the result of this operation
            return await dc.EndDialogAsync(result: requestResult, cancellationToken: cancellationToken);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{Method} {Url?.ToString()}]";
        }

        private async Task ReplaceJTokenRecursively(DialogContext dc, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var child in token.Children<JProperty>())
                    {
                        await ReplaceJTokenRecursively(dc, child);
                    }

                    break;

                case JTokenType.Array:
                    // NOTE: ToList() is required because JToken.Replace will break the enumeration.
                    foreach (var child in token.Children().ToList())
                    {
                        await ReplaceJTokenRecursively(dc, child);
                    }

                    break;

                case JTokenType.Property:
                    await ReplaceJTokenRecursively(dc, ((JProperty)token).Value);
                    break;

                default:
                    if (token.Type == JTokenType.String)
                    {
                        var text = token.ToString();

                        // if it is a "{bindingpath}" then run through expression parser and treat as a value
                        var (result, error) = new ValueExpression(text).TryGetValue(dc.State);
                        if (error == null)
                        {
                            token.Replace(JToken.FromObject(result));
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Result data of the the http operation.
        /// </summary>
        public class Result
        {
            public Result()
            {
            }

            public Result(HttpHeaders headers)
            {
                this.Headers = headers.ToDictionary(t => t.Key, t => t.Value.First());
            }

            /// <summary>
            /// Gets or sets the status code from the response to the http operation.
            /// </summary>
            /// <value>Response status code.</value>
            [JsonProperty("statusCode")]
            public int StatusCode { get; set; }

            /// <summary>
            /// Gets or sets the reason phrase from the response to the http operation.
            /// </summary>
            /// <value>Response reason phrase.</value>
            [JsonProperty("reasonPhrase")]
            public string ReasonPhrase { get; set; }

            /// <summary>
            /// Gets the headers from the response to the http operation.
            /// </summary>
            /// <value>Response headers.</value>
            [JsonProperty("headers")]
            public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

            /// <summary>
            /// Gets or sets the content body from the response to the http operation.
            /// </summary>
            /// <value>Response content body.</value>
            [JsonProperty("content")]
            public object Content { get; set; }
        }
    }
}
