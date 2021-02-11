// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Extension methods for UserToken.
    /// </summary>
    public static partial class UserTokenExtensions
    {
            /// <summary> Get Token. </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='userId'> User ID. </param>
            /// <param name='connectionName'> Connection name. </param>
            /// <param name='channelId'> Channel ID. </param>
            /// <param name='code'> Code. </param>
            /// <param name='cancellationToken'> The cancellation token. </param>
            /// <returns> A task representing the token response. </returns>
            public static async Task<TokenResponse> GetTokenAsync(this IUserToken operations, string userId, string connectionName, string channelId = default(string), string code = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var result = await operations.GetTokenWithHttpMessagesAsync(userId, connectionName, channelId, code, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }
            
            /// <summary> Get AAD Token. </summary>
            /// <param name='operations'> The operations group for this extension method. </param>
            /// <param name='userId'> User ID. </param>
            /// <param name='connectionName'> Connection name. </param>
            /// <param name='aadResourceUrls'> AAD Resource URLs. </param>
            /// <param name='channelId'> Channel ID. </param>
            /// <param name='cancellationToken'> The cancellation token. </param>
            /// <returns> A task representing an IDictionary of TokenResponses. </returns>
            public static async Task<IDictionary<string, TokenResponse>> GetAadTokensAsync(this IUserToken operations, string userId, string connectionName, AadResourceUrls aadResourceUrls, string channelId = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var result = await operations.GetAadTokensWithHttpMessagesAsync(userId, connectionName, aadResourceUrls, channelId, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }

            /// <summary> Sign out. </summary>
            /// <param name='operations'> The operations group for this extension method. </param>
            /// <param name='userId'> User ID. </param>
            /// <param name='connectionName'> Connection name. </param>
            /// <param name='channelId'> Channel ID. </param>
            /// <param name='cancellationToken'> The cancellation token. </param>
            /// <returns> A task representing the work to be done. </returns>
            public static async Task<object> SignOutAsync(this IUserToken operations, string userId, string connectionName = default(string), string channelId = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var result = await operations.SignOutWithHttpMessagesAsync(userId, connectionName, channelId, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }

            /// <summary> Get Token Status. </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='userId'> User ID. </param>
            /// <param name='channelId'> Channel ID. </param>
            /// <param name='include'> Include. </param>
            /// <param name='cancellationToken'> The cancellation token. </param>
            /// <returns> A task representing the token status. </returns>
            public static async Task<IList<TokenStatus>> GetTokenStatusAsync(this IUserToken operations, string userId, string channelId = default(string), string include = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var result = await operations.GetTokenStatusWithHttpMessagesAsync(userId, channelId, include, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }

            /// <summary> Exchange. </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='userId'> User ID. </param>
            /// <param name='connectionName'> Connection name. </param>
            /// <param name='channelId'> Channel ID. </param>
            /// <param name='exchangeRequest'> Exchange request. </param>
            /// <param name='cancellationToken'> The cancellation token. </param>
            /// <returns> A task that represents the work queued to execute. </returns>
            public static async Task<object> ExchangeAsyncAsync(this OAuthClient operations, string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var result = await operations.ExchangeAsyncWithHttpMessagesAsync(userId, connectionName, channelId, exchangeRequest, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }
    }
}
