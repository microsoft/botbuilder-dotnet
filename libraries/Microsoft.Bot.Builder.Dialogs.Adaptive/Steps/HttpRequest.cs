// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Step for HttpRequests
    /// </summary>
    public class HttpRequest : DialogCommand
    {
        private static readonly HttpClient client = new HttpClient();

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

        public enum HttpMethod
        {
            GET,
            POST,
            PATCH,
            PUT,
            DELETE
        }

        [JsonConstructor]
        public HttpRequest([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override string OnComputeId()
        {
            return $"HttpRequest[{Method} {Url}]";
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("method")]
        public HttpMethod Method { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty("body")]
        public JToken Body { get; set; }

        [JsonProperty("responseType")]
        public ResponseTypes ResponseType { get; set; } = ResponseTypes.Json;

        /// <summary>
        /// Property which is bidirectional property for input and output.  Example: user.age will be passed in, and user.age will be set when the dialog completes
        /// </summary>
        public string Property
        {
            get
            {
                return OutputBinding;
            }

            set
            {
                InputBindings[DialogContextState.DIALOG_VALUE] = value;
                OutputBinding = value;
            }
        }

        public HttpRequest(HttpMethod method, string url, string property, Dictionary<string, string> headers = null, JObject body = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Method = method;
            this.Url = url ?? throw new ArgumentNullException(nameof(url));
            this.Property = property;
            this.Headers = headers;
            this.Body = body;
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
                    foreach (var child in token.Children())
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
                        var temp = await new TextTemplate(token.ToString()).BindToData(dc.Context, dc.State);
                        if ((temp.StartsWith("{") && temp.EndsWith("}")) || (temp.StartsWith("[") && temp.EndsWith("]")))
                        {
                            // try parse with json                        
                            var jtoken = JToken.Parse(temp);
                            token.Replace(jtoken);
                        }
                        else
                        {
                            token.Replace(temp);
                        }
                    }

                    break;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // Single command running with a copy of the original data
            client.DefaultRequestHeaders.Clear();

            JToken instanceBody = null;
            if (this.Body != null)
            {
                instanceBody = (JToken)this.Body.DeepClone();
            }

            var instanceHeaders = Headers == null ? null : new Dictionary<string, string>(Headers);
            var instanceUrl = this.Url;

            instanceUrl = await new TextTemplate(this.Url).BindToData(dc.Context, dc.State).ConfigureAwait(false);

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
                    client.DefaultRequestHeaders.Add(
                        await new TextTemplate(unit.Key).BindToData(dc.Context, dc.State),
                        await new TextTemplate(unit.Value).BindToData(dc.Context, dc.State));
                }
            }

            HttpResponseMessage response = null;

            switch (this.Method)
            {
                case HttpMethod.POST:
                    if (instanceBody == null)
                    {
                        response = await client.PostAsync(instanceUrl, null);
                    }
                    else
                    {
                        response = await client.PostAsync(instanceUrl, new StringContent(instanceBody.ToString(), Encoding.UTF8, "application/json"));
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
                        request.Content = new StringContent(instanceBody.ToString(), Encoding.UTF8, "application/json");
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
                        response = await client.PutAsync(instanceUrl, new StringContent(instanceBody.ToString(), Encoding.UTF8, "application/json"));
                    }

                    break;

                case HttpMethod.DELETE:
                    response = await client.DeleteAsync(instanceUrl);
                    break;

                case HttpMethod.GET:
                    response = await client.GetAsync(instanceUrl);
                    break;
            }

            object result = (object)await response.Content.ReadAsStringAsync();

            switch (this.ResponseType)
            {
                case ResponseTypes.Activity:
                    var activity = JsonConvert.DeserializeObject<Activity>((string)result);
                    await dc.Context.SendActivityAsync(activity, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return await dc.EndDialogAsync(cancellationToken: cancellationToken);

                case ResponseTypes.Activities:
                    var activities = JsonConvert.DeserializeObject<Activity[]>((string)result);
                    await dc.Context.SendActivitiesAsync(activities, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return await dc.EndDialogAsync(cancellationToken: cancellationToken);

                case ResponseTypes.Json:
                    // Try set with JOjbect for further retreiving
                    try
                    {
                        result = JToken.Parse((string)result);
                    }
                    catch
                    {
                        result = result.ToString();
                    }

                    return await dc.EndDialogAsync(result, cancellationToken: cancellationToken);

                case ResponseTypes.None:
                default:
                    return await dc.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
