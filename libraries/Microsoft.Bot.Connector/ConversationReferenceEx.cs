// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector
{
    public partial class ConversationReference : IEquatable<ConversationReference>
    {
        /// <summary>
        /// Creates <see cref="Activity"/> from conversation reference as it is posted to bot.
        /// </summary>
        public Activity GetPostToBotMessage()
        {
            return new Activity
            {
                Type = ActivityTypes.Message,
                Id = Guid.NewGuid().ToString(),
                Recipient = new ChannelAccount
                {
                    Id = this.Bot.Id,
                    Name = this.Bot.Name
                },
                ChannelId = this.ChannelId,
                ServiceUrl = this.ServiceUrl,
                Conversation = new ConversationAccount
                {
                    Id = this.Conversation.Id, 
                    IsGroup = this.Conversation.IsGroup, 
                    Name = this.Conversation.Name
                },
                From = new ChannelAccount
                {
                    Id = this.User.Id,
                    Name = this.User.Id
                }
            };
        }

        /// <summary>
        /// Creates <see cref="Activity"/> from conversation reference that can be posted to user as reply.
        /// </summary>
        public Activity GetPostToUserMessage()
        {
            var msg = this.GetPostToBotMessage();

            // swap from and recipient
            var bot = msg.Recipient;
            var user = msg.From;
            msg.From = bot;
            msg.Recipient = user;

            return msg;
        }

        public bool Equals(ConversationReference other)
        {
            return other != null
                && object.Equals(this.User, other.User)
                && object.Equals(this.Bot, other.Bot)
                && object.Equals(this.Conversation, other.Conversation)
                && this.ChannelId == other.ChannelId
                && this.ServiceUrl == other.ServiceUrl;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as ConversationReference);
        }

        public override int GetHashCode()
        {
            var code = this.User.GetHashCode()
                ^ this.Bot.GetHashCode()
                ^ this.Conversation.GetHashCode()
                ^ this.ServiceUrl.GetHashCode()
                ^ this.ChannelId.GetHashCode();
            return code;
        }
    }
}
