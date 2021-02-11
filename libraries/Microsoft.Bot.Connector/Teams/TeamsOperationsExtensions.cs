// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Teams
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Extension methods for TeamsOperations.
    /// </summary>
    public static partial class TeamsOperationsExtensions
    {
        /// <summary>
        /// Fetches channel list for a given team.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='teamId'>
        /// Team Id.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <returns>The channel list for a given team.</returns>
        public static async Task<ConversationList> FetchChannelListAsync(this ITeamsOperations operations, string teamId, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var result = await operations.FetchChannelListWithHttpMessagesAsync(teamId, null, cancellationToken).ConfigureAwait(false))
            {
                return result.Body;
            }
        }

        /// <summary>
        /// Fetches details related to a team.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='teamId'>
        /// Team Id.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <returns>The details related to a team.</returns>
        public static async Task<TeamDetails> FetchTeamDetailsAsync(this ITeamsOperations operations, string teamId, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var result = await operations.FetchTeamDetailsWithHttpMessagesAsync(teamId, null, cancellationToken).ConfigureAwait(false))
            {
                return result.Body;
            }
        }

        /// <summary>
        /// Fetches participant details related to a Teams meeting.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='meetingId'>
        /// Team meeting Id.
        /// </param>
        /// <param name='participantId'>
        /// Team meeting participant Id.
        /// </param>
        /// <param name='tenantId'>
        /// Team meeting tenant Id.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <returns>The participant details related to a Teams meeting.</returns>
        public static async Task<TeamsMeetingParticipant> FetchParticipantAsync(this ITeamsOperations operations, string meetingId, string participantId, string tenantId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (operations is TeamsOperations teamsOperations)
            {
                using (var result = await teamsOperations.FetchParticipantWithHttpMessagesAsync(meetingId, participantId, tenantId, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }
            else
            {
                throw new InvalidOperationException("TeamsOperations with GetParticipantWithHttpMessagesAsync is required for FetchParticipantAsync.");
            }
        }
    }
}
