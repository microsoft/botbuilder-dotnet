using System.Collections.Generic;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests.Mocks
{
    public class ServiceAgentMock : IServiceAgent
    {
        private IDictionary<string, string> _resolutionsDictionary;
        public ServiceAgentMock(IDictionary<string, string> resolutionsDictionary)
        {
            _resolutionsDictionary = resolutionsDictionary;
        }
        public string Generate(LGRequest request) => _resolutionsDictionary[request.TemplateId];

        public Task<string> GenerateAsync(LGRequest request)
        {
            var response = _resolutionsDictionary[request.TemplateId];
            ResolveEntities(ref response, request);
            return Task.FromResult(response);
        }

        private void ResolveEntities(ref string uttrance, LGRequest request)
        {
            foreach (var slot in request.Slots)
            {
                if (slot.Key != "GetStateName")
                {
                    if (slot.Value.StringValues != null)
                    {
                        uttrance = uttrance.Replace(slot.Key, slot.Value.StringValues[0]);
                    }

                    if (slot.Value.IntValues != null)
                    {
                        uttrance = uttrance.Replace(slot.Key, slot.Value.IntValues[0].ToString());
                    }

                    if (slot.Value.FloatValues != null)
                    {
                        uttrance = uttrance.Replace(slot.Key, slot.Value.FloatValues[0].ToString());
                    }

                    NormalizeUttrance(ref uttrance);
                }
            }
        }

        private void NormalizeUttrance(ref string uttrance) => uttrance = uttrance.Replace("{", "").Replace("}", "");
    }
}
