// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public interface IUserTokenProvider
    {
        /// <summary>Attempts to retrieve the token for a user that's in a login flow, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, string connectionName, string magicCode, CancellationToken cancellationToken);

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, CancellationToken cancellationToken);

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Signs the user out with the token server, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">User id of user to sign out.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task SignOutUserAsync(ITurnContext turnContext, string connectionName = null, string userId = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves the token status for each configured connection for the given user, using the bot's AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="userId">The user Id for which token status is retrieved.</param>
        /// <param name="includeFilter">Optional comma separated list of connection's to include. Blank will return token status for all configured connections.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>Array of TokenStatus.</returns>
        Task<TokenStatus[]> GetTokenStatusAsync(ITurnContext context, string userId, string includeFilter = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves Azure Active Directory tokens for particular resources on a configured connection, using the bot's AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">The name of the Azure Active Directory connection configured with this bot.</param>
        /// <param name="resourceUrls">The list of resource URLs to retrieve tokens for.</param>
        /// <param name="userId">The user Id for which tokens are retrieved. If passing in null the userId is taken from the Activity in the ITurnContext.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>Dictionary of resourceUrl to the corresponding TokenResponse.</returns>
        Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(ITurnContext context, string connectionName, string[] resourceUrls, string userId = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
