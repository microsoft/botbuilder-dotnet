using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    /// <summary>
    /// Defines utility methods used by the ExtensionResourceResolver class
    /// </summary>
    public class ExtensionTokenResolverHelper
    {
        public static string InvokeResponseKey = "BotFrameworkStreamingExtensionsAdapter.InvokeResponse";

        /// <summary>
        /// Creates a response containing a token.
        /// </summary>
        /// <param name="relatesTo"></param>
        /// <param name="token">The token to be included in the response</param>
        /// <param name="connectionName">The connection name of which this token belongs to</param>
        /// <returns>The resnsponse to be sent to the bot</returns>
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

        /// <summary>
        /// Gets the conversation's reference object with details the conversation.
        /// </summary>
        /// <param name="turnContext">The current turn context</param>
        /// <returns>The conversation reference object</returns>
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

        /// <summary>
        /// Find an OAuth card from an attachment.
        /// </summary>
        /// <param name="attachment">Activity attachment</param>
        /// <returns>Return the OAuth card found in the attachment</returns>
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
