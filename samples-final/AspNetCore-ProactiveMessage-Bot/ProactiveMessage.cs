using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace AspNetCore_ProactiveMessage_Bot
{
    public class ProactiveMessage
    {
        public static string fromId;
        private static string fromName;
        private static string toId;
        private static string toName;
        private static string serviceUrl;
        private static string channelId;
        private static string conversationId;
        private static string locale;
        private static string text;

        public static void FromActivity(Activity activity, string messageText)
        {
            fromId = activity.From.Id;
            toName = activity.From.Name;
            fromId = activity.Recipient.Id;
            fromName = activity.Recipient.Name;
            serviceUrl = activity.ServiceUrl;
            channelId = activity.ChannelId;
            conversationId = activity.Conversation.Id;
            locale = activity.Locale;
            text = messageText;
        }

        //This will send an adhoc message to the user
        public static async Task Resume()
        {
            var userAccount = new ChannelAccount(toId, toName);
            var botAccount = new ChannelAccount(fromId, fromName);
            var connector = new ConnectorClient(new Uri(serviceUrl));

            IMessageActivity message = Activity.CreateMessageActivity();
            if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(channelId))
            {
                message.ChannelId = channelId;
            }
            else
            {
                conversationId = (await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount)).Id;
            }

            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(id: conversationId);
            message.Text = text;
            message.Locale = locale;

            await connector.Conversations.SendToConversationAsync((Activity)message);
        }
    }
}