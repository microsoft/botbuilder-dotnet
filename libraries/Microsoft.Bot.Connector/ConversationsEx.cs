
namespace Microsoft.Bot.Connector
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Rest;


    public static partial class ConversationsExtensions
    {
        /// <summary>
        /// Create a new direct conversation between a bot and a user
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='bot'>Bot to create conversation from</param>
        /// <param name='user'>User to create conversation with</param>
        /// <param name="activity">(OPTIONAL) initial message to send to the new conversation</param>
        public static ConversationResourceResponse CreateDirectConversation(this IConversations operations, ChannelAccount bot, ChannelAccount user, Activity activity = null)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).CreateConversationAsync(GetDirectParameters(bot, user, activity)), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='bot'>Bot to create conversation from</param>
        /// <param name='user'>User to create conversation with</param>
        /// <param name="activity">(OPTIONAL) initial message to send to the new conversation</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        public static async Task<ConversationResourceResponse> CreateDirectConversationAsync(this IConversations operations, ChannelAccount bot, ChannelAccount user, Activity activity = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _result = await operations.CreateConversationWithHttpMessagesAsync(GetDirectParameters(bot, user, activity), null, cancellationToken).ConfigureAwait(false);
            var res = await _result.HandleErrorAsync<ConversationResourceResponse>().ConfigureAwait(false);
            MicrosoftAppCredentials.TrustServiceUrl(res.ServiceUrl);
            return res;
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='botAddress'>Bot to create conversation from</param>
        /// <param name='userAddress'>User to create conversation with</param>
        /// <param name="activity">(OPTIONAL) initial message to send to the new conversation</param>
        public static ConversationResourceResponse CreateDirectConversation(this IConversations operations, string botAddress, string userAddress, Activity activity = null)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).CreateConversationAsync(GetDirectParameters(botAddress, userAddress, activity)), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='botAddress'>Bot to create conversation from</param>
        /// <param name='userAddress'>User to create conversation with</param>
        /// <param name="activity">(OPTIONAL) initial message to send to the new conversation</param>
        /// <param name='cancellationToken'>The cancellation token</param>
        public static async Task<ConversationResourceResponse> CreateDirectConversationAsync(this IConversations operations, string botAddress, string userAddress, Activity activity = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _result = await operations.CreateConversationWithHttpMessagesAsync(GetDirectParameters(botAddress, userAddress, activity), null, cancellationToken).ConfigureAwait(false);
            var res = await _result.HandleErrorAsync<ConversationResourceResponse>().ConfigureAwait(false);
            MicrosoftAppCredentials.TrustServiceUrl(res.ServiceUrl);
            return res;
        }

        /// <summary>
        /// Send an activity to a conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        public static ResourceResponse SendToConversation(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).SendToConversationAsync(activity, activity.Conversation.Id), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Send an activity to a conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        public static Task<ResourceResponse> SendToConversationAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return operations.SendToConversationAsync(activity, activity.Conversation.Id, cancellationToken);
        }

        /// <summary>
        /// Replyto an activity in an existing conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        public static ResourceResponse ReplyToActivity(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).ReplyToActivityAsync(activity.Conversation.Id, activity.ReplyToId, activity), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reply to an activity in an existing conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        public static Task<ResourceResponse> ReplyToActivityAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TEMP TODO REMOVE THIS AFTER SKYPE DEPLOYS NEW SERVICE WHICH PROPERLY IMPLEMENTS THIS ENDPOINT
            if (activity.ReplyToId == "0")
                return operations.SendToConversationAsync(activity);

            if (activity.ReplyToId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "ReplyToId");
            }

            return operations.ReplyToActivityAsync(activity.Conversation.Id, activity.ReplyToId, activity, cancellationToken);
        }

        /// <summary>
        /// Update an activity in an existing conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to update
        /// </param>
        public static ResourceResponse UpdateActivity(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).UpdateActivityAsync(activity.Conversation.Id, activity.Id, activity), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Update an activity in an existing conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to update
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        public static Task<ResourceResponse> UpdateActivityAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return operations.UpdateActivityAsync(activity.Conversation.Id, activity.Id, activity, cancellationToken);
        }


        private static ConversationParameters GetDirectParameters(string botId, string userId, Activity activity)
        {
            return new ConversationParameters()
            {
                Bot = new ChannelAccount(botId),
                Members = new ChannelAccount[] { new ChannelAccount(userId) },
                Activity = activity
            };
        }

        private static ConversationParameters GetDirectParameters(ChannelAccount bot, ChannelAccount user, Activity activity)
        {
            return new ConversationParameters()
            {
                Bot = bot,
                Members = new ChannelAccount[] { user },
                Activity = activity
            };
        }

    }
}
