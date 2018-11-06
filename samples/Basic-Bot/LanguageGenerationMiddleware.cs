// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;

namespace Microsoft.BotBuilderSamples
{
    public class LanguageGenerationMiddleware : IMiddleware
    {
        private readonly LanguageGenerationResolver _languageGenerationResolver;
        private readonly IStatePropertyAccessor<Dictionary<string, object>> _entitiesStateAccessor;

        public LanguageGenerationMiddleware(LanguageGenerationResolver languageGenerationResolver, IStatePropertyAccessor<Dictionary<string, object>> entitiesStateAccessor)
        {
            _languageGenerationResolver = languageGenerationResolver;
            _entitiesStateAccessor = entitiesStateAccessor;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate nextDelegate, CancellationToken cancellationToken = default(CancellationToken))
        {
            turnContext.OnSendActivities(async (context, activities, next) =>
            {
                var entities = await _entitiesStateAccessor.GetAsync(turnContext, () => new Dictionary<string, object>(), cancellationToken);

                foreach (var activity in activities)
                {
                    await _languageGenerationResolver.ResolveAsync(activity, entities);
                }

                return await next();
            });

            await nextDelegate(cancellationToken);
        }
    }
}
