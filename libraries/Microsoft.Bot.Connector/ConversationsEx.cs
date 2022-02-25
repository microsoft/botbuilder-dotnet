﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// ConversationsExtensions Conversation extensions.
    /// </summary>
    public static partial class ConversationsExtensions
    {
        /// <summary>
        /// Create a new direct conversation between a bot and a user.
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='bot'>Bot to create conversation from.</param>
        /// <param name='user'>User to create conversation with.</param>
        /// <param name="activity">(OPTIONAL) initial message to send to the new conversation.</param>
        /// <returns>ConversationResourceResponse.</returns>
        public static ConversationResourceResponse CreateDirectConversation(this IConversations operations, ChannelAccount bot, ChannelAccount user, Activity activity = null)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).CreateConversationAsync(GetDirectParameters(bot, user, activity)), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user.
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='bot'>Bot to create conversation from.</param>
        /// <param name='user'>User to create conversation with.</param>
        /// <param name="activity">(OPTIONAL) initial message to send to the new conversation.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<ConversationResourceResponse> CreateDirectConversationAsync(this IConversations operations, ChannelAccount bot, ChannelAccount user, Activity activity = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await operations.CreateConversationWithHttpMessagesAsync(GetDirectParameters(bot, user, activity), null, cancellationToken).ConfigureAwait(false);
            return result.Body;
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user.
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='botAddress'>Bot to create conversation from.</param>
        /// <param name='userAddress'>User to create conversation with.</param>
        /// <param name="activity">(OPTIONAL) initial message to send to the new conversation.</param>
        /// <returns>ConversationResourceResponse.</returns>
        public static ConversationResourceResponse CreateDirectConversation(this IConversations operations, string botAddress, string userAddress, Activity activity = null)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).CreateConversationAsync(GetDirectParameters(botAddress, userAddress, activity)), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user.
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='botAddress'>Bot to create conversation from.</param>
        /// <param name='userAddress'>User to create conversation with.</param>
        /// <param name="activity">(OPTIONAL) initial message to send to the new conversation.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<ConversationResourceResponse> CreateDirectConversationAsync(this IConversations operations, string botAddress, string userAddress, Activity activity = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await operations.CreateConversationWithHttpMessagesAsync(GetDirectParameters(botAddress, userAddress, activity), null, cancellationToken).ConfigureAwait(false);
            return result.Body;
        }

        /// <summary>
        /// Send an activity to a conversation.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send.
        /// </param>
        /// <returns>ResourceResponse.</returns>
        public static ResourceResponse SendToConversation(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).SendToConversationAsync(activity.Conversation.Id, activity), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Send an activity to a conversation.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<ResourceResponse> SendToConversationAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return operations.SendToConversationAsync(activity.Conversation.Id, activity, cancellationToken);
        }

        /// <summary>
        /// Replyto an activity in an existing conversation.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send.
        /// </param>
        /// <returns>ResourceResponse.</returns>
        public static ResourceResponse ReplyToActivity(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).ReplyToActivityAsync(activity.Conversation.Id, activity.ReplyToId, activity), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reply to an activity in an existing conversation.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<ResourceResponse> ReplyToActivityAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity.ReplyToId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "ReplyToId");
            }

            return operations.ReplyToActivityAsync(activity.Conversation.Id, activity.ReplyToId, activity, cancellationToken);
        }

        /// <summary>
        /// Update an activity in an existing conversation.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to update.
        /// </param>
        /// <returns>ResourceResponse.</returns>
        public static ResourceResponse UpdateActivity(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).UpdateActivityAsync(activity.Conversation.Id, activity.Id, activity), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Update an activity in an existing conversation.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to update.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<ResourceResponse> UpdateActivityAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return operations.UpdateActivityAsync(activity.Conversation.Id, activity.Id, activity, cancellationToken);
        }

        private static ConversationParameters GetDirectParameters(string botId, string userId, Activity activity)
        {
            var convParameters = new ConversationParameters()
            {
                Bot = new ChannelAccount(botId),
                Activity = activity,
            };
            convParameters.Members.Add(new ChannelAccount(userId));

            return convParameters;
        }

        private static ConversationParameters GetDirectParameters(ChannelAccount bot, ChannelAccount user, Activity activity)
        {
            var convParameters = new ConversationParameters()
            {
                Bot = bot,
                Activity = activity,
            };
            convParameters.Members.Add(user);

            return convParameters;
        }
    }
}
