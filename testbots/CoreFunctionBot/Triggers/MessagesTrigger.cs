using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreFunctionBot.Triggers
{
    /// <summary>
    /// Functions trigger for Bot Framework messages.
    /// </summary>
    public class MessagesTrigger
    {
        private readonly Dictionary<string, IBotFrameworkHttpAdapter> _adapters = new Dictionary<string, IBotFrameworkHttpAdapter>();
        private readonly IBot _bot;
        private readonly ILogger<MessagesTrigger> _logger;

        public MessagesTrigger(
            IConfiguration configuration,
            IEnumerable<IBotFrameworkHttpAdapter> adapters,
            IBot bot,
            ILogger<MessagesTrigger> logger)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _logger = logger;

            IBotFrameworkHttpAdapter adapter = adapters.FirstOrDefault(s => s.GetType().FullName == typeof(AdapterWithErrorHandler).FullName);
            if (adapter == null)
            {
                throw new ArgumentException($"A IBotFrameworkHttpAdapter must be registered for the {nameof(MessagesTrigger)} to process incoming messages.");
            }

            _adapters.Add("messages", adapter);
        }

        /// <summary>
        /// Bot Framework messages trigger handling.
        /// </summary>
        /// <param name="req">
        /// The <see cref="HttpRequest"/>.
        /// </param>
        /// <param name="adapterRoute">
        /// The route that is being requested.
        /// </param>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        [FunctionName("messages")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/{adapterRoute}")] HttpRequest req, string adapterRoute)
        {
            if (string.IsNullOrEmpty(adapterRoute))
            {
                _logger.LogError($"RunAsync: No route provided.");
                throw new ArgumentNullException(nameof(adapterRoute));
            }

            if (_adapters.TryGetValue(adapterRoute, out IBotFrameworkHttpAdapter adapter))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"RunAsync: routed '{adapterRoute}' to {adapter.GetType().Name}");
                }

                // Delegate the processing of the HTTP POST to the appropriate adapter.
                // The adapter will invoke the bot.
                // IBotFrameworkHttpAdapter is expected to set the appropriate HttpResponse properties
                // (eg. Status and Body), so there is no need to return an IActionResult that will
                // set different HttpResponse values.
                await adapter.ProcessAsync(req, req.HttpContext.Response, _bot).ConfigureAwait(false);
                return new EmptyResult();
            }

            _logger.LogError($"RunAsync: No adapter registered and enabled for route {adapterRoute}.");
            throw new KeyNotFoundException($"No adapter registered and enabled for route {adapterRoute}.");
        }
    }
}
