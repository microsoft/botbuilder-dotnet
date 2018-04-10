using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai
{
    public class SmartTalkMiddleware : IMiddleware
    {
        private readonly SmartTalk _smartTalk;
        private readonly SmartTalkMiddlewareOptions _options;

        public SmartTalkMiddleware(SmartTalkMiddlewareOptions options, HttpClient httpClient = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _smartTalk = new SmartTalk(options, httpClient);
        }

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var messageActivity = context.Activity.AsMessageActivity();
                if (!string.IsNullOrEmpty(messageActivity.Text))
                {
                    var results = await _smartTalk.GetAnswers(messageActivity.Text.Trim()).ConfigureAwait(false);

                    if (!_options.UseChatQuerySignal || results.IsChatQuery)
                    {
                        var response = results?.ScenarioList?.FirstOrDefault()?.Responses?.FirstOrDefault();

                        if (string.IsNullOrEmpty(response) && !string.IsNullOrEmpty(_options.DefaultMessage))
                        {
                            response = _options.DefaultMessage;
                        }

                        if (!string.IsNullOrEmpty(response))
                        {
                            await context.SendActivity(response);

                            if (_options.EndActivityRoutingOnAnswer)
                                //Question is answered, don't keep routing
                                return;
                        }
                    }
                }
            }

            await next().ConfigureAwait(false);
        }
    }
}
