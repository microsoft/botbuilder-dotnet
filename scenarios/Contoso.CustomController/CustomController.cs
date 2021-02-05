using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Contoso.CustomController
{
    [Route("api/custom")]
    [ApiController]
    public class CustomController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        public CustomController(IBotFrameworkHttpAdapter adapter, IBot bot)
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
