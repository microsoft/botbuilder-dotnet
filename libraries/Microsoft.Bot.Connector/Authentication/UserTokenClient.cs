// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Client for access user token service.
    /// </summary>
    public abstract class UserTokenClient : IDisposable
    {
        /// <summary>
        /// Attempts to retrieve the token for a user that's in a login flow.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        public abstract Task<TokenResponse> GetUserTokenAsync(string userId, string connectionName, string channelId, string magicCode, CancellationToken cancellationToken);

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="activity">The <see cref="Activity"/> from which to derive the token exchange state.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public abstract Task<SignInResource> GetSignInResourceAsync(string connectionName, Activity activity, string finalRedirect, CancellationToken cancellationToken);

        /// <summary>
        /// Signs the user out with the token server.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public abstract Task SignOutUserAsync(string userId, string connectionName, string channelId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the token status for each configured connection for the given user.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="includeFilter">The includeFilter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A Task of Array of TokenStatus.</returns>
        public abstract Task<TokenStatus[]> GetTokenStatusAsync(string userId, string channelId, string includeFilter, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves Azure Active Directory tokens for particular resources on a configured connection.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="resourceUrls">The list of resource URLs to retrieve tokens for.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A Task of Dictionary of resourceUrl to the corresponding TokenResponse.</returns>
        public abstract Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(string userId, string connectionName, string[] resourceUrls, string channelId, CancellationToken cancellationToken);

        /// <summary>
        /// Performs a token exchange operation such as for single sign-on.
        /// </summary>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="channelId">The channel Id that will be associated with the token.</param>
        /// <param name="exchangeRequest">The exchange request details, either a token to exchange or a uri to exchange.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public abstract Task<TokenResponse> ExchangeTokenAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Helper function to create the base64 encoded token exchange state used in GetSignInResourceAsync calls.
        /// </summary>
        /// <param name="appId">The appId to include in the token exchange state.</param>
        /// <param name="connectionName">The connectionName to include in the token exchange state.</param>
        /// <param name="activity">The <see cref="Activity"/> from which to derive the token exchange state.</param>
        /// <returns>base64 encoded token exchange state.</returns>
        protected static string CreateTokenExchangeState(string appId, string connectionName, Activity activity)
        {
            _ = appId ?? throw new ArgumentNullException(nameof(appId));
            _ = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _ = activity ?? throw new ArgumentNullException(nameof(activity));

            var tokenExchangeState = new TokenExchangeState
            {
                ConnectionName = connectionName,
                Conversation = activity.GetConversationReference(),
                RelatesTo = activity.RelatesTo,
                MsAppId = appId,
            };
            var json = JsonConvert.SerializeObject(tokenExchangeState);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        /// <summary>
        /// Protected implementation of dispose pattern.
        /// </summary>
        /// <param name="disposing">Indicates where this method is called from.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
