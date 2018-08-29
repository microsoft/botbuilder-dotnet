using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    public class ServiceAgentMock : IServiceAgent
    {
        private IDictionary<string, string> _resolutionsDictionary;
        public ServiceAgentMock(IDictionary<string, string> resolutionsDictionary)
        {
            _resolutionsDictionary = resolutionsDictionary;
        }
        public LGResponse Generate(LGRequest request)
        {
            var response = new LGResponseMock();
            response.DisplayText = _resolutionsDictionary[request.Slots["GetStateName"].StringValues[0]];
            return response;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<LGResponse> GenerateAsync(LGRequest request)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var response = new LGResponseMock();
            response.DisplayText = _resolutionsDictionary[request.Slots["GetStateName"].StringValues[0]];
            return response;
        }
    }
}
