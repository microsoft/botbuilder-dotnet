using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Defines methods to help resolve tokens.
    /// </summary>
    public interface ITokenResolver
    {
        Task CheckForOAuthCard(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken);

        Task<string> GetSignInUrl(ITurnContext turnContext, OAuthClient oAuthClient, string connectionName, string msAppId, CancellationToken cancellationToken);
    }
}
