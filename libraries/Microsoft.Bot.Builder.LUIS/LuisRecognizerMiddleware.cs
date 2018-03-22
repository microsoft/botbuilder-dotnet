// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Cognitive.LUIS;

namespace Microsoft.Bot.Builder.LUIS
{
    /// <summary>
    /// A Middleware for running the Luis recognizer
    /// This could eventually be generalized and moved to the core Bot Builder library
    /// in order to support multiple recognizers
    /// </summary>
    public class LuisRecognizerMiddleware : IMiddleware
    {
        public const string LuisRecognizerResultKey = "LuisRecognizerResult";
        private readonly IRecognizer _luisRecognizer;

        public LuisRecognizerMiddleware(ILuisModel luisModel, ILuisRecognizerOptions luisRecognizerOptions = null, ILuisOptions luisOptions = null)
        {
            if(luisModel == null)
                throw new ArgumentNullException(nameof(luisModel));

            _luisRecognizer = new LuisRecognizer(luisModel, luisRecognizerOptions, luisOptions);
        }

        public async Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            BotAssert.ContextNotNull(context);

            if (context.Activity.Type == ActivityTypes.Message)
            {
                var utterance = context.Activity.AsMessageActivity().Text;
                var result = await _luisRecognizer.Recognize(utterance, CancellationToken.None).ConfigureAwait(false);
                context.Services.Add(LuisRecognizerResultKey, result);
            }
            await next().ConfigureAwait(false);
        }
    }
}
