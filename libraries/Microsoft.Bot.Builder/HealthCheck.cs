// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    internal class HealthCheck
    {
        public static HealthCheckResponse CreateHealthCheckResponse(IConnectorClient connector)
        {
            // A derived class may override this, however, the default is that the bot is healthy given we have got to here.
            var healthResults = new HealthResults { Success = true };

            if (connector != null)
            {
                try
                {
                    // This is a mock secure SendToConversation to grab the exact HTTP headers.
                    // If you have no appId and no secret this code will run but not produce an Authorization header.
                    var captureHandler = new CaptureRequestHandler();
                    var client = new ConnectorClient(connector.BaseUri, connector.Credentials, captureHandler);
                    var activity = new Activity { Type = ActivityTypes.Message, Conversation = new ConversationAccount { Id = "capture" } };
                    client.Conversations.SendToConversation(activity);
                    var headers = captureHandler.Request.Headers;
                    healthResults.Authorization = headers.Authorization?.ToString();
                    healthResults.UserAgent = headers.UserAgent?.ToString();
                }
                catch (Exception)
                {
                    // This exception happens when you have a valid appId but invalid or blank secret.

                    // No callbacks will be possible, although the bot maybe healthy in other respects.
                }
            }

            var successMessage = "Health check succeeded.";
            healthResults.Messages = healthResults.Authorization != null ? new[] { successMessage } : new[] { successMessage, "Callbacks are not authorized." };

            return new HealthCheckResponse { HealthResults = healthResults };
        }

        private class CaptureRequestHandler : HttpClientHandler
        {
            public HttpRequestMessage Request { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                return Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{ \"id\": \"1234\"}") });
            }
        }
    }
}
