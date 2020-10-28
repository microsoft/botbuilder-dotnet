// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TestBot.Shared.Bots
{
    public class ProactiveBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var claimsIdentity = turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) as ClaimsIdentity;

            var botAppIdClaim = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim);

            var appId = botAppIdClaim.Value;

            var conversationReference = turnContext.Activity.GetConversationReference();

            await turnContext.Adapter.ContinueConversationAsync(appId, conversationReference, BotCallback, cancellationToken);
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("proactive hello");
        }
    }
}
