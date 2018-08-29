using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal class ResolverPipeline : IResolverPipeline
    {
        private readonly ISlotBuilder _slotuilder;
        private readonly IRequestBuilder _requestBuilder;
        private readonly IResponseGenerator _responseGenerator;
        private readonly IActivityModifier _activityModifier;
        private readonly IServiceAgent _serviceAgent;

        public ResolverPipeline(ISlotBuilder slotBuilder, IRequestBuilder requestBuilder, IResponseGenerator responseGenerator, IActivityModifier activityModifier, IServiceAgent serviceAgent)
        {
            _slotuilder = slotBuilder ?? throw new ArgumentNullException(nameof(slotBuilder));
            _requestBuilder = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
            _activityModifier = activityModifier ?? throw new ArgumentNullException(nameof(activityModifier));
            _serviceAgent = serviceAgent ?? throw new ArgumentNullException(nameof(serviceAgent));
        }

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

            var slots = _slotuilder.BuildSlots(activity, entities);
            var request = _requestBuilder.BuildRequest(slots);
            var response = await _responseGenerator.GenerateResponseAsync(request, _serviceAgent).ConfigureAwait(false);
            _activityModifier.ModifyActivity(activity, response);
        }
    }
}
