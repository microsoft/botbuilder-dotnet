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
        public string Generate(LGRequest request)
        {
            return _resolutionsDictionary[request.Slots["GetStateName"].StringValues[0]];
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<string> GenerateAsync(LGRequest request)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var response = _resolutionsDictionary[request.Slots["GetStateName"].StringValues[0]];
            ResolveEntities(ref response, request);
            return response;
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
