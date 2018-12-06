using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Engine;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver
{
    /// <summary>
    /// Language generation resolver engine, used for resolving templates references by bot developer to generate responses in a more human-like manner. 
    /// </summary>
    public class LanguageGenerationResolver
    {
        private readonly LanguageGenerationApplication _languageGenerationApplication;
        private readonly LanguageGenerationOptions _languageGenerationOptions;

        private readonly IResolverPipeline _resolverPipeline;

        /// <summary>
        /// Construct a new <see cref="LanguageGenerationResolver"/> object.
        /// </summary>
        /// <param name="languageGenerationApplication">Language generation application</param>
        /// <param name="languageGenerationOptions">Language generation options</param>
        /// <param name="serviceAgent"></param>
        public LanguageGenerationResolver(LanguageGenerationApplication languageGenerationApplication, LanguageGenerationOptions languageGenerationOptions = null, IServiceAgent serviceAgent = null)
        {
            _languageGenerationApplication = languageGenerationApplication ?? throw new ArgumentNullException(nameof(languageGenerationApplication));
            _languageGenerationOptions = languageGenerationOptions ?? new LanguageGenerationOptions();

            var resolverPipelineFactory = new ResolverPipelineFactory();

            if (serviceAgent != null)
            {
                _resolverPipeline = resolverPipelineFactory.CreateResolverPipeline(_languageGenerationApplication, serviceAgent);
            } 
            else
            {
                var endpointProvider = new EndpointProvider(_languageGenerationApplication, _languageGenerationOptions);
                _resolverPipeline = resolverPipelineFactory.CreateResolverPipeline(_languageGenerationApplication, endpointProvider);
            }
        }

        /// <summary>
        /// Resolve outgoing activity to generate a response based on the template references in <see cref="Activity"/>.
        /// Example : 
        /// Activity outgoingActivity.Text = "[sayHello]"
        /// await resolver.ResolveAsync(outgoingActivity).configureAwait(false);
        /// await SendActivityAsync(outgoingActivity).configureAwait(false);
        /// the user sees something like "hello there" or any other resolution that was previously configured for template [sayHello].
        /// </summary>
        /// <param name="activity">activity object that contains template references.</param>
        /// <param name="entities">entity list that will be used for resolving templates.</param>
        /// <returns></returns>
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
