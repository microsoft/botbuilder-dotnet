// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai
{
    public class QnAMakerMiddleware : IMiddleware
    {
        private readonly QnAMaker _qnaMaker;
        private readonly QnAMakerMiddlewareOptions _options;

        public QnAMakerMiddleware(QnAMakerMiddlewareOptions options, HttpClient httpClient = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _qnaMaker = new QnAMaker(options, httpClient);
        }

        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                var messageActivity = context.Request.AsMessageActivity();
                if (!string.IsNullOrEmpty(messageActivity.Text))
                {
                    var results = await _qnaMaker.GetAnswers(messageActivity.Text.Trim()).ConfigureAwait(false);
                    if (results.Any())
                    {
                        if (!string.IsNullOrEmpty(_options.DefaultAnswerPrefixMessage))
                            context.Batch().Reply(_options.DefaultAnswerPrefixMessage);

                        context.Batch().Reply(results.First().Answer);

                        if (_options.EndActivityRoutingOnAnswer)
                            //Question is answered, don't keep routing
                            return;
                    }
                }
            }

            await next().ConfigureAwait(false);
        }
    }
}
