using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    public class ExtensionTokenResolverHelper
    {
        public static string InvokeResponseKey = "BotFrameworkStreamingExtensionsAdapter.InvokeResponse";

        public static IEventActivity CreateTokenResponse(ConversationReference relatesTo, string token, string connectionName)
        {
            var tokenResponse = Activity.CreateEventActivity() as Activity;

            // IActivity properties
            tokenResponse.Id = Guid.NewGuid().ToString();
            tokenResponse.Timestamp = DateTime.UtcNow;
            tokenResponse.From = relatesTo.User;
            tokenResponse.Recipient = relatesTo.Bot;
            tokenResponse.ReplyToId = relatesTo.ActivityId;
            tokenResponse.ServiceUrl = relatesTo.ServiceUrl;
            tokenResponse.ChannelId = relatesTo.ChannelId;
            tokenResponse.Conversation = relatesTo.Conversation;
            tokenResponse.Attachments = new List<Attachment>().ToArray();
            tokenResponse.Entities = new List<Entity>().ToArray();

            // IEventActivity properties
            tokenResponse.Name = "tokens/response";
            tokenResponse.RelatesTo = relatesTo;
            tokenResponse.Value = new TokenResponse()
            {
                Token = token,
                ConnectionName = connectionName
            };

            return tokenResponse;
        }

        public static ConversationReference GetConversationReference(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;

            return new ConversationReference()
            {
                ActivityId = activity.Id,
                Bot = new ChannelAccount
                {
                    Id = turnContext.Activity.Recipient.Id,
                    Name = turnContext.Activity.Recipient.Name
                },
                ChannelId = activity.ChannelId,
                Conversation = activity.Conversation,
                ServiceUrl = activity.ServiceUrl,
                User = new ChannelAccount
                {
                    Id = turnContext.Activity.From.Id,
                    Name = turnContext.Activity.From.Name
                },
            };
        }

        public static OAuthCard FindOAuthCard(Attachment attachment)
        {
            if (attachment.Content is OAuthCard)
            {
                return (OAuthCard)attachment.Content;
            }

            throw new ArgumentException("Invalid OAuthCard in activity attachment.");
        }
    }
}
