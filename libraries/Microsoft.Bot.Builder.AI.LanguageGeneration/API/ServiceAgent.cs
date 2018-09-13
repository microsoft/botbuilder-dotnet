using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API.Model;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    public class ServiceAgent : IServiceAgent
    {
        private LGServiceAgent _serviceAgent;
        private readonly string _tokenGenerationEndpoint;
        private readonly string _subscriptionKey;
        private HttpClient _tokenGenerationHttpClient;
        private static readonly HttpClient DefaultTokenGenerationHttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };


        public ServiceAgent(string endPoint, string subscriptionKey, string tokenGenerationEndpoint = "", HttpClient tokenGenerationHttpClient = null)
        {
            if (!Guid.TryParse(subscriptionKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{subscriptionKey}\" is not a valid Language generation subscription key.");
            }

            if (string.IsNullOrWhiteSpace(tokenGenerationEndpoint))
            {
                _tokenGenerationEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
            }
            else
            {
                _tokenGenerationEndpoint = tokenGenerationEndpoint;
            }

            _serviceAgent = new LGServiceAgent
            {
                Endpoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint))
            };
            var settings = new ServiceLGSetting(_serviceAgent.Endpoint);
            var isOk = _serviceAgent.Initialize(settings);
            _tokenGenerationHttpClient = tokenGenerationHttpClient ?? DefaultTokenGenerationHttpClient;
            _subscriptionKey = subscriptionKey;
        }

        public async Task<string> GenerateAsync(LGRequest request)
        {
            var tokenGenerationRequestModel = new TokenGenerationRequestModel()
            {
                SubscriptionKey = _subscriptionKey
            };
            using (var tokenGenerationRequest = GetRequestMessage(tokenGenerationRequestModel))
            {
                using (var tokenGenerationResponse = await _tokenGenerationHttpClient.SendAsync(tokenGenerationRequest).ConfigureAwait(false))
                {
                    if (tokenGenerationResponse.IsSuccessStatusCode)
                    {
                        var responseBody = await tokenGenerationResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var additionalHeaders = new WebHeaderCollection
                        {
                            { "Authorization", "Bearer " + responseBody }
                        };
                        var response = await _serviceAgent.GenerateAsync(request, additionalHeaders).ConfigureAwait(false);
                        return response.DisplayText;
                    }
                    else
                    {
                        throw new Exception("Authorization token failed with the error code " + tokenGenerationResponse.StatusCode);
                    }
                }
            }


        }

        private HttpRequestMessage GetRequestMessage(TokenGenerationRequestModel tokenGenerationRequestModel)
        {
            var requestUri = new Uri(_tokenGenerationEndpoint);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Ocp-Apim-Subscription-Key", tokenGenerationRequestModel.SubscriptionKey);
            return request;
        }
    }
}
