// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using SlackAPI;
using SlackAPI.RPCMessages;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackClientWrapper
    {
        private SlackTaskClient _api;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackClientWrapper"/> class.
        /// Creates a Slack client by supplying the access token.
        /// </summary>
        /// <param name="botToken">The bot token from the Slack account.</param>
        public SlackClientWrapper(string botToken)
        {
            _api = new SlackTaskClient(botToken);
        }

        /// <summary>
        /// Wraps Slack API's AddReactionAsync method.
        /// </summary>
        /// <param name="name">The optional name.</param>
        /// <param name="channel">The optional channel.</param>
        /// <param name="timestamp">The optional timestamp.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The <see cref="ReactionAddedResponse"/> representing the response to the reaction added.</returns>
        public virtual async Task<ReactionAddedResponse> AddReactionAsync(string name = null, string channel = null, string timestamp = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.AddReactionAsync(name, channel, timestamp).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's APIRequestWithTokenAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <param name="postParameters">The parameters to the POST request.</param>
        /// <typeparam name="TResult">The generic type for the return. Must be of type Response.</param>
        /// <returns>A <see cref="Task"/> of type T representing the asynchronous operation.</returns>
        public virtual async Task<TResult> APIRequestWithTokenAsync<TResult>(CancellationToken cancellationToken, params Tuple<string, string>[] postParameters)
             where TResult : Response
        {
            return (postParameters != null) ?
                await _api.APIRequestWithTokenAsync<TResult>().ConfigureAwait(false) :
                await _api.APIRequestWithTokenAsync<TResult>(postParameters).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's ChannelsCreateAsync method.
        /// </summary>
        /// <param name="name">Name of the channel to be created.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="ChannelCreateResponse"/> representing the response from creating a channel.</returns>
        public virtual async Task<ChannelCreateResponse> ChannelsCreateAsync(string name, CancellationToken cancellationToken)
        {
            return await _api.ChannelsCreateAsync(name).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's ChannelSetTopicAsync method.
        /// </summary>
        /// <param name="channelId">The channel to set the topic to.</param>
        /// <param name="newTopic">The new topic.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="ChannelSetTopicResponse"/> representing the response of setting the channel's topic.</returns>
        public virtual async Task<ChannelSetTopicResponse> ChannelSetTopicAsync(string channelId, string newTopic, CancellationToken cancellationToken)
        {
            return await _api.ChannelSetTopicAsync(channelId, newTopic).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's ChannelsInviteAsync method.
        /// </summary>
        /// <param name="userId">The user to invite.</param>
        /// <param name="channelId">The channel to invite the user to.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="ChannelInviteResponse"/> representing the response to the invite.</returns>
        public virtual async Task<ChannelInviteResponse> ChannelsInviteAsync(string userId, string channelId, CancellationToken cancellationToken)
        {
            return await _api.ChannelsInviteAsync(userId, channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's ConnectAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="LoginResponse"/> representing the login response.</returns>
        public virtual async Task<LoginResponse> ConnectAsync(CancellationToken cancellationToken)
        {
            return await _api.ConnectAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's DeleteMessageAsync method.
        /// </summary>
        /// <param name="channelId">The channel to delete the message from.</param>
        /// <param name="ts">The timestamp of the message.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="DeletedResponse"/> representing the response to deleting the message.</returns>
        public virtual async Task<DeletedResponse> DeleteMessageAsync(string channelId, DateTime ts, CancellationToken cancellationToken)
        {
            return await _api.DeleteMessageAsync(channelId, ts).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's DialogOpenAsync method.
        /// </summary>
        /// <param name="triggerId">.</param>
        /// <param name="dialog">The dialog to open.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="DialogOpenResponse"/> representing the response to the operation.</returns>
        public virtual async Task<DialogOpenResponse> DialogOpenAsync(string triggerId, Dialog dialog, CancellationToken cancellationToken)
        {
            return await _api.DialogOpenAsync(triggerId, dialog).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's EmitLoginAsync method.
        /// </summary>
        /// <param name="agent">The agent name.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="LoginResponse"/> representing the response to the login operation.</returns>
        public virtual async Task<LoginResponse> EmitLoginAsync(string agent = "Inumedia.SlackAPI", CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.EmitLoginAsync(agent).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's EmitPresence method.
        /// </summary>
        /// <param name="status">The presence status.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="PresenceResponse"/> representing the response of the operation.</returns>
        public virtual async Task<PresenceResponse> EmitPresence(Presence status, CancellationToken cancellationToken)
        {
            return await _api.EmitPresence(status).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetChannelHistoryAsync method.
        /// </summary>
        /// <param name="channelInfo">The channel info.</param>
        /// <param name="latest">The date of the latest.</param>
        /// <param name="oldest">The date of the oldest.</param>
        /// <param name="count">The requested count.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="ChannelMessageHistory"/> representing the response.</returns>
        public virtual async Task<ChannelMessageHistory> GetChannelHistoryAsync(Channel channelInfo, DateTime? latest = null, DateTime? oldest = null, int? count = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.GetChannelHistoryAsync(channelInfo, latest, oldest, count).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetChannelListAsync method.
        /// </summary>
        /// <param name="excludeArchived">Flag to set if archived channels are to be listed.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="ChannelListResponse"/> representing the response of listing the channel.</returns>
        public virtual async Task<ChannelListResponse> GetChannelListAsync(bool excludeArchived = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.GetChannelListAsync(excludeArchived).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetCountsAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="UserCountsResponse"/> representing the response.</returns>
        public virtual async Task<UserCountsResponse> GetCountsAsync()
        {
            return await _api.GetCountsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetDirectMessageHistoryAsync method.
        /// </summary>
        /// <param name="conversationInfo">.</param>
        /// <param name="latest">The date of the latest.</param>
        /// <param name="oldest">The date of the oldest.</param>
        /// <param name="count">The requested count.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="MessageHistory"/> representing the retrieved message history.</returns>
        public virtual async Task<MessageHistory> GetDirectMessageHistoryAsync(DirectMessageConversation conversationInfo, DateTime? latest = null, DateTime? oldest = null, int? count = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.GetDirectMessageHistoryAsync(conversationInfo, latest, oldest, count).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetDirectMessageListAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="DirectMessageConversationListResponse"/> representing the response to listing the messages.</returns>
        public virtual async Task<DirectMessageConversationListResponse> GetDirectMessageListAsync(CancellationToken cancellationToken)
        {
            return await _api.GetDirectMessageListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetFileInfoAsync method.
        /// </summary>
        /// <param name="fileId">The id of the file to retrieve the info from.</param>
        /// <param name="page">The page number.</param>
        /// <param name="count">The requested count.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="FileInfoResponse"/> representing the response.</returns>
        public virtual async Task<FileInfoResponse> GetFileInfoAsync(string fileId, int? page = null, int? count = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.GetFileInfoAsync(fileId, page, count).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetFilesAsync method.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="dateFrom">The initial date.</param>
        /// <param name="dateTo">The final date.</param>
        /// <param name="count">The requested count.</param>
        /// <param name="page">The page number.</param>
        /// <param name="types">The type of files to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="FileListResponse"/> representing the response.</returns>
        public virtual async Task<FileListResponse> GetFilesAsync(string userId = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? count = null, int? page = null, FileTypes types = FileTypes.all, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.GetFilesAsync(userId, dateFrom, dateTo, count, page, types).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetGroupHistoryAsync method.
        /// </summary>
        /// <param name="groupInfo">Info about the channel.</param>
        /// <param name="latest">The date of the latest.</param>
        /// <param name="oldest">The date of the oldest.</param>
        /// <param name="count">The requested count.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupMessageHistory"/> representing the asynchronous operation.</returns>
        public virtual async Task<GroupMessageHistory> GetGroupHistoryAsync(Channel groupInfo, DateTime? latest = null, DateTime? oldest = null, int? count = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.GetGroupHistoryAsync(groupInfo, latest, oldest, count).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetGroupsListAsync method.
        /// </summary>
        /// <param name="excludeArchived">Flag setting if archived groups are to be excluded.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupListResponse"/> with the list of groups.</returns>
        public virtual async Task<GroupListResponse> GetGroupsListAsync(bool excludeArchived = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.GetGroupsListAsync(excludeArchived).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetPreferencesAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="UserPreferencesResponse"/> representing the asynchronous operation.</returns>
        public virtual async Task<UserPreferencesResponse> GetPreferencesAsync(CancellationToken cancellationToken)
        {
            return await _api.GetPreferencesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetStarsAsync method.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="count">The count to retrieve.</param>
        /// <param name="page">The page to retrieve from.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="StarListResponse"/> representing the asynchronous operation.</returns>
        public virtual async Task<StarListResponse> GetStarsAsync(string userId = null, int? count = null, int? page = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.GetStarsAsync(userId, count, page).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GetUserListAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="UserListResponse"/> representing the response.</returns>
        public virtual async Task<UserListResponse> GetUserListAsync()
        {
            return await _api.GetUserListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsArchiveAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupArchiveResponse"/> representing the response to the operation.</returns>
        public virtual async Task<GroupArchiveResponse> GroupsArchiveAsync(string channelId, CancellationToken cancellationToken)
        {
            return await _api.GroupsArchiveAsync(channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsCloseAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupCloseResponse"/> representing the response.</returns>
        public virtual async Task<GroupCloseResponse> GroupsCloseAsync(string channelId, CancellationToken cancellationToken)
        {
            return await _api.GroupsCloseAsync(channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsCreateAsync method.
        /// </summary>
        /// <param name="name">The name of the group to create.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupCreateResponse"/> representing the response to the group creation.</returns>
        public virtual async Task<GroupCreateResponse> GroupsCreateAsync(string name, CancellationToken cancellationToken)
        {
            return await _api.GroupsCreateAsync(name).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsCreateChildAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupCreateChildResponse"/> representing the asynchronous operation.</returns>
        public virtual async Task<GroupCreateChildResponse> GroupsCreateChildAsync(string channelId, CancellationToken cancellationToken)
        {
            return await _api.GroupsCreateChildAsync(channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsInviteAsync method.
        /// </summary>
        /// <param name="userId">The id of the user to invite.</param>
        /// <param name="channelId">The channel to invite the user to.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupInviteResponse"/> representing the response.</returns>
        public virtual async Task<GroupInviteResponse> GroupsInviteAsync(string userId, string channelId, CancellationToken cancellationToken)
        {
            return await _api.GroupsInviteAsync(userId, channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsKickAsync method.
        /// </summary>
        /// <param name="userId">The id of the user to kick.</param>
        /// <param name="channelId">The channel to kick the user from.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupKickResponse"/> representing the response.</returns>
        public virtual async Task<GroupKickResponse> GroupsKickAsync(string userId, string channelId, CancellationToken cancellationToken)
        {
            return await _api.GroupsKickAsync(userId, channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsLeaveAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupLeaveResponse"/> representing the response.</returns>
        public virtual async Task<GroupLeaveResponse> GroupsLeaveAsync(string channelId, CancellationToken cancellationToken)
        {
            return await _api.GroupsLeaveAsync(channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsMarkAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="ts">The timestamp.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupMarkResponse"/> representing the response.</returns>
        public virtual async Task<GroupMarkResponse> GroupsMarkAsync(string channelId, DateTime ts, CancellationToken cancellationToken)
        {
            return await _api.GroupsMarkAsync(channelId, ts).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsOpenAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupOpenResponse"/> representing the response.</returns>
        public virtual async Task<GroupOpenResponse> GroupsOpenAsync(string channelId, CancellationToken cancellationToken)
        {
            return await _api.GroupsOpenAsync(channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsRenameAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="name">The new name to set to the group.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupRenameResponse"/> representing the response to the group renaming.</returns>
        public virtual async Task<GroupRenameResponse> GroupsRenameAsync(string channelId, string name, CancellationToken cancellationToken)
        {
            return await _api.GroupsRenameAsync(channelId, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsSetPurposeAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="purpose">The new purpose.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupSetPurposeResponse"/> representing the asynchronous operation.</returns>
        public virtual async Task<GroupSetPurposeResponse> GroupsSetPurposeAsync(string channelId, string purpose, CancellationToken cancellationToken)
        {
            return await _api.GroupsSetPurposeAsync(channelId, purpose).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsSetTopicAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="topic">The new topic.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupSetTopicResponse"/> representing the response to setting the topic.</returns>
        public virtual async Task<GroupSetTopicResponse> GroupsSetTopicAsync(string channelId, string topic, CancellationToken cancellationToken)
        {
            return await _api.GroupsSetTopicAsync(channelId, topic).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's GroupsUnarchiveAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="GroupUnarchiveResponse"/> representing the response.</returns>
        public virtual async Task<GroupUnarchiveResponse> GroupsUnarchiveAsync(string channelId, CancellationToken cancellationToken)
        {
            return await _api.GroupsUnarchiveAsync(channelId).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's JoinDirectMessageChannelAsync method.
        /// </summary>
        /// <param name="user">The user to join in.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="JoinDirectMessageChannelResponse"/> representing the response.</returns>
        public virtual async Task<JoinDirectMessageChannelResponse> JoinDirectMessageChannelAsync(string user, CancellationToken cancellationToken)
        {
            return await _api.JoinDirectMessageChannelAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's MarkChannelAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="ts">The timestamp.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="MarkResponse"/> representing the response.</returns>
        public virtual async Task<MarkResponse> MarkChannelAsync(string channelId, DateTime ts, CancellationToken cancellationToken)
        {
            return await _api.MarkChannelAsync(channelId, ts).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's PostEphemeralMessageAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="text">The text of the message.</param>
        /// <param name="targetUser">The target user to the message.</param>
        /// <param name="parse">Change how messages are treated.Defaults to 'none'. See https://api.slack.com/methods/chat.postMessage#formatting. </param>
        /// <param name="linkNames">If to find and link channel names and username.</param>
        /// <param name="attachments">The attachments, if any.</param>
        /// <param name="asUser">If the message is being sent as user instead of as a bot.</param>
        /// <param name="threadTs">Info about the message coming from a thread. CURRENTLY NOT USED.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="PostEphemeralResponse"/> representing the response to the message posting.</returns>
        public virtual async Task<PostEphemeralResponse> PostEphemeralMessageAsync(string channelId, string text, string targetUser, string parse = null, bool linkNames = false, Attachment[] attachments = null, bool asUser = false, string threadTs = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.PostEphemeralMessageAsync(channelId, text, targetUser, parse, linkNames, attachments, asUser, threadTs).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's PostMessageAsync method.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="text">The text of the message.</param>
        /// <param name="botName">The bot name.</param>
        /// <param name="parse">Change how messages are treated.Defaults to 'none'. See https://api.slack.com/methods/chat.postMessage#formatting .</param>
        /// <param name="linkNames">If to find and link channel names and username.</param>
        /// <param name="blocks">A JSON-based array of structured blocks, presented as a URL-encoded string.</param>
        /// <param name="attachments">The attachments, if any.</param>
        /// <param name="unfurlLinks">True to enable unfurling of primarily text-based content.</param>
        /// <param name="iconUrl">The url of the icon with the message, if any.</param>
        /// <param name="iconEmoji">The emoji icon, if any.</param>
        /// <param name="asUser">If the message is being sent as user instead of as a bot.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="PostMessageResponse"/> representing the response to the message posting.</returns>
        public virtual async Task<PostMessageResponse> PostMessageAsync(string channelId, string text, string botName = null, string parse = null, bool linkNames = false, IBlock[] blocks = null, Attachment[] attachments = null, bool unfurlLinks = false, string iconUrl = null, string iconEmoji = null, bool asUser = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.PostMessageAsync(channelId, text, botName, parse, linkNames, blocks, attachments, unfurlLinks, iconUrl, iconEmoji, asUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's SearchAllAsync method.
        /// </summary>
        /// <param name="query">The query to search.</param>
        /// <param name="sorting">The sorting.</param>
        /// <param name="direction">The direction of the sorting.</param>
        /// <param name="enableHighlights">Set if highlights are enabled.</param>
        /// <param name="count">The count to return.</param>
        /// <param name="page">The page to search from.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="SearchResponseAll"/> representing the response for the operation.</returns>
        public virtual async Task<SearchResponseAll> SearchAllAsync(string query, string sorting = null, SearchSortDirection? direction = null, bool enableHighlights = false, int? count = null, int? page = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.SearchAllAsync(query, sorting, direction, enableHighlights, count, page).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's SearchFilesAsync method.
        /// </summary>
        /// <param name="query">The query to search.</param>
        /// <param name="sorting">The sorting.</param>
        /// <param name="direction">The direction of the sorting.</param>
        /// <param name="enableHighlights">Set if highlights are enabled.</param>
        /// <param name="count">The count to return.</param>
        /// <param name="page">The page to search from.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="SearchResponseFiles"/> representing the response of the search operation.</returns>
        public virtual async Task<SearchResponseFiles> SearchFilesAsync(string query, string sorting = null, SearchSortDirection? direction = null, bool enableHighlights = false, int? count = null, int? page = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.SearchFilesAsync(query, sorting, direction, enableHighlights, count, page).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's SearchMessagesAsync method.
        /// </summary>
        /// <param name="query">The query to search.</param>
        /// <param name="sorting">The sorting.</param>
        /// <param name="direction">The direction of the sorting.</param>
        /// <param name="enableHighlights">Set if highlights are enabled.</param>
        /// <param name="count">The count to return.</param>
        /// <param name="page">The page to search from.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="SearchResponseMessages"/> representing the response of the search operation.</returns>
        public virtual async Task<SearchResponseMessages> SearchMessagesAsync(string query, string sorting = null, SearchSortDirection? direction = null, bool enableHighlights = false, int? count = null, int? page = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.SearchMessagesAsync(query, sorting, direction, enableHighlights, count).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's TestAuthAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The user Id.</returns>
        public virtual async Task<string> TestAuthAsync(CancellationToken cancellationToken)
        {
            var auth = await _api.TestAuthAsync().ConfigureAwait(false);
            return auth.user_id;
        }

        /// <summary>
        /// Wraps Slack API's UpdateAsync method.
        /// </summary>
        /// <param name="ts">The timestamp of the message.</param>
        /// <param name="channelId">The channel to delete the message from.</param>
        /// <param name="text">The text to update with.</param>
        /// <param name="botName">The optional bot name.</param>
        /// <param name="parse">Change how messages are treated.Defaults to 'none'. See https://api.slack.com/methods/chat.postMessage#formatting. </param>
        /// <param name="linkNames">If to find and link channel names and username.</param>
        /// <param name="attachments">The attachments, if any.</param>
        /// <param name="asUser">If the message is being sent as user instead of as a bot.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="UpdateResponse"/> representing the response to the operation.</returns>
        public virtual async Task<UpdateResponse> UpdateAsync(string ts, string channelId, string text, string botName = null, string parse = null, bool linkNames = false, Attachment[] attachments = null, bool asUser = false, CancellationToken cancellationToken = default)
        {
            return await _api.UpdateAsync(ts, channelId, text, botName, parse, linkNames, attachments, asUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's UploadFileAsync method.
        /// </summary>
        /// <param name="fileData">The data of the file to be uploaded.</param>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <param name="channelIds">The channel ids.</param>
        /// <param name="title">The title.</param>
        /// <param name="initialComment">The optional message text introducing the file in specified channels.</param>
        /// <param name="useAsync">Whether to use the async version of the upload.</param>
        /// <param name="fileType">A file type identifier. See https://api.slack.com/types/file#file_types for the available types.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="FileUploadResponse"/> representing the response.</returns>
        public virtual async Task<FileUploadResponse> UploadFileAsync(byte[] fileData, string fileName, string[] channelIds, string title = null, string initialComment = null, bool useAsync = false, string fileType = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _api.UploadFileAsync(fileData, fileName, channelIds, title, initialComment, useAsync, fileType).ConfigureAwait(false);
        }
    }
}
