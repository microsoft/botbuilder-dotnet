using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class HttpRequest : DialogCommand
    {

        public enum HttpMethod {
            GET,
            POST
        }

        public HttpRequest()
            : base()
        {
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

        [JsonProperty("header")]
        public Dictionary<string, string> Header { get; set; }

        [JsonProperty("body")]
        public JObject Body { get; set; }

        private static readonly HttpClient client = new HttpClient();

        public HttpRequest(HttpMethod method, string url, string property, Dictionary<string, string> header = null, JObject body = null)
        {
            this.Method = method;
            this.Url = url ?? throw new ArgumentNullException(nameof(url));
            this.Property = property;
            this.Header = header;
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
                    await ReplaceJTokenRecursively(dc, ((JProperty) token).Value);
                    break;

                default:
                    if (token.Type == JTokenType.String)
                    {
                        token.Replace(await new TextTemplate(token.ToString()).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)));
                    }
                    break;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Single command running with a copy of the original data
            client.DefaultRequestHeaders.Clear();

            JToken instanceBody = null;
            if (this.Body != null)
            {
                instanceBody = (JToken)this.Body.DeepClone();
            }

            var instanceHeader = Header == null ? null: new Dictionary<string, string>(Header);
            var instanceUrl = this.Url;

            instanceUrl = await new TextTemplate(this.Url).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)).ConfigureAwait(false);

            // Bind each string token to the data in state
            if (instanceBody != null)
            {
                await ReplaceJTokenRecursively(dc, instanceBody);
            }

            // Set header
            if (instanceHeader != null)
            {
                foreach (var unit in instanceHeader)
                {
                    client.DefaultRequestHeaders.Add(
                        await new TextTemplate(unit.Key).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)),
                        await new TextTemplate(unit.Value).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)));
                }
            }

            
            HttpResponseMessage response = null;

            if (instanceBody != null && this.Method == HttpMethod.POST)
            {
                response = await client.PostAsync(instanceUrl, new StringContent(instanceBody.ToString(), Encoding.UTF8, "application/json"));
            }

            if (this.Method == HttpMethod.GET)
            {
                response = await client.GetAsync(instanceUrl);
            }

            object result = (object)await response.Content.ReadAsStringAsync();
            // Try set with JOjbect for further retreiving
            try
            {
                result = JToken.Parse((string)result);
            }
            catch
            {
                result = result.ToString();
            }

            return await dc.EndDialogAsync(result);

        }

    }
}
