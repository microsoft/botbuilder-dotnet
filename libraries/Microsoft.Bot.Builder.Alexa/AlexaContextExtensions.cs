using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Alexa.Directives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Alexa
{
    public static class AlexaContextExtensions
    {
        public static Dictionary<string, string> AlexaSessionAttributes(this ITurnContext context)
        {
            return context.Services.Get<Dictionary<string, string>>("AlexaSessionAttributes");
        }

        public static List<IAlexaDirective> AlexaResponseDirectives(this ITurnContext context)
        {
            return context.Services.Get<List<IAlexaDirective>>("AlexaResponseDirectives");
        }

        public static async Task<HttpResponseMessage> AlexaSendProgressiveResponse(this ITurnContext context, string content)
        {
            var originalAlexaRequest = (AlexaRequestBody)context.Activity.ChannelData;

            var directive = new AlexaDirectiveRequest()
            {
                Header = new AlexaDirectiveRequest.DirectiveHeader()
                {
                    RequestId = originalAlexaRequest.Request.RequestId
                },
                Directive = new AlexaDirectiveRequest.DirectiveContent()
                {
                    Type = "VoicePlayer.Speak",
                    Speech = content
                }
            };

            var client = new HttpClient();

            var jsonRequest = JsonConvert.SerializeObject(directive,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            var directiveContent = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
            var directiveEndpoint = $"{originalAlexaRequest.Context.System.ApiEndpoint}/v1/directives";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", originalAlexaRequest.Context.System.ApiAccessToken);

            return await client.PostAsync(directiveEndpoint, directiveContent);
        }

        public static AlexaRequestBody GetAlexaRequestBody(this ITurnContext context)
        {
            try
            {
                return (AlexaRequestBody)context.Activity.ChannelData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool AlexaDeviceHasDisplay(this ITurnContext context)
        {
            var alexaRequest = (AlexaRequestBody)context.Activity.ChannelData;
            var hasDisplay =
                alexaRequest?.Context?.System?.Device?.SupportedInterfaces?.Interfaces?.Keys.Contains("Display");
            return hasDisplay.HasValue && hasDisplay.Value;
        }

        public static bool AlexaDeviceHasAudioPlayer(this ITurnContext context)
        {
            var alexaRequest = (AlexaRequestBody)context.Activity.ChannelData;
            var hasDisplay =
                alexaRequest?.Context?.System?.Device?.SupportedInterfaces?.Interfaces?.Keys.Contains("AudioPlayer");
            return hasDisplay.HasValue && hasDisplay.Value;
        }
    }
}
