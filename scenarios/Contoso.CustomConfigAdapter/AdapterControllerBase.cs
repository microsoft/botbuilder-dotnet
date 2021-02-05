using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Contoso.CustomConfigAdapter
{
    public class AdapterControllerBase<TAdapter> : ControllerBase
        where TAdapter : IBotFrameworkHttpAdapter
    {
        private readonly TAdapter _adapter;
        private readonly IBot _bot;

        public AdapterControllerBase(TAdapter adapter, IBot bot)
        {
            this._adapter = adapter;
            this._bot = bot;
        }

        [HttpPost]
        [HttpGet]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await this._adapter.ProcessAsync(Request, Response, _bot).ConfigureAwait(false);
        }
    }
}
