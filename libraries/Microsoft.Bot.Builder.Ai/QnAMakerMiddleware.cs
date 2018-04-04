// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai
{
    public class QnAMakerMiddleware : IMiddleware
    {
        private readonly QnAMaker _qnaMaker;
        private readonly QnAMakerMiddlewareOptions _options;
        private bool _isLastMiddleware;

        public QnAMakerMiddleware(QnAMakerMiddlewareOptions options, HttpClient httpClient = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _qnaMaker = new QnAMaker(options, httpClient);
        }

        public async Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var messageActivity = context.Activity.AsMessageActivity();
                if (!string.IsNullOrEmpty(messageActivity.Text))
                {
                    var results = await _qnaMaker.GetAnswers(messageActivity.Text.Trim()).ConfigureAwait(false);
                    if (results.Any())
                    {
                        if (!string.IsNullOrEmpty(_options.DefaultAnswerPrefixMessage))
                        {
                            if (_isLastMiddleware)
                                await context.SendActivity(_options.DefaultAnswerPrefixMessage);
                            else
                                messageActivity.Text = _options.DefaultAnswerPrefixMessage;
                        }
                        if (_isLastMiddleware)
                            await context.SendActivity(results.First().Answer);
                        else
                            messageActivity.Text = results.First().Answer;
                    }
                    else {
                        await next().ConfigureAwait(false);
                        return;
                    }
                }
            }
            if(!_isLastMiddleware)
                await next().ConfigureAwait(false);
        }

        /// <summary>
        /// Change Middleware Status to be the last Middleware
        /// </summary>
        /// <param name="last">boolean true of this middleware is the last middleware.</param>
        public void SetIsMiddlewareLast(bool last)
        {
            _isLastMiddleware = last;
        }
    }
}
