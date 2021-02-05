using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Settings;

namespace BotName.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly Dictionary<string, IBotFrameworkHttpAdapter> _adapters = new Dictionary<string, IBotFrameworkHttpAdapter>();
        private readonly IBot _bot;

        public BotController(
            IEnumerable<IBotFrameworkHttpAdapter> adapters, 
            IEnumerable<AdapterSettings> adapterSettings, 
            IBot bot)
        {
            this._bot = bot ?? throw new ArgumentNullException(nameof(bot));

            foreach (var adapter in adapters ?? throw new ArgumentNullException(nameof(adapters)))
            {
                _adapters.Add(adapterSettings.Single(s => s.Name == adapter.GetType().FullName).Route, adapter);
            }
        }

        [HttpPost]
        [HttpGet]
        [Route("api/{route}")]
        public async Task PostAsync(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                throw new ArgumentNullException(nameof(route));
            }

            IBotFrameworkHttpAdapter adapter;

            if (_adapters.TryGetValue(route, out adapter))
            {
                // Delegate the processing of the HTTP POST to the appropriate adapter.
                // The adapter will invoke the bot.
                await adapter.ProcessAsync(Request, Response, _bot).ConfigureAwait(false);
            }
            else
            {
                throw new KeyNotFoundException($"No adapter registered for route {route}.");
            }
        }
    }
}
