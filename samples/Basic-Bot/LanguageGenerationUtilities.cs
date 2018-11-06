// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;

namespace Microsoft.BotBuilderSamples
{
    public static class LanguageGenerationUtilities
    {
        /// <summary>
        /// Creates an instance of <see cref="LanguageGenerationResolver"/>.
        /// </summary>
        /// <param name="applicationId">Language generation application id.</param>
        /// <param name="endpointKey">The language generation service subscription key.</param>
        /// <param name="endpointRegion">The language generation region.</param>
        /// <returns>An instance of the language generation resolver.</returns>
        public static LanguageGenerationResolver CreateResolver(string applicationId, string endpointKey, string endpointRegion)
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                throw new ArgumentException("Application id can't be null or empty.", nameof(applicationId));
            }

            if (string.IsNullOrEmpty(endpointKey))
            {
                throw new ArgumentException("Endpoint subscription key can't be null or empty.", nameof(applicationId));
            }

            if (string.IsNullOrEmpty(endpointRegion))
            {
                // Set default region to westus
                endpointRegion = "westus";
            }

            var languageGenerationEndpoint = $"https://{endpointRegion}.cts.speech.microsoft.com/v1/lg";
            var tokenIssuingEndpoint = $"https://{endpointRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

            var application = new LanguageGenerationApplication(applicationId, endpointKey, languageGenerationEndpoint);
            var options = new LanguageGenerationOptions
            {
                TokenGenerationApiEndpoint = tokenIssuingEndpoint,
            };

            return new LanguageGenerationResolver(application, options);
        }

        /// <summary>
        /// Creates an instance of <see cref="LanguageGenerationMiddleware"/> to simplify and contain the resolution of the activities with the middleware
        /// </summary>
        /// <param name="applicationId">Language generation application id.</param>
        /// <param name="endpointKey">The language generation service subscription key.</param>
        /// <param name="endpointRegion">The language generation region.</param>
        /// <param name="entitiesStateAccessor">The user state accessor providing the entities preserved in the request.</param>
        /// <returns>An instance of the language generation middleware</returns>
        public static LanguageGenerationMiddleware CreateMiddleware(string applicationId, string endpointKey, string endpointRegion, IStatePropertyAccessor<Dictionary<string, object>> entitiesStateAccessor)
        {
            if (entitiesStateAccessor == null)
            {
                throw new ArgumentNullException(nameof(entitiesStateAccessor));
            }

            var resolver = CreateResolver(applicationId, endpointKey, endpointRegion);
            return new LanguageGenerationMiddleware(resolver, entitiesStateAccessor);
        }

        /// <summary>
        /// Appends the extracted entities to the user entity state for language generation resolver.
        /// </summary>
        /// <param name="entitiesStateAccessor">The user state accessor providing the entities preserved in the request.</param>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="entities">The extracted entities to append.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task UpdateEntitiesStateAsync(IStatePropertyAccessor<Dictionary<string, object>> entitiesStateAccessor, ITurnContext turnContext, Dictionary<string, object> entities, CancellationToken cancellationToken = default(CancellationToken))
        {
            var contextEntities = await entitiesStateAccessor.GetAsync(turnContext, () => new Dictionary<string, object>(), cancellationToken);

            foreach (var entityInfo in entities)
            {
                contextEntities[entityInfo.Key] = entityInfo.Value;
            }

            await entitiesStateAccessor.SetAsync(turnContext, contextEntities, cancellationToken);
        }
    }
}
