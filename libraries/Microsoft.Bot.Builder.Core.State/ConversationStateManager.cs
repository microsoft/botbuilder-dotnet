using System;

namespace Microsoft.Bot.Builder.Core.State
{
    public interface IConversationStateManager : IStateManager
    {
        string ChannelId { get; }
        string ConversationId { get; }
    }

    public class ConversationStateManager : StateManager, IConversationStateManager
    {
        public ConversationStateManager(string channelId, string conversationId, IStateStorageProvider stateStore) : base(BuildStateNamespace(channelId, conversationId), stateStore)
        {
            ChannelId = channelId ?? throw new System.ArgumentNullException(nameof(channelId));
            ConversationId = conversationId ?? throw new System.ArgumentNullException(nameof(conversationId));
        }


        public string ChannelId { get; }

        public string ConversationId { get; }

        public static string BuildStateNamespace(string channelId, string conversationId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentException("Expected a non-null/empty value.", nameof(channelId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentException("Expected a non-null/empty value.", nameof(conversationId));
            }

            return $"/channels/{channelId}/conversations/{conversationId}";
        }

        public static string BuildStateNamespace(ITurnContext turnContext)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            return BuildStateNamespace(turnContext.Activity.ChannelId, turnContext.Activity.Conversation.Id);
        }
    }
}
