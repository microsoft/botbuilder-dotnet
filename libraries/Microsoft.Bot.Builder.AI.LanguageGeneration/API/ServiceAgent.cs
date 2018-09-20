using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API.Model;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    /// <summary>
    /// Service agent that's used to communicate requests/responses with language generation runtime service.
    /// </summary>
    public class ServiceAgent : IServiceAgent
    {
        private readonly LGServiceAgent _serviceAgent;
        private readonly string _tokenGenerationEndpoint;
        private readonly string _subscriptionKey;
        private readonly HttpClient _tokenGenerationHttpClient;
        private static readonly HttpClient DefaultTokenGenerationHttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };

        /// <summary>
        /// Construct a new instance of <see cref="ServiceAgent"/> 
        /// </summary>
        /// <param name="endPoint">Service endpoint.</param>
        /// <param name="subscriptionKey">Subscription key.</param>
        /// <param name="tokenGenerationEndpoint">Token generation endpoint, used to generate authorization tokens that will be used to authorize communication with language generation service.</param>
        /// <param name="tokenGenerationHttpClient">Token generation http client.</param>
        public ServiceAgent(string endPoint, string subscriptionKey, string tokenGenerationEndpoint = "", HttpClient tokenGenerationHttpClient = null)
        {
            if (!Guid.TryParse(subscriptionKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{subscriptionKey}\" is not a valid Language generation subscription key.");
            }

            // if the user didn't pass an endpoint, the default endpoint is used.
            if (string.IsNullOrWhiteSpace(tokenGenerationEndpoint))
            {
                _tokenGenerationEndpoint = Constants.DefaultTokenGenerationEndpoint;
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
            if (!isOk)
            {
                throw new Exception("Language generation service initialization failed.");
            }
            _tokenGenerationHttpClient = tokenGenerationHttpClient ?? DefaultTokenGenerationHttpClient;
            _subscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Generate async is used to generate responses (aka, resolve user referenced templates) using language generation cognitive service.
        /// </summary>
        /// <param name="request">A <see cref="LGRequest"/> object containing the referenced template and slots used from the language generation runtime.</param>
        /// <returns>A <see cref="Task"/> containing the generation result.</returns>
        public async Task<string> GenerateAsync(LGRequest request)
        {
            var tokenGenerationRequestModel = new TokenGenerationRequestModel()
            {
                SubscriptionKey = _subscriptionKey
            };

            // first, a token gets generated using subscription key and issueToken api, this token will be used to authorize communication with language generation runtime api,
            // second a request will be initiated with language generation runtime api.
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
                        if (response != null)
                        {
                            return response.DisplayText;
                        }
                        else
                        {
                            throw new Exception("Failed to communicate with language generation runtime api.");
                        }
                    }
                    else
                    {
                        throw new Exception("Authorization token failed with the error code " + tokenGenerationResponse.StatusCode);
                    }
                }
            }


        }

        /// <summary>
        /// Prepare request message for token generation.
        /// </summary>
        /// <param name="tokenGenerationRequestModel">A <see cref="TokenGenerationRequestModel"/> object.</param>
        /// <returns></returns>
        private HttpRequestMessage GetRequestMessage(TokenGenerationRequestModel tokenGenerationRequestModel)
        {
            var requestUri = new Uri(_tokenGenerationEndpoint);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Ocp-Apim-Subscription-Key", tokenGenerationRequestModel.SubscriptionKey);
            return request;
        }
    }
}
