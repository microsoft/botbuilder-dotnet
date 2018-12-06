using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The main language generation resolver pipeline.
    /// </summary>
    internal class ResolverPipeline : IResolverPipeline
    {
        private readonly ISlotBuilder _slotuilder;
        private readonly IRequestBuilder _requestBuilder;
        private readonly IResponseGenerator _responseGenerator;
        private readonly IActivityModifier _activityModifier;
        private readonly IServiceAgent _serviceAgent;

        /// <summary>
        /// Constructs a new instance of <see cref="ResolverPipeline"/> using the main pipeline processors.
        /// </summary>
        /// <param name="slotBuilder">A <see cref="ISlotBuilder"/> object.</param>
        /// <param name="requestBuilder">A <see cref="IRequestBuilder"/> object.</param>
        /// <param name="responseGenerator">A <see cref="IResponseGenerator"/> object.</param>
        /// <param name="activityModifier">A <see cref="IActivityModifier"/> object.</param>
        /// <param name="serviceAgent">A <see cref="IServiceAgent"/> object.</param>
        public ResolverPipeline(ISlotBuilder slotBuilder, IRequestBuilder requestBuilder, IResponseGenerator responseGenerator, IActivityModifier activityModifier, IServiceAgent serviceAgent)
        {
            _slotuilder = slotBuilder ?? throw new ArgumentNullException(nameof(slotBuilder));
            _requestBuilder = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
            _activityModifier = activityModifier ?? throw new ArgumentNullException(nameof(activityModifier));
            _serviceAgent = serviceAgent ?? throw new ArgumentNullException(nameof(serviceAgent));
        }

        /// <summary>
        /// The entry point for the pipeline, that executes all the necessary language generation logic.
        /// </summary>
        /// <param name="activity">A <see cref="Activity"/> object.</param>
        /// <param name="entities">A <see cref="IDictionary{string, object}"/> that contains entities/slots used to resolve referenced templates.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task ExecuteAsync(Activity activity, IDictionary<string, object> entities)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            // inistanciate pipeline processors.
            var slots = _slotuilder.BuildSlots(activity, entities);
            var request = _requestBuilder.BuildRequest(slots);
            var response = await _responseGenerator.GenerateResponseAsync(request, _serviceAgent).ConfigureAwait(false);
            _activityModifier.ModifyActivity(activity, response);
        }
    }
}
