using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ResponseGenerator : IResponseGenerator
    {
        public async Task<ICompositeResponse> GenerateResponseAsync(ICompositeRequest compositeRequest, ServiceAgent serviceAgent)
        {
            var compositeResponse = new CompositeResponse();
            if (compositeRequest == null)
            {
                throw new ArgumentNullException(nameof(compositeRequest));
            }

            foreach (var request in compositeRequest.Requests)
            {
                var response = await serviceAgent.GenerateAsync(request.Value).ConfigureAwait(false);
                compositeResponse.TemplateResolutions.Add(request.Key, response.DisplayText);
            }

            return compositeResponse;
        }
    }
}
