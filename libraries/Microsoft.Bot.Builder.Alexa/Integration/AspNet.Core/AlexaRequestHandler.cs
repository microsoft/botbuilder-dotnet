// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Alexa.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Alexa.Integration.AspNet.Core
{
    public class AlexaRequestHandler
    {
        public static readonly JsonSerializer AlexaBotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        });

        private readonly AlexaAdapter _alexaAdapter;
        private readonly AlexaOptions _alexaOptions;

        public AlexaRequestHandler(AlexaAdapter alexaAdapter, AlexaOptions alexaOptions)
        {
            _alexaAdapter = alexaAdapter;
            _alexaOptions = alexaOptions;
        }
       
        protected async Task<AlexaResponseBody> ProcessMessageRequestAsync(HttpRequest request, AlexaAdapter alexaAdapter, Func<ITurnContext, Task> botCallbackHandler)
        {
            AlexaRequestBody skillRequest;

            var memoryStream = new MemoryStream();
            request.Body.CopyTo(memoryStream);
            var requestBytes = memoryStream.ToArray();
            memoryStream.Position = 0;

            using (var bodyReader = new JsonTextReader(new StreamReader(memoryStream, Encoding.UTF8)))
            {
                skillRequest = AlexaBotMessageSerializer.Deserialize<AlexaRequestBody>(bodyReader);
            }

            if (skillRequest.Version != "1.0")
                throw new Exception($"Unexpected version of '{skillRequest.Version}' received.");

            if (_alexaOptions.ValidateIncomingAlexaRequests)
            {
                request.Headers.TryGetValue("SignatureCertChainUrl", out var certUrls);
                request.Headers.TryGetValue("Signature", out var signatures);
                var certChainUrl = certUrls.FirstOrDefault();
                var signature = signatures.FirstOrDefault();
                await AlexaValidateRequestSecurityHelper.Validate(skillRequest, requestBytes, certChainUrl, signature);
            }

            var alexaResponseBody = await alexaAdapter.ProcessActivity(
                    skillRequest,
                    _alexaOptions,
                    botCallbackHandler);

            return alexaResponseBody;
        }

        public async Task HandleAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;

            if (request.Method != HttpMethods.Post)
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                return;
            }

            if (request.ContentLength == 0)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeaderValue)
                || mediaTypeHeaderValue.MediaType != "application/json")
            {
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return;
            }

            try
            {
                var alexaResponseBody = await ProcessMessageRequestAsync(
                    request,
                    _alexaAdapter,
                    context =>
                    {
                        var bot = httpContext.RequestServices.GetRequiredService<IBot>();
                        return bot.OnTurn(context);
                    });
                
                var alexaResponseBodyJson = JsonConvert.SerializeObject(alexaResponseBody, Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
                    });

                await response.WriteAsync(alexaResponseBodyJson);
            }
            catch (UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}