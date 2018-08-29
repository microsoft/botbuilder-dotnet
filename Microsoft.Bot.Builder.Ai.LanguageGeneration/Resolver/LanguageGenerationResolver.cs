using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using LanguageGeneration.V2;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Engine;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver
{
    public class LanguageGenerationResolver
    {
        private readonly LanguageGenerationApplication _languageGenerationApplication;
        private readonly IResolverPipeline _resolverPipeline;

        public LanguageGenerationResolver(LanguageGenerationApplication languageGenerationApplication, LanguageGenerationOptions lgOptions = null, IServiceAgent serviceAgent = null)
        {
            _languageGenerationApplication = languageGenerationApplication ?? throw new ArgumentNullException(nameof(_languageGenerationApplication));
            var resolverPipelineFactory = new ResolverPipelineFactory();
            _resolverPipeline = resolverPipelineFactory.CreateResolverPipeline(_languageGenerationApplication.AzureRegion, serviceAgent);
        }

        public async Task ResolveAsync(Activity activity, IDictionary<string, object> entities)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            await _resolverPipeline.ExecuteAsync(activity, entities).ConfigureAwait(false);
        }
    }
}
