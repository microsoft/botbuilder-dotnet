using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal class ResponseGenerator : IResponseGenerator
    {
        public async Task<ICompositeResponse> GenerateResponseAsync(ICompositeRequest compositeRequest, IServiceAgent serviceAgent)
        {
            var compositeResponse = new CompositeResponse();
            if (compositeRequest == null)
            {
                throw new ArgumentNullException(nameof(compositeRequest));
            }

            foreach (var request in compositeRequest.Requests)
            {
                var response = await serviceAgent.GenerateAsync(request.Value).ConfigureAwait(false);
                compositeResponse.TemplateResolutions.Add(request.Key, "Hello");
            }

            return compositeResponse;
        }
    }
}
