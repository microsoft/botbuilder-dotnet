// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Teams
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Rest;

    /// <summary>
    /// TeamsOperations operations.
    /// </summary>
    public partial interface ITeamsOperations
    {
        /// <summary>
        /// Fetches channel list for a given team.
        /// </summary>
        /// <param name='teamId'>
        /// Team Id.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>The channel list for a given team.</returns>
        Task<HttpOperationResponse<ConversationList>> FetchChannelListWithHttpMessagesAsync(string teamId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Fetches details related to a team.
        /// </summary>
        /// <param name='teamId'>
        /// Team Id.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>The details related to a team.</returns>
        Task<HttpOperationResponse<TeamDetails>> FetchTeamDetailsWithHttpMessagesAsync(string teamId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
