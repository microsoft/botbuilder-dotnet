// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// The key that minimally and completely identifies a bot's conversation with a user on a channel. 
    /// </summary>
    public interface IAddress
    {
        string BotId { get; }
        string ChannelId { get; }
        string UserId { get; }
        string ConversationId { get; }
        string ServiceUrl { get; }
    }

    /// <summary>
    /// The key that minimally and completely identifies a bot's conversation with a user on a channel. 
    /// </summary>
    [Serializable]
    public sealed class Address : IAddress, IEquatable<IAddress>
    {
        public static Address FromActivity(IActivity activity)
        {
            return new Address
                (
                    // purposefully using named arguments because these all have the same type
                    botId: activity.Recipient.Id,
                    channelId: activity.ChannelId,
                    userId: activity.From.Id,
                    conversationId: activity.Conversation.Id,
                    serviceUrl: activity.ServiceUrl
                );
        }

        [JsonConstructor]
        public Address(string botId, string channelId, string userId, string conversationId, string serviceUrl)
        {
            SetField.CheckNull(nameof(botId), botId);
            SetField.CheckNull(nameof(channelId), channelId);
            SetField.CheckNull(nameof(userId), userId);
            SetField.CheckNull(nameof(conversationId), conversationId);
            SetField.CheckNull(nameof(serviceUrl), serviceUrl);

            this.BotId = botId;
            this.ChannelId = channelId;
            this.UserId = userId;
            this.ConversationId = conversationId;
            this.ServiceUrl = serviceUrl;
        }
        public string BotId { get; }
        public string ChannelId { get; }
        public string UserId { get; }
        public string ConversationId { get; }
        public string ServiceUrl { get; }

        public bool Equals(IAddress other)
        {
            return other != null
                && object.Equals(this.BotId, other.BotId)
                && object.Equals(this.ChannelId, other.ChannelId)
                && object.Equals(this.UserId, other.UserId)
                && object.Equals(this.ConversationId, other.ConversationId)
                && object.Equals(this.ServiceUrl, other.ServiceUrl)
                ;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as IAddress);
        }

        public override int GetHashCode()
        {
            var code
                = this.BotId.GetHashCode()
                ^ this.ChannelId.GetHashCode()
                ^ this.UserId.GetHashCode()
                ^ this.ConversationId.GetHashCode()
                ^ this.ServiceUrl.GetHashCode()
                ;

            return code;
        }
    }

    /// <summary>
    /// Compare two Address instances for equality, excluding the user information.
    /// </summary>
    /// <remarks>
    /// This equality comparer excludes the user from the Address identity
    /// so that dialog execution can be serialized by conversation, thereby
    /// making it less likely to encounter 412 "precondition failed" when
    /// updating the bot state data bags with optimistic concurrency.  Updates
    /// to the user's data bags may still conflict across multiple conversations.
    /// </remarks>
    public sealed class ConversationAddressComparer : IEqualityComparer<IAddress>
    {
        bool IEqualityComparer<IAddress>.Equals(IAddress one, IAddress two)
        {
            var equals =
                object.ReferenceEquals(one, two)
                || (
                    object.Equals(one.BotId, two.BotId)
                    && object.Equals(one.ChannelId, two.ChannelId)
                    && object.Equals(one.ConversationId, two.ConversationId)
                    && object.Equals(one.ServiceUrl, two.ServiceUrl)
                    );

            return equals;
        }

        int IEqualityComparer<IAddress>.GetHashCode(IAddress address)
        {
            var code
                = address.BotId.GetHashCode()
                ^ address.ChannelId.GetHashCode()
                ^ address.ConversationId.GetHashCode()
                ^ address.ServiceUrl.GetHashCode()
                ;

            return code;
        }
    }
}
