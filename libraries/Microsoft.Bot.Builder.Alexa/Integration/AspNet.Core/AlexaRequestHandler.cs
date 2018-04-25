// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private bool _validateIncomingAlexaRequests;

        public AlexaRequestHandler(AlexaAdapter alexaAdapter, bool validateIncomingAlexaRequests)
        {
            _alexaAdapter = alexaAdapter;
            _validateIncomingAlexaRequests = validateIncomingAlexaRequests;
        }
       
        protected async Task ProcessMessageRequestAsync(HttpRequest request, AlexaAdapter alexaAdapter, Func<ITurnContext, Task> botCallbackHandler)
        {
            AlexaRequestBody skillRequest;
            
            using (var bodyReader = new JsonTextReader(new StreamReader(request.Body, Encoding.UTF8)))
            {
                skillRequest = AlexaBotMessageSerializer.Deserialize<AlexaRequestBody>(bodyReader);
            }

            if (skillRequest.Version != "1.0")
                throw new Exception($"Unexpected version of '{skillRequest.Version}' received.");

            if (_validateIncomingAlexaRequests)
            {
                var requestValidationHelper = new AlexaRequestValidationHelper();
                await requestValidationHelper.ValidateRequestSecurity(request, skillRequest);
            }

            await alexaAdapter.ProcessActivity(
                    skillRequest,
                    botCallbackHandler);
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
                await ProcessMessageRequestAsync(
                    request,
                    _alexaAdapter,
                    context =>
                    {
                        var bot = httpContext.RequestServices.GetRequiredService<IBot>();

                        return bot.OnTurn(context);
                    });

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}