// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.QnA
{
    public class QnAMakerMiddleware : IMiddleware
    {
        public const string QnAMakerMiddlewareName = "QnAMakerMiddleware";
        public const string QnAMakerResultKey = "QnAMakerResult";
        public const string QnAMakerTraceType = "https://www.qnamaker.ai/schemas/trace";
        public const string QnAMakerTraceLabel = "QnAMaker Trace";
        public const string Obfuscated = "****";
        private readonly QnAMaker _qnaMaker;
        private readonly QnAMakerMiddlewareOptions _options;

        public QnAMakerMiddleware(QnAMakerMiddlewareOptions options, HttpClient httpClient = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _qnaMaker = new QnAMaker(options, httpClient);
        }

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var messageActivity = context.Activity.AsMessageActivity();
                if (!string.IsNullOrEmpty(messageActivity.Text))
                {
                    var results = await _qnaMaker.GetAnswers(messageActivity.Text.Trim()).ConfigureAwait(false);
                    var traceInfo = new QnAMakerTraceInfo
                    {
                        QueryResults = results,
                        QnAMakerOptions = RemoveSensitiveData(_options)
                    };
                    var traceActivity = Activity.CreateTraceActivity(QnAMakerMiddlewareName, QnAMakerTraceType, traceInfo, QnAMakerTraceLabel);
                    await context.SendActivity(traceActivity).ConfigureAwait(false);

                    if (results.Any())
                    {
                        if (!string.IsNullOrEmpty(_options.DefaultAnswerPrefixMessage))
                            await context.SendActivity(_options.DefaultAnswerPrefixMessage);

                        await context.SendActivity(results.First().Answer);

                        if (_options.EndActivityRoutingOnAnswer)
                            //Question is answered, don't keep routing
                            return;
                    }
                }
            }

            await next().ConfigureAwait(false);
        }

        public static QnAMakerOptions RemoveSensitiveData(QnAMakerOptions options)
        {
            return new QnAMakerOptions
            {
                ScoreThreshold = options.ScoreThreshold,
                Top = options.Top,
                StrictFilters = options.StrictFilters,
                MetadataBoost = options.MetadataBoost,
                SubscriptionKey = Obfuscated,
                KnowledgeBaseId = Obfuscated
            };
        }
    }
}