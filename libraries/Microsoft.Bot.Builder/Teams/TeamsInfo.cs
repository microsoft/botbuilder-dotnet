// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams
{
    /// <summary>
    /// The TeamsInfo Test If Build Remote Successful
    /// provides utility methods for the events and interactions that occur within Microsoft Teams.
    /// </summary>
    public static class TeamsInfo
    {
        /// <summary>
        /// Gets the details for the given meeting participant. This only works in teams meeting scoped conversations. 
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="meetingId">The id of the Teams meeting. TeamsChannelData.Meeting.Id will be used if none provided.</param>
        /// <param name="participantId">The id of the Teams meeting participant. From.AadObjectId will be used if none provided.</param>
        /// <param name="tenantId">The id of the Teams meeting Tenant. TeamsChannelData.Tenant.Id will be used if none provided.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks> <see cref="InvalidOperationException"/> will be thrown if meetingId, participantId or tenantId have not been
        /// provided, and also cannot be retrieved from turnContext.Activity.</remarks>
        /// <returns>Team participant channel account.</returns>
        public static async Task<TeamsMeetingParticipant> GetMeetingParticipantAsync(ITurnContext turnContext, string meetingId = null, string participantId = null, string tenantId = null, CancellationToken cancellationToken = default)
        {
            meetingId ??= turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of a MS Teams Meeting.");
            participantId ??= turnContext.Activity.From.AadObjectId ?? throw new InvalidOperationException($"{nameof(participantId)} is required.");
            tenantId ??= turnContext.Activity.GetChannelData<TeamsChannelData>()?.Tenant?.Id ?? throw new InvalidOperationException($"{nameof(tenantId)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.FetchParticipantAsync(meetingId, participantId, tenantId, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the information for the given meeting id.
        /// </summary>
        /// <param name="turnContext"> Turn context.</param>
        /// <param name="meetingId"> The BASE64-encoded id of the Teams meeting.</param>
        /// <param name="cancellationToken"> Cancellation token.</param>
        /// <returns>Team Details.</returns>
        public static async Task<MeetingInfo> GetMeetingInfoAsync(ITurnContext turnContext, string meetingId = null, CancellationToken cancellationToken = default)
        {
            meetingId ??= turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("The meetingId can only be null if turnContext is within the scope of a MS Teams Meeting.");
            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.FetchMeetingInfoAsync(meetingId, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the details for the given team id. This only works in teams scoped conversations. 
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="teamId"> The id of the Teams team. </param>
        /// <param name="cancellationToken"> Cancellation token. </param>
        /// <returns>Team Details.</returns>
        public static async Task<TeamDetails> GetTeamDetailsAsync(ITurnContext turnContext, string teamId = null, CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.FetchTeamDetailsAsync(t, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns a list of channels in a Team. 
        /// This only works in teams scoped conversations.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="teamId"> ID of the Teams team. </param>
        /// <param name="cancellationToken"> cancellation token. </param>
        /// <returns>Team Details.</returns>
        public static async Task<IList<ChannelInfo>> GetTeamChannelsAsync(ITurnContext turnContext, string teamId = null, CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                var channelList = await teamsClient.Teams.FetchChannelListAsync(t, cancellationToken).ConfigureAwait(false);
                return channelList.Conversations;
            }
        }

        /// <summary>
        /// Gets the list of TeamsChannelAccounts within a team. 
        /// This only works in teams scoped conversations.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="teamId"> ID of the Teams team. </param>
        /// <param name="cancellationToken"> cancellation token. </param>
        /// <returns>TeamsChannelAccount.</returns>
        [Obsolete("Microsoft Teams is deprecating the non-paged version of the getMembers API which this method uses. Please use GetPagedTeamMembersAsync instead of this API.")]
        public static Task<IEnumerable<TeamsChannelAccount>> GetTeamMembersAsync(ITurnContext turnContext, string teamId = null, CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            return GetMembersAsync(GetConnectorClient(turnContext), t, cancellationToken);
        }

        /// <summary>
        /// Gets the conversation members of a one-on-one or group chat.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="cancellationToken"> Cancellation token. </param>
        /// <returns>TeamsChannelAccount.</returns>
        [Obsolete("Microsoft Teams is deprecating the non-paged version of the getMembers API which this method uses. Please use GetPagedTeamMembersAsync instead of this API.")]
        public static Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var teamInfo = turnContext.Activity.TeamsGetTeamInfo();

            if (teamInfo?.Id != null)
            {
                return GetTeamMembersAsync(turnContext, teamInfo.Id, cancellationToken);
            }
            else
            {
                var conversationId = turnContext.Activity?.Conversation?.Id;
                return GetMembersAsync(GetConnectorClient(turnContext), conversationId, cancellationToken);
            }
        }

        /// <summary>
        /// Gets a paginated list of members of a team. 
        /// This only works in teams scoped conversations.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="teamId"> ID of the Teams team. </param>
        /// <param name="continuationToken"> continuationToken token. </param>
        /// <param name="pageSize"> number of entries on the page. </param>
        /// /// <param name="cancellationToken"> cancellation token. </param>
        /// <returns>TeamsPagedMembersResult.</returns>
        public static Task<TeamsPagedMembersResult> GetPagedTeamMembersAsync(ITurnContext turnContext, string teamId = null, string continuationToken = default(string), int? pageSize = default(int?), CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            return GetPagedMembersAsync(GetConnectorClient(turnContext), t, continuationToken, cancellationToken, pageSize);
        }

        /// <summary>
        /// Gets a paginated list of members of one-on-one, group, or team conversation.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="pageSize"> Suggested number of entries on a page. </param>
        /// <param name="continuationToken"> ContinuationToken token. </param>
        /// /// <param name="cancellationToken"> Cancellation token. </param>
        /// <returns>TeamsPagedMembersResult.</returns>
        public static Task<TeamsPagedMembersResult> GetPagedMembersAsync(ITurnContext turnContext, int? pageSize = default(int?), string continuationToken = default(string), CancellationToken cancellationToken = default)
        {
            var teamInfo = turnContext.Activity.TeamsGetTeamInfo();

            if (teamInfo?.Id != null)
            {
                return GetPagedTeamMembersAsync(turnContext, teamInfo.Id, continuationToken, pageSize, cancellationToken);
            }
            else
            {
                var conversationId = turnContext.Activity?.Conversation?.Id;
                return GetPagedMembersAsync(GetConnectorClient(turnContext), conversationId, continuationToken, cancellationToken, pageSize);
            }
        }

        /// <summary>
        /// Gets the member of a teams scoped conversation.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="userId"> user id. </param>
        /// <param name="teamId"> ID of the Teams team. </param>
        /// <param name="cancellationToken"> cancellation token. </param>
        /// <returns>Team Details.</returns>
        public static Task<TeamsChannelAccount> GetTeamMemberAsync(ITurnContext turnContext, string userId, string teamId = null, CancellationToken cancellationToken = default)
        {
            var t = teamId ?? turnContext.Activity.TeamsGetTeamInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            return GetMemberAsync(GetConnectorClient(turnContext), userId, t, cancellationToken);
        }

        /// <summary>
        /// Gets the account of a single conversation member. 
        /// This works in one-on-one, group, and teams scoped conversations.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="userId"> ID of the user in question. </param>
        /// <param name="cancellationToken"> cancellation token. </param>
        /// <returns>Team Details.</returns>
        public static Task<TeamsChannelAccount> GetMemberAsync(ITurnContext turnContext, string userId, CancellationToken cancellationToken = default)
        {
            var teamInfo = turnContext.Activity.TeamsGetTeamInfo();

            if (teamInfo?.Id != null)
            {
                return GetTeamMemberAsync(turnContext, userId, teamInfo.Id, cancellationToken);
            }
            else
            {
                var conversationId = turnContext.Activity?.Conversation?.Id;
                return GetMemberAsync(GetConnectorClient(turnContext), userId, conversationId, cancellationToken);
            }
        }

        /// <summary>
        /// Creates a new thread in a team chat and sends an activity to that new thread. Use this method if you are using BotFrameworkAdapter and are handling credentials in your code.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="activity"> The activity to send on starting the new thread. </param>
        /// <param name="teamsChannelId"> The Team's Channel ID, note this is distinct from the Bot Framework activity property with same name. </param>
        /// <param name="credentials"> Microsoft app credentials. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>Team Details.</returns>
        public static async Task<Tuple<ConversationReference, string>> SendMessageToTeamsChannelAsync(ITurnContext turnContext, IActivity activity, string teamsChannelId, MicrosoftAppCredentials credentials, CancellationToken cancellationToken = default)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity == null)
            {
                throw new InvalidOperationException(nameof(turnContext.Activity));
            }

            if (string.IsNullOrEmpty(teamsChannelId))
            {
                throw new ArgumentNullException(nameof(teamsChannelId));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            ConversationReference conversationReference = null;
            var newActivityId = string.Empty;
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo() { Id = teamsChannelId } },
                Activity = (Activity)activity,
            };

            await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                teamsChannelId,
                serviceUrl,
                credentials,
                conversationParameters,
                (t, ct) =>
                {
                    conversationReference = t.Activity.GetConversationReference();
                    newActivityId = t.Activity.Id;
                    return Task.CompletedTask;
                },
                cancellationToken).ConfigureAwait(false);

            return new Tuple<ConversationReference, string>(conversationReference, newActivityId);
        }

        /// <summary>
        /// Creates a new thread in a team chat and sends an activity to that new thread. Use this method if you are using CloudAdapter where credentials are handled by the adapter.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="activity"> The activity to send on starting the new thread. </param>
        /// <param name="teamsChannelId"> The Team's Channel ID, note this is distinct from the Bot Framework activity property with same name. </param>
        /// <param name="botAppId"> The bot's appId. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>Team Details.</returns>
        public static async Task<Tuple<ConversationReference, string>> SendMessageToTeamsChannelAsync(ITurnContext turnContext, IActivity activity, string teamsChannelId, string botAppId, CancellationToken cancellationToken = default)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity == null)
            {
                throw new InvalidOperationException(nameof(turnContext.Activity));
            }

            if (string.IsNullOrEmpty(teamsChannelId))
            {
                throw new ArgumentNullException(nameof(teamsChannelId));
            }

            ConversationReference conversationReference = null;
            var newActivityId = string.Empty;
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo() { Id = teamsChannelId } },
                Activity = (Activity)activity,
            };

            await turnContext.Adapter.CreateConversationAsync(
                botAppId,
                Channels.Msteams,
                serviceUrl,
                null,
                conversationParameters,
                (t, ct) =>
                {
                    conversationReference = t.Activity.GetConversationReference();
                    newActivityId = t.Activity.Id;
                    return Task.CompletedTask;
                },
                cancellationToken).ConfigureAwait(false);

            return new Tuple<ConversationReference, string>(conversationReference, newActivityId);
        }

        /// <summary>
        /// Sends a notification to meeting participants. This functionality is available only in teams meeting scoped conversations. 
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="notification">The notification to send to Teams.</param>
        /// <param name="meetingId">The id of the Teams meeting. TeamsChannelData.Meeting.Id will be used if none provided.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>InvalidOperationException will be thrown if meetingId or notification have not been
        /// provided, and also cannot be retrieved from turnContext.Activity.</remarks>
        /// <returns> <see cref="MeetingNotificationResponse"/>.</returns>
        public static async Task<MeetingNotificationResponse> SendMeetingNotificationAsync(ITurnContext turnContext, MeetingNotificationBase notification, string meetingId = null, CancellationToken cancellationToken = default)
        {
            meetingId ??= turnContext.Activity.TeamsGetMeetingInfo()?.Id ?? throw new InvalidOperationException("This method is only valid within the scope of a MS Teams Meeting.");
            notification = notification ?? throw new InvalidOperationException($"{nameof(notification)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.SendMeetingNotificationAsync(meetingId, notification, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a message to the provided list of Teams members.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="activity"> The activity to send. </param>
        /// <param name="teamsMembers"> The list of members. </param>
        /// <param name="tenantId"> The tenant ID. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>The operation Id.</returns>
        public static async Task<string> SendMessageToListOfUsersAsync(ITurnContext turnContext, IActivity activity, List<object> teamsMembers, string tenantId, CancellationToken cancellationToken = default)
        {
            activity = activity ?? throw new InvalidOperationException($"{nameof(activity)} is required.");
            teamsMembers = teamsMembers ?? throw new InvalidOperationException($"{nameof(teamsMembers)} is required.");
            tenantId = tenantId ?? throw new InvalidOperationException($"{nameof(tenantId)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.SendMessageToListOfUsersAsync(activity, teamsMembers, tenantId, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a message to all the users in a tenant.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="activity"> The activity to send to the tenant. </param>
        /// <param name="tenantId"> The tenant ID. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>The operation Id.</returns>
        public static async Task<string> SendMessageToAllUsersInTenantAsync(ITurnContext turnContext, IActivity activity, string tenantId, CancellationToken cancellationToken = default)
        {
            activity = activity ?? throw new InvalidOperationException($"{nameof(activity)} is required.");
            tenantId = tenantId ?? throw new InvalidOperationException($"{nameof(tenantId)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.SendMessageToAllUsersInTenantAsync(activity, tenantId, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a message to all the users in a team.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="activity"> The activity to send to the users in the team. </param>
        /// <param name="teamId"> The team ID. </param>
        /// <param name="tenantId"> The tenant ID. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>The operation Id.</returns>
        public static async Task<string> SendMessageToAllUsersInTeamAsync(ITurnContext turnContext, IActivity activity, string teamId, string tenantId, CancellationToken cancellationToken = default)
        {
            activity = activity ?? throw new InvalidOperationException($"{nameof(activity)} is required.");
            teamId = teamId ?? throw new InvalidOperationException($"{nameof(teamId)} is required.");
            tenantId = tenantId ?? throw new InvalidOperationException($"{nameof(tenantId)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.SendMessageToAllUsersInTeamAsync(activity, teamId, tenantId, cancellationToken).ConfigureAwait(false);
            }
        }
        
        /// <summary>
        /// Sends a message to the provided list of Teams channels.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="activity"> The activity to send. </param>
        /// <param name="channelsMembers"> The list of channels. </param>
        /// <param name="tenantId"> The tenant ID. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>The operation Id.</returns>
        public static async Task<string> SendMessageToListOfChannelsAsync(ITurnContext turnContext, IActivity activity, List<object> channelsMembers, string tenantId, CancellationToken cancellationToken = default)
        {
            activity = activity ?? throw new InvalidOperationException($"{nameof(activity)} is required.");
            channelsMembers = channelsMembers ?? throw new InvalidOperationException($"{nameof(channelsMembers)} is required.");
            tenantId = tenantId ?? throw new InvalidOperationException($"{nameof(tenantId)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.SendMessageToListOfChannelsAsync(activity, channelsMembers, tenantId, cancellationToken).ConfigureAwait(false);
            }
        }
        
        /// <summary>
        /// Gets the state of an operation.
        /// </summary>
        /// <param name="turnContext"> Turn context. </param>
        /// <param name="operationId"> The operationId to get the state of. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>The state and responses of the operation.</returns>
        public static async Task<BatchOperationState> GetOperationStateAsync(ITurnContext turnContext, string operationId, CancellationToken cancellationToken = default)
        {
            operationId = operationId ?? throw new InvalidOperationException($"{nameof(operationId)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.GetOperationStateAsync(operationId, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the failed entries of a batch operation.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="operationId">The operationId to get the failed entries of.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of failed entries of the operation.</returns>
        public static async Task<BatchFailedEntriesResponse> GetPagedFailedEntriesAsync(ITurnContext turnContext, string operationId, CancellationToken cancellationToken = default)
        {
            operationId = operationId ?? throw new InvalidOperationException($"{nameof(operationId)} is required.");

            using (var teamsClient = GetTeamsConnectorClient(turnContext))
            {
                return await teamsClient.Teams.GetPagedFailedEntriesAsync(operationId, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(IConnectorClient connectorClient, string conversationId, CancellationToken cancellationToken)
        {
            if (conversationId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid conversation Id.");
            }

            var teamMembers = await connectorClient.Conversations.GetConversationMembersAsync(conversationId, cancellationToken).ConfigureAwait(false);
            var teamsChannelAccounts = teamMembers.Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>());
            return teamsChannelAccounts;
        }

        private static IConnectorClient GetConnectorClient(ITurnContext turnContext)
        {
            return turnContext.TurnState.Get<IConnectorClient>() ?? throw new InvalidOperationException("This method requires a connector client.");
        }

        private static async Task<TeamsChannelAccount> GetMemberAsync(IConnectorClient connectorClient, string userId, string conversationId, CancellationToken cancellationToken)
        {
            if (conversationId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid conversation Id.");
            }

            if (userId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid user Id.");
            }

            var teamMember = await ((Conversations)connectorClient.Conversations).GetConversationMemberAsync(userId, conversationId, cancellationToken).ConfigureAwait(false);
            var teamsChannelAccount = JObject.FromObject(teamMember).ToObject<TeamsChannelAccount>();
            return teamsChannelAccount;
        }

        private static async Task<TeamsPagedMembersResult> GetPagedMembersAsync(IConnectorClient connectorClient, string conversationId, string continuationToken, CancellationToken cancellationToken, int? pageSize = default(int?))
        {
            if (conversationId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid conversation Id.");
            }

            var pagedMemberResults = await connectorClient.Conversations.GetConversationPagedMembersAsync(conversationId, pageSize, continuationToken, cancellationToken).ConfigureAwait(false);
            var teamsPagedMemberResults = new TeamsPagedMembersResult(pagedMemberResults.ContinuationToken, pagedMemberResults.Members);
            return teamsPagedMemberResults;
        }

        private static ITeamsConnectorClient GetTeamsConnectorClient(ITurnContext turnContext)
        {
            var connectorClient = GetConnectorClient(turnContext);
            if (connectorClient is ConnectorClient connectorClientImpl)
            {
                return new TeamsConnectorClient(connectorClientImpl.BaseUri, connectorClientImpl.Credentials, connectorClientImpl.HttpClient, connectorClientImpl.HttpClient == null);
            }
            else
            {
                return new TeamsConnectorClient(connectorClient.BaseUri, connectorClient.Credentials);
            }
        }
    }
}
