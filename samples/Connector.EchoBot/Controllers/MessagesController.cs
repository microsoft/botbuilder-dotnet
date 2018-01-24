using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace Connector.EchoBot.Controllers
{
    [Route("api/messages")]
    public class MessagesController : Controller
    {
        private readonly MicrosoftAppCredentials credentials;

        public MessagesController(IConfiguration configuration)
        {
            this.credentials = new MicrosoftAppCredentials(
                configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
                configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            // Validate Authorization Header
            var authHeader = this.Request.Headers["Authorization"].SingleOrDefault();
            bool isValidIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, this.credentials.MicrosoftAppId, activity.ServiceUrl);
            if (!isValidIdentity)
            {
                return this.Unauthorized();
            }

            // On message activity, reply with the same text
            if (activity.Type == ActivityTypes.Message)
            {
                var reply = activity.CreateReply($"You said: {activity.Text}");

                // Thrust service Url
                MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);

                // Reply to Activity using Connector
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl, UriKind.Absolute), credentials);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }

            return this.Ok();
        }
    }
}
