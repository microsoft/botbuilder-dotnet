// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using Microsoft.Bot.Builder.Alexa.Helpers;

namespace Microsoft.Bot.Builder.Alexa.Integration.AspNet.WebApi
{
    internal sealed class AlexaRequestHandler : HttpMessageHandler
    {
        public static readonly MediaTypeFormatter[] AlexaMessageMediaTypeFormatters = {
            new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                }
            }
        };

        private readonly AlexaAdapter _alexaAdapter;
        private readonly bool _validateIncomingAlexaRequests;

        public AlexaRequestHandler(AlexaAdapter alexaAdapter, bool validateIncomingAlexaRequests = true)
        {
            _alexaAdapter = alexaAdapter;
            _validateIncomingAlexaRequests = validateIncomingAlexaRequests;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Post)
            {
                return request.CreateResponse(HttpStatusCode.MethodNotAllowed);
            }

            var requestContentHeaders = request.Content.Headers;

            if (requestContentHeaders.ContentLength == 0)
            {
                return request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request body should not be empty.");
            }

            try
            {
                return await ProcessMessageRequestAsync(
                    request,
                    _alexaAdapter,
                    context =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        IBot bot;

                        try
                        {
                            bot = (IBot)request.GetDependencyScope().GetService(typeof(IBot));
                        }
                        catch
                        {
                            bot = null;
                        }

                        if (bot == null)
                        {
                            throw new InvalidOperationException($"Did not find an {typeof(IBot).Name} service via the dependency resolver. Please make sure you have registered your bot with your dependency injection container.");
                        }

                        return bot.OnTurn(context);
                    },
                    cancellationToken);
            }
            catch (UnauthorizedAccessException e)
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, e.Message);
            }
            catch (InvalidOperationException e)
            {
                return request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message);
            }
        }

        public async Task<HttpResponseMessage> ProcessMessageRequestAsync(HttpRequestMessage request, AlexaAdapter alexaAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken)
        {
            AlexaRequestBody skillRequest;
            byte[] requestByteArray;

            try
            {
                requestByteArray = await request.Content.ReadAsByteArrayAsync();
                skillRequest = await request.Content.ReadAsAsync<AlexaRequestBody>(AlexaMessageMediaTypeFormatters, cancellationToken);
            }
            catch (Exception)
            {
                throw new JsonSerializationException("Invalid JSON received");
            }

            if (skillRequest.Version != "1.0")
                throw new Exception($"Unexpected version of '{skillRequest.Version}' received.");

            if (_validateIncomingAlexaRequests)
            {
                request.Headers.TryGetValues("SignatureCertChainUrl", out var certUrls);
                request.Headers.TryGetValues("Signature", out var signatures);
                var certChainUrl = certUrls.FirstOrDefault();
                var signature = signatures.FirstOrDefault();
                await AlexaValidateRequestSecurityHelper.Validate(skillRequest, requestByteArray, certChainUrl, signature);
            }

            var alexaResponseBody = await alexaAdapter.ProcessActivity(
                skillRequest,
                botCallbackHandler);

            var contractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };

            var alexaResponseBodyJson = JsonConvert.SerializeObject(alexaResponseBody, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = contractResolver
                });

            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(alexaResponseBodyJson);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return response;
        }
    }
}