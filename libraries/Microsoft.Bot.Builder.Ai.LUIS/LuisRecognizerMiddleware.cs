// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// A Middleware for running the Luis recognizer.
    /// This could eventually be generalized and moved to the core Bot Builder library
    /// in order to support multiple recognizers.
    /// </summary>
    public class LuisRecognizerMiddleware : IMiddleware
    {
        /// <summary>
        /// The service key to use to retrieve recognition results.
        /// </summary>
        public const string LuisRecognizerResultKey = "LuisRecognizerResult";

        /// <summary>
        /// The value type for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceType = "https://www.luis.ai/schemas/trace";

        /// <summary>
        /// The context label for a LUIS trace activity.
        /// </summary>
        public const string LuisTraceLabel = "Luis Trace";

        /// <summary>
        /// A string used to obfuscate the LUIS subscription key.
        /// </summary>
        public const string Obfuscated = "****";

        private readonly IRecognizer _luisRecognizer;
        private readonly ILuisModel _luisModel;
        private readonly ILuisOptions _luisOptions;

        /// <summary>
        /// Creates a new <see cref="LuisRecognizerMiddleware"/> object.
        /// </summary>
        /// <param name="luisModel">The LUIS model to use to recognize text.</param>
        /// <param name="luisRecognizerOptions">The LUIS recognizer options to use.</param>
        /// <param name="options">The LUIS request options to use.</param>
        public LuisRecognizerMiddleware(ILuisModel luisModel, ILuisRecognizerOptions luisRecognizerOptions = null, ILuisOptions luisOptions = null)
        {
            _luisModel = luisModel ?? throw new ArgumentNullException(nameof(luisModel));
            _luisOptions = luisOptions;
            _luisRecognizer = new LuisRecognizer(luisModel, luisRecognizerOptions, luisOptions);
        }

        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            BotAssert.ContextNotNull(context);

            if (context.Activity.Type == ActivityTypes.Message)
            {
                var utterance = context.Activity.AsMessageActivity().Text;
                var result = await _luisRecognizer.Recognize(utterance, CancellationToken.None).ConfigureAwait(false);
                context.Services.Add(LuisRecognizerResultKey, result);

                var traceInfo = new LuisTraceInfo
                {
                    RecognizerResult = result,
                    LuisModel = RemoveSensitiveData(_luisModel),
                    LuisOptions = _luisOptions,
                    LuisResult = (LuisResult) result.Properties["luisResult"]
                };
                var traceActivity = Activity.CreateTraceActivity("LuisRecognizerMiddleware", LuisTraceType, traceInfo, LuisTraceLabel);
                await context.SendActivity(traceActivity).ConfigureAwait(false);
            }
            await next().ConfigureAwait(false);
        }

        /// <summary>
        /// Removes sensitive information from a LUIS model.
        /// </summary>
        /// <param name="luisModel">The model.</param>
        /// <returns>A new model with the sensitive information removed.</returns>
        public static ILuisModel RemoveSensitiveData(ILuisModel luisModel)
        {
            return new LuisApplication(luisModel.ModelID, Obfuscated, luisModel.UriBase, luisModel.ApiVersion);
        }
    }
}