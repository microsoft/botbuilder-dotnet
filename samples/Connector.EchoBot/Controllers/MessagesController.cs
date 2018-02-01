using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Connector.EchoBot.Controllers
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
            if (!await this.credentials.IsAuthenticationDisabledAsync())
            {
                // Validate Authorization Header
                var authHeader = this.Request.Headers["Authorization"].SingleOrDefault();
                bool isValidIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, this.credentials, activity.ServiceUrl);
                if (!isValidIdentity)
                    return this.Unauthorized();
                MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
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
