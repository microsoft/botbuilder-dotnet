using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ResolverPipeline : IResolverPipeline
    {
        private readonly ISlotBuilder _slotuilder;
        private readonly IRequestBuilder _requestBuilder;
        private readonly IResponseGenerator _responseGenerator;
        private readonly IActivityModifier _activityModifier;

        public ResolverPipeline(ISlotBuilder slotBuilder, IRequestBuilder requestBuilder, IResponseGenerator responseGenerator, IActivityModifier activityModifier)
        {
            _slotuilder = slotBuilder ?? throw new ArgumentNullException(nameof(slotBuilder));
            _requestBuilder = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
            _activityModifier = activityModifier ?? throw new ArgumentNullException(nameof(activityModifier));
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

            var slots = await _slotuilder.BuildSlotsAsync(activity, entities).ConfigureAwait(false);
            var request = await _requestBuilder.BuildRequestAsync(slots).ConfigureAwait(false);
            var response = await _responseGenerator.GenerateResponseAsync(request).ConfigureAwait(false);

            await _activityModifier.ModifyActivityAsync(response).ConfigureAwait(false);
        }
    }
}
