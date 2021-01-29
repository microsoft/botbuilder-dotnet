// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;

    /// <summary>
    /// UserToken operations.
    /// </summary>
    public partial interface IUserToken
    {
        /// <summary> Get token with HTTP message.</summary>
        /// <param name='userId'> User ID.</param>
        /// <param name='connectionName'> Connection name.</param>
        /// <param name='channelId'> Channel ID.</param>
        /// <param name='code'> Code.</param>
        /// <param name='customHeaders'> The headers that will be added to request.</param>
        /// <param name='cancellationToken'> The cancellation token.</param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code.</exception>
        /// <exception cref="Microsoft.Rest.SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="Microsoft.Rest.ValidationException"> Thrown when a required parameter is null. </exception>
        /// <returns>A Task representing the <see cref="TokenResponse"/> of the HTTP operation.</returns>
        Task<HttpOperationResponse<TokenResponse>> GetTokenWithHttpMessagesAsync(string userId, string connectionName, string channelId = default(string), string code = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Get AAD token with HTTP message.</summary>
        /// <param name='userId'> User ID.</param>
        /// <param name='connectionName'> Connection name.</param>
        /// <param name='aadResourceUrls'>AAD resource URLs. </param>
        /// <param name='channelId'>The channel ID. </param>
        /// <param name='customHeaders'> The headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token.</param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code.</exception>
        /// <exception cref="Microsoft.Rest.SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="Microsoft.Rest.ValidationException"> Thrown when a required parameter is null.</exception>
        /// <returns>A Task representing the <see cref="TokenResponse"/> of the HTTP operation.</returns>
        Task<HttpOperationResponse<IDictionary<string, TokenResponse>>> GetAadTokensWithHttpMessagesAsync(string userId, string connectionName, AadResourceUrls aadResourceUrls, string channelId = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>Sign out with HTTP message.</summary>
        /// <param name='userId'> User ID.</param>
        /// <param name='connectionName'>Connection name. </param>
        /// <param name='channelId'> Channel ID. </param>
        /// <param name='customHeaders'> The headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code. </exception>
        /// <exception cref="Microsoft.Rest.SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="Microsoft.Rest.ValidationException"> Thrown when a required parameter is null. </exception>
        /// <returns>A Task representing the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<object>> SignOutWithHttpMessagesAsync(string userId, string connectionName = default(string), string channelId = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Get the token status with HTTP message. </summary>
        /// <param name='userId'> User ID.</param>
        /// <param name='channelId'> Channel ID.</param>
        /// <param name='include'> Include.</param>
        /// <param name='customHeaders'> The headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="ErrorResponseException"> Thrown when the operation returned an invalid status code. </exception>
        /// <exception cref="Microsoft.Rest.SerializationException"> Thrown when unable to deserialize the response. </exception>
        /// <exception cref="Microsoft.Rest.ValidationException"> Thrown when a required parameter is null. </exception>
        /// <returns>A task representing an IList of <see cref="TokenStatus"/> from the HTTP operation.</returns>
        Task<HttpOperationResponse<IList<TokenStatus>>> GetTokenStatusWithHttpMessagesAsync(string userId, string channelId = default(string), string include = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
