using System;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace <%= botName %>.Triggers
{
    /// <summary>
    /// Functions trigger for Bot Framework messages.
    /// </summary>
    public class MessagesTrigger
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        public MessagesTrigger(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            this._adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this._bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        /// <summary>
        /// Bot Framework messages trigger handling.
        /// </summary>
        /// <param name="req">
        /// The <see cref="HttpRequest"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        [FunctionName("messages")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _adapter.ProcessAsync(req, req.HttpContext.Response, _bot).ConfigureAwait(false);

            if (req.HttpContext.Response.IsSuccessStatusCode())
            {
                return new OkResult();
            }

            return new ContentResult()
            {
                StatusCode = req.HttpContext.Response.StatusCode,
            };
        }
    }
}
