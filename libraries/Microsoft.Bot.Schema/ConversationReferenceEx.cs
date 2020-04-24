// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// ConversationReference extensions - helper for creating Activity.
    /// </summary>
    public partial class ConversationReference
    {
        /// <summary>
        /// Creates <see cref="Activity"/> from conversation reference as it is posted to bot.
        /// </summary>
        /// <returns>Continuation activity.</returns>
        public Activity GetContinuationActivity()
        {
            var activity = Activity.CreateEventActivity();
            activity.Name = "ContinueConversation";
            activity.Id = Guid.NewGuid().ToString();
            activity.ChannelId = this.ChannelId;
            (activity as Activity).Locale = this.Locale;
            activity.ServiceUrl = this.ServiceUrl;
            activity.Conversation = this.Conversation;
            activity.Recipient = this.Bot;
            activity.From = this.User;
            activity.RelatesTo = this;
            return (Activity)activity;
        }
    }
}
