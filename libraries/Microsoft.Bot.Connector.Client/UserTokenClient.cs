// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Client.Models;

namespace Microsoft.Bot.Connector.Client
{
    /// <summary>
    /// Client for accessing the user token service.
    /// </summary>
    public abstract class UserTokenClient
    {
        /// <summary>
        /// Attempts to retrieve the token for a user that's in a login flow.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="code">(Optional) User entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        public abstract Task<TokenResponse> GetTokenAsync(string userId, string connectionName, string channelId, string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves Azure Active Directory tokens for particular resources on a configured connection.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="resourceUrls">The list of resource URLs to retrieve tokens for.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A Dictionary of resourceUrl to the corresponding TokenResponse.</returns>
        public abstract Task<IReadOnlyDictionary<string, TokenResponse>> GetAadTokensAsync(string userId, string connectionName, string[] resourceUrls, string channelId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs the user out with the token server.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public abstract Task SignOutAsync(string userId, string connectionName, string channelId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the token status for each configured connection for the given user.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="include">The include filter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of Token Statuses.</returns>
        public abstract Task<IReadOnlyList<TokenStatus>> GetTokenStatusAsync(string userId, string channelId, string include, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a token exchange operation such as for single sign-on.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="exchangeRequest">The exchange request details, either a token to exchange or a uri to exchange.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="TokenResponse"/>.</returns>
        public abstract Task<TokenResponse> ExchangeTokenAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="activity">The <see cref="Activity"/> from which to derive the token exchange state.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="SignInUrlResponse"/> containing the raw signin link.</returns>
        public abstract Task<SignInUrlResponse> GetSignInResourceAsync(string connectionName, Activity activity, string finalRedirect, CancellationToken cancellationToken);
    }
}
