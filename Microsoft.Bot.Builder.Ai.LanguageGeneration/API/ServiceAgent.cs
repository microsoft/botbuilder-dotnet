using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    public class ServiceAgent : IServiceAgent
    {
        private LGServiceAgent _serviceAgent;
        public ServiceAgent(string endPoint)
        {
            _serviceAgent = new LGServiceAgent
            {
                Endpoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint))
            };
        }

        public string Generate(LGRequest request)
        {
            var response = _serviceAgent.Generate(request);
            return response.DisplayText;
        }

        public async Task<string> GenerateAsync(LGRequest request)
        {
            var response = await _serviceAgent.GenerateAsync(request).ConfigureAwait(false);
            return response.DisplayText;
        }
    }
}
