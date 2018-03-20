// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.Connector.EchoBot.Controllers
{
    [Route("api/messages")]
    public class MessagesController : Controller
    {
        private readonly SimpleCredentialProvider credentials;

        public MessagesController(IConfiguration configuration)
        {
            this.credentials = new ConfigurationCredentialProvider(configuration);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            // Validate Authorization Header. Should be a jwt token. 
            var authHeader = this.Request.Headers["Authorization"].SingleOrDefault();

            try
            {
                await JwtTokenValidation.AuthenticateRequest(activity, authHeader, this.credentials);
            }
            catch (UnauthorizedAccessException)
            {                    
                return this.Unauthorized();
            }

            // On message activity, reply with the same text
            if (activity.Type == ActivityTypes.Message)
            {
                var reply = activity.CreateReply($"You said: {activity.Text}");

                // Reply to Activity using Connector
                var connector = new ConnectorClient(
                    new Uri(activity.ServiceUrl, UriKind.Absolute),
                    new MicrosoftAppCredentials(this.credentials.AppId, this.credentials.Password));

                await connector.Conversations.ReplyToActivityAsync(reply);
            }

            return this.Ok();
        }
    }
}
