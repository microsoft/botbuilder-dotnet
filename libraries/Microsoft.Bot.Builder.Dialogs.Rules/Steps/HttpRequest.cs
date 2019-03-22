using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

        public HttpMethod Method { get; set; }

        public string Url { get; set; }

        public string ResponseProperty { get; set; }

        public Dictionary<string, string> Header { get; set; }

        public JObject Body { get; set; }

        private static readonly HttpClient client = new HttpClient();

        public HttpRequest(HttpMethod method, string url, string responseProperty, Dictionary<string, string> header = null, JObject body = null)
        {
            this.Method = method;
            this.Url = url ?? throw new ArgumentNullException(nameof(url));
            this.ResponseProperty = responseProperty;
            this.Header = header;
            this.Body = body;
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Single command running with a copy of the original data
            JObject instanceBody = null;
            try
            {
                instanceBody = this.Body;
            }
            catch
            {
                return await dc.EndDialogAsync();
            }

            var instanceHeader = Header == null ? null: new Dictionary<string, string>(Header);
            var instanceUrl = this.Url;

            instanceUrl = await new TextTemplate(this.Url).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)).ConfigureAwait(false);

            // Bind each string token to the data in state
            if (instanceBody != null)
            {
                foreach (var unit in instanceBody)
                {
                    if (unit.Value.Type == JTokenType.String)
                    {
                        unit.Value.Replace(await new TextTemplate(unit.Value.ToString()).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)));
                    }
                }
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

            var res = response.Content.ReadAsStringAsync();

            // Try set with JOjbect for further retreiving
            try
            {
                dc.State.SetValue(ResponseProperty, JObject.Parse(res.Result));
            }
            catch
            {
                dc.State.SetValue(ResponseProperty, res.Result);
            }

            return await dc.EndDialogAsync();

        }

    }
}
