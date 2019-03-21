using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class HttpRequest : DialogCommand
    {

        public enum Method {
            GET,
            POST
        }

        public HttpRequest()
            : base()
        {
        }

        protected override string OnComputeId()
        {
            return $"HttpRequest[{method}]";
        }

        public Method method;

        public string url;

        public string responseProperty;

        public Dictionary<string, string> header;

        public JObject body;

        public HttpRequest(Method method, string url, string responseProperty, Dictionary<string, string> header = null, JObject body = null)
        {

            this.method = method;
            this.url = url;
            this.responseProperty = responseProperty;
            if (header != null)
            {
                this.header = header;
            }
            
            if (body != null)
            {
                this.body = body;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.url = await new TextTemplate(this.url).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)).ConfigureAwait(false);

            // Bind each string token to the data in state
            if (body != null)
            {
                foreach (var unit in this.body)
                {
                    if (unit.Value.Type == JTokenType.String)
                    {
                        unit.Value.Replace(await new TextTemplate(unit.Value.ToString()).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)));
                    }
                }
            }

            var client = new HttpClient();
            if (header != null)
            {
                foreach (var unit in header)
                {
                    client.DefaultRequestHeaders.Add(
                        await new TextTemplate(unit.Key).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)),
                        await new TextTemplate(unit.Value).BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)));
                }
            }
            HttpResponseMessage response = null;

            if (body != null && this.method == Method.POST)
            {
                response = await client.PostAsync(this.url, new StringContent(body.ToString(), Encoding.UTF8, "application/json"));
            }

            if (this.method == Method.GET)
            {
                response = await client.GetAsync(this.url);
            }

            var res = response.Content.ReadAsStringAsync();

            dc.State.SetValue(responseProperty, JObject.Parse(res.Result));

            return await dc.EndDialogAsync();

        }

    }
}
