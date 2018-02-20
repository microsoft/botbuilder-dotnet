// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai
{
    public class QnAMakerMiddleware : Middleware.IReceiveActivity
    {
        private readonly QnAMaker _qnaMaker;
        private readonly QnAMakerMiddlewareOptions _qnaMakerMiddlewareOptions;

        public QnAMakerMiddleware(QnAMakerOptions options, HttpClient httpClient, QnAMakerMiddlewareOptions middlewareOptions = null)
        {
            _qnaMaker = new QnAMaker(options, httpClient);
            _qnaMakerMiddlewareOptions = middlewareOptions ?? new QnAMakerMiddlewareOptions();
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                var messageActivity = context.Request.AsMessageActivity();
                if (!string.IsNullOrEmpty(messageActivity.Text))
                {
                    var results = await _qnaMaker.GetAnswers(messageActivity.Text.Trim()).ConfigureAwait(false);
                    if (results.Any())
                    {
                        if (!string.IsNullOrEmpty(_qnaMakerMiddlewareOptions.DefaultAnswerPrefixMessage))
                            context.Reply(_qnaMakerMiddlewareOptions.DefaultAnswerPrefixMessage);

                        context.Reply(results.First().Answer);

                        if (_qnaMakerMiddlewareOptions.EndActivityRoutingOnAnswer)
                            return;
                    }
                }
            }

            await next().ConfigureAwait(false);
        }
    }
}
