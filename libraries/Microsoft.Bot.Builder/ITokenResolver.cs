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
        /// <summary>
        /// Check for oauth cards in the activity's attachments 
        /// </summary>
        /// <param name="turnContext">Current turn contxt</param>
        /// <param name="activity">The activity</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The task after the execution has completed</returns>
        Task CheckForOAuthCard(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken);

        /// <summary>
        /// Get Signin URL associated with the connection name
        /// </summary>
        /// <param name="turnContext">The current turn context</param>
        /// <param name="oAuthClient"></param>
        /// <param name="connectionName">The connection name</param>
        /// <param name="msAppId">The Microsoft App ID</param>
        /// <param name="cancellationToken">The cancelation token</param>
        /// <returns>The signin URL string</returns>
        Task<string> GetSignInUrl(ITurnContext turnContext, OAuthClient oAuthClient, string connectionName, string msAppId, CancellationToken cancellationToken);
    }
}
