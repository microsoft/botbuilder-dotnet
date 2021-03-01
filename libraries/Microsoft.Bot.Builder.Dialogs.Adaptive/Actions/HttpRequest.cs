// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.HttpRequest";

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequest"/> class.
        /// </summary>
        /// <param name="method">The HTTP method, for example POST, GET, DELETE or PUT.</param>
        /// <param name="url">URL for the request.</param>
        /// <param name="headers">Optional, the headers of the request.</param>
        /// <param name="body">Optional, the raw body of the request.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public HttpRequest(HttpMethod method, string url, Dictionary<string, StringExpression> headers = null, object body = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Method = method;
            this.Url = url ?? throw new ArgumentNullException(nameof(url));
            this.Headers = headers;
            this.Body = JToken.FromObject(body);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequest"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public HttpRequest([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// List of possible response types.
        /// </summary>
#pragma warning disable CA1717 // Only FlagsAttribute enums should have plural names (we can't change this without breaking binary compat).
        public enum ResponseTypes
#pragma warning restore CA1717 // Only FlagsAttribute enums should have plural names
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
            Activities,

            /// <summary>
            /// Binary data parsing from http response content
            /// </summary>
            Binary
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
        /// <value>Content type such as "application/json" or "text/plain".  Default is "application/json".</value>
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
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public Dictionary<string, StringExpression> Headers { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

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

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

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
            instanceBody = instanceBody?.ReplaceJTokenRecursively(dc.State);

            using (HttpRequestMessage request = new HttpRequestMessage(new System.Net.Http.HttpMethod(this.Method.ToString()), instanceUrl))
            {
                // Set headers
                if (instanceHeaders != null)
                {
                    foreach (var unit in instanceHeaders)
                    {
                        request.Headers.TryAddWithoutValidation(unit.Key, unit.Value);
                    }
                }

                dynamic traceInfo = new ExpandoObject();
                traceInfo.request = new ExpandoObject();
                traceInfo.request.method = this.Method.ToString();
                traceInfo.request.url = instanceUrl;
                string contentType = ContentType?.GetValue(dc.State) ?? "application/json";

                if (this.Method == HttpMethod.PATCH || this.Method == HttpMethod.POST || this.Method == HttpMethod.PUT)
                {
                    var contentString = instanceBody.ToString();
                    request.Content = new StringContent(contentString, Encoding.UTF8, contentType);
                    traceInfo.request.content = contentString;
                }

                HttpResponseMessage response = await SendRequestAsync(dc, request, cancellationToken).ConfigureAwait(false);
                traceInfo.request.headers = JObject.FromObject(request.Headers.ToDictionary(t => t.Key, t => (object)t.Value?.FirstOrDefault()));

                var requestResult = new Result(response.Headers)
                {
                    StatusCode = (int)response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                };

                var responseType = this.ResponseType.GetValue(dc.State);
                if (responseType == ResponseTypes.Binary)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    requestResult.Content = bytes;
                }
                else
                {
                    string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    switch (this.ResponseType.GetValue(dc.State))
                    {
                        case ResponseTypes.Activity:
                            var activity = JsonConvert.DeserializeObject<Activity>(content);
                            requestResult.Content = JObject.FromObject(activity);
                            await dc.Context.SendActivityAsync(activity, cancellationToken: cancellationToken).ConfigureAwait(false);
                            break;

                        case ResponseTypes.Activities:
                            var activities = JsonConvert.DeserializeObject<Activity[]>(content);
                            requestResult.Content = JArray.FromObject(activities);
                            await dc.Context.SendActivitiesAsync(activities, cancellationToken: cancellationToken).ConfigureAwait(false);
                            break;

                        case ResponseTypes.Json:
                            // Try set with JOjbect for further retrieving
                            try
                            {
                                requestResult.Content = JToken.Parse(content);
                            }
#pragma warning disable CA1031 // Do not catch general exception types (just stringify the content if we can't parse the content).
                            catch
#pragma warning restore CA1031 // Do not catch general exception types
                            {
                                requestResult.Content = content;
                            }

                            break;

                        case ResponseTypes.None:
                        default:
                            break;
                    }
                }

                traceInfo.response = JObject.FromObject(requestResult);

                // Write Trace Activity for the http request and response values
                await dc.Context.TraceActivityAsync("HttpRequest", (object)traceInfo, valueType: "Microsoft.HttpRequest", label: this.Id).ConfigureAwait(false);

                if (this.ResultProperty != null)
                {
                    dc.State.SetValue(this.ResultProperty.GetValue(dc.State), requestResult);
                }

                // return the actionResult as the result of this operation
                return await dc.EndDialogAsync(result: requestResult, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{Method} {Url?.ToString()}]";
        }

        /// <summary>
        /// Sends the request. It uses the HttpClient from the TurnState. If TurnState doesn't have HttpClient, creates a new instance of HttpClient.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="message">The http request message to send. Method mutates this object to add the user-agent if needed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The http response.</returns>
        protected virtual async Task<HttpResponseMessage> SendRequestAsync(DialogContext dc, HttpRequestMessage message, CancellationToken cancellationToken)
        {
            var client = dc.Context.TurnState.Get<HttpClient>();

            // if there no user-agent in the header, set the user-agent to Mozzila/5.0
            if (client == null || !client.DefaultRequestHeaders.Contains("user-agent"))
            {
                message.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0");
            }

            if (client != null)
            {
                return await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                using (client = new HttpClient())
                {
                    return await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Result data of the the http operation.
        /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible (this should have been a separate class but we can't change it without breaking binary compat).
        public class Result
#pragma warning restore CA1034 // Nested types should not be visible
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Result"/> class.
            /// </summary>
            public Result()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Result"/> class.
            /// </summary>
            /// <param name="headers">HTTP headers from the response to the http operation.</param>
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
