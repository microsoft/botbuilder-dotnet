using System;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Extensions
{
    public static class ActivityExtensions
    {
        public static void SetValueFromRequest(this IEventActivity activity, IRequestMessageBase wechatRequest)
        {
            SetRequiredField(activity, wechatRequest);
        }

        public static void SetValueFromRequest(this IMessageActivity activity, IRequestMessageBase wechatRequest)
        {
            SetRequiredField(activity, wechatRequest);
        }

        private static void SetRequiredField(IActivity activity, IRequestMessageBase wechatRequest)
        {
            if (wechatRequest is RequestMessage requestMessage)
            {
                activity.Id = requestMessage.MsgId.ToString();
            }
            else
            {
                // Event message don't have Id;
                activity.Id = new Guid().ToString();
            }

            activity.ChannelId = Channels.WeChat;
            activity.Recipient = new ChannelAccount(wechatRequest.ToUserName, "Bot", "bot");
            activity.From = new ChannelAccount(wechatRequest.FromUserName, "User", "user");

            // Message is handled by adapter itself, may not need serviceurl.
            activity.ServiceUrl = Constants.ServiceUrl;

            // Set user ID as conversation id. wechat request don't have conversation id.
            // TODO: consider how to handle conversation end request if needed. For now Wechat don't have this type.
            activity.Conversation = new ConversationAccount(false, id: wechatRequest.FromUserName);
            activity.Timestamp = DateTimeOffset.FromUnixTimeSeconds(wechatRequest.CreateTime);
            activity.ChannelData = wechatRequest;
        }
    }
}
