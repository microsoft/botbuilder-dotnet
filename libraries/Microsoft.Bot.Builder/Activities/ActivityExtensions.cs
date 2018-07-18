// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A set of extension methods for <see cref="Activity"/>.
    /// </summary>
    public static class ActivityExtensions
    {
        /// <summary>
        /// Creates a conversation reference from an activity.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to get a <see cref="ConversationReference"/> for.</param>
        /// <returns>A conversation reference for the conversation that contains the activity.</returns>
        public static ConversationReference GetConversationReference(this Activity activity) =>
            new ConversationReference
            {
                ActivityId = activity.Id,
                User = activity.From,
                Bot = activity.Recipient,
                Conversation = activity.Conversation,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
            };

        /// <summary>
        /// Updates this activity with the delivery information from an existing
        /// conversation reference.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to apply the <paramref name="reference"/> to.</param>
        /// <param name="reference">The conversation reference.</param>
        /// <param name="isIncoming">(Optional) <c>true</c> to treat the activity as an 
        /// incoming activity, where the bot is the recipient; otherwaire <c>false</c>.
        /// Default is <c>false</c>, and the activity will show the bot as the sender.</param>
        /// <remarks>Call <see cref="GetConversationReference()"/> on an incoming
        /// activity to get a conversation reference that you can then use to update an
        /// outgoing activity with the correct delivery information.
        /// </remarks>
        /// <returns>The original <paramref name="activity"/> with the <paramref name="reference"/> applied.</returns>
        public static Activity ApplyConversationReference(this Activity activity, ConversationReference reference, bool isIncoming = false)
        {
            activity.ChannelId = reference.ChannelId;
            activity.ServiceUrl = reference.ServiceUrl;
            activity.Conversation = reference.Conversation;

            if (isIncoming)
            {
                activity.From = reference.User;
                activity.Recipient = reference.Bot;
                if (reference.ActivityId != null)
                {
                    activity.Id = reference.ActivityId;
                }
            }
            else
            {
                activity.From = reference.Bot;
                activity.Recipient = reference.User;
                if (reference.ActivityId != null)
                {
                    activity.ReplyToId = reference.ActivityId;
                }
            }

            return activity;
        }

        /// <summary>
        /// Creates a reply <see cref="MessageActivity"/> which is configured to correctly route back to the
        /// source of the original <paramref name="activity"/>.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to create a reply for.</param>
        /// <param name="text">The text of the reply.</param>
        /// <param name="locale">The language code for the <paramref name="text"/>.</param>
        /// <returns>A <see cref="MessageActivity"/> populated to correctly route back to the sender of the original <paramref name="activity"/>.</returns>
        public static MessageActivity CreateReply(this Activity activity, string text = null, string locale = null) =>
            new MessageActivity
            {
                Timestamp = DateTime.UtcNow,
                From = new ChannelAccount(id: activity.Recipient.Id, name: activity.Recipient.Name),
                Recipient = new ChannelAccount(id: activity.From.Id, name: activity.From.Name),
                ReplyToId = activity.Id,
                ServiceUrl = activity.ServiceUrl,
                ChannelId = activity.ChannelId,
                Conversation = new ConversationAccount(isGroup: activity.Conversation.IsGroup, id: activity.Conversation.Id, name: activity.Conversation.Name),
                Text = text ?? string.Empty,
                Locale = locale,
            };

        /// <summary>
        /// Gets the channel data as a strongly-typed object.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="activity">The <see cref="Activity"/> to fetch the channel data from.</param>
        /// <returns>The strongly-typed object; or the type's default value, if the <see cref="ChannelData"/> is null.</returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="TryGetChannelData{TypeT}(out TypeT)"/>
        public static T GetChannelData<T>(this Activity activity)
        {
            var channelData = activity.ChannelData;

            if (channelData == null)
            {
                return default(T);
            }

            if (channelData is T alreadyTypedChannelData)
            {
                return alreadyTypedChannelData;
            }

            return ((JObject)channelData).ToObject<T>();
        }

        /// <summary>
        /// Gets the channel data as a strongly-typed object.. A return value idicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="activity">The <see cref="Activity"/> to fetch the channel data from.</param>
        /// <param name="instance">When this method returns, contains the strongly-typed object if the operation succeeded,
        /// or the type's default value if the operation failed.</param>
        /// <returns>
        /// <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="GetChannelData{TypeT}"/>
        public static bool TryGetChannelData<T>(this Activity activity, out T instance)
        {
            instance = default(T);

            if (activity.ChannelData == null)
            {
                return false;
            }

            try
            {
                instance = activity.GetChannelData<T>();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a trace activity based on this activity.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> whose context should be used to create a <see cref="TraceActivity"/>.</param>
        /// <param name="name">The value to assign to the trace activity's <see cref="Activity.Name"/> property.</param>
        /// <param name="value">The value to assign to the trace activity's <see cref="Activity.Value"/> property.</param>
        /// <param name="valueType">The value to assign to the trace activity's <see cref="Activity.ValueType"/> property.</param>
        /// <param name="label">The value to assign to the trace activity's <see cref="Activity.Label"/> property.</param>
        /// <returns>The created <see cref="TraceActivity"/>.</returns>
        public static TraceActivity CreateTrace(this Activity activity, string name, object value = null, string valueType = null, [CallerMemberName] string label = null) =>
            new TraceActivity
            {
                Timestamp = DateTime.UtcNow,
                From = new ChannelAccount(id: activity.Recipient.Id, name: activity.Recipient.Name),
                Recipient = new ChannelAccount(id: activity.From.Id, name: activity.From.Name),
                ReplyToId = activity.Id,
                ServiceUrl = activity.ServiceUrl,
                ChannelId = activity.ChannelId,
                Conversation = new ConversationAccount(isGroup: activity.Conversation.IsGroup, id: activity.Conversation.Id, name: activity.Conversation.Name),
                Name = name,
                Label = label,
                ValueType = valueType ?? value?.GetType().Name,
                Value = value,
            };
    }
}
