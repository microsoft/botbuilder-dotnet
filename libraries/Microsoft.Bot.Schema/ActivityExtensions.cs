// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Helper functions for Message Activities
    /// </summary>
    public static class ActivityExtensions
    {
        /// <summary>
        /// Creates a conversation reference from an activity.
        /// </summary>
        /// <returns>A conversation reference for the conversation that contains the activity.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static ConversationReference GetConversationReference(this Activity activity) =>
            new ConversationReference
            {
                ActivityId = activity.Id,
                User = activity.From,
                Bot = activity.Recipient,
                Conversation = activity.Conversation,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl
            };

        /// <summary>
        /// Updates this activity with the delivery information from an existing 
        /// conversation reference.
        /// </summary>
        /// <param name="reference">The conversation reference.</param>
        /// <param name="isIncoming">(Optional) <c>true</c> to treat the activity as an 
        /// incoming activity, where the bot is the recipient; otherwaire <c>false</c>.
        /// Default is <c>false</c>, and the activity will show the bot as the sender.</param>
        /// <remarks>Call <see cref="GetConversationReference()"/> on an incoming
        /// activity to get a conversation reference that you can then use to update an
        /// outgoing activity with the correct delivery information.
        /// </remarks>
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
                    activity.Id = reference.ActivityId;
            }
            else  // Outgoing
            {
                activity.From = reference.Bot;
                activity.Recipient = reference.User;
                if (reference.ActivityId != null)
                    activity.ReplyToId = reference.ActivityId;
            }

            return activity;
        }

        /// <summary>
        /// Creates a reply message to this message and set up the routing information 
        /// as a reply to the source message.
        /// </summary>
        /// <param name="text">The text of the reply.</param>
        /// <param name="locale">The language code for the <paramref name="text"/>.</param>
        /// <returns>A <see cref="MessageActivity">message activity</see> set up to route back to the sender.</returns>
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
                Text = text ?? String.Empty,
                Locale = locale
            };

        /// <summary>
        /// Gets the channel data as a strongly-typed object.
        /// </summary>
        /// <typeparam name="TypeT">The type of the object to return.</typeparam>
        /// <returns>The strongly-typed object; or the type's default value, if the <see cref="ChannelData"/> is null.</returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="TryGetChannelData{TypeT}(out TypeT)"/>
        public static TypeT GetChannelData<TypeT>(this Activity activity)
        {
            if (activity.ChannelData == null)
                return default(TypeT);
            if (activity.ChannelData.GetType() == typeof(TypeT))
                return (TypeT)activity.ChannelData;

            return ((JObject)activity.ChannelData).ToObject<TypeT>();
        }

        /// <summary>
        /// Gets the channel data as a strongly-typed object.. A return value idicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="TypeT">The type of the object to return.</typeparam>
        /// <param name="instance">When this method returns, contains the strongly-typed object if the operation succeeded,
        /// or the type's default value if the operation failed.</param>
        /// <returns>
        /// <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="GetChannelData{TypeT}"/>
        public static bool TryGetChannelData<TypeT>(this Activity activity, out TypeT instance)
        {
            instance = default(TypeT);

            if (activity.ChannelData == null)
                return false;

            try
            {
                instance = activity.GetChannelData<TypeT>();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Is there a mention of Id in the Text Property 
        /// </summary>
        /// <param name="id">ChannelAccount.Id</param>
        /// <param name="activity"></param>
        /// <returns>true if this id is mentioned in the text</returns>
        public static bool MentionsId(this MessageActivity activity, string id) =>
            activity.GetMentions().Where(mention => mention.Mentioned.Id == id).Any();

        /// <summary>
        /// Is there a mention of Recipient.Id in the Text Property 
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>true if this id is mentioned in the text</returns>
        public static bool MentionsRecipient(this MessageActivity activity) => activity.GetMentions().Where(mention => mention.Mentioned.Id == activity.Recipient.Id).Any();

        /// <summary>
        /// Remove recipient mention text from Text property
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>new .Text property value</returns>
        public static string RemoveRecipientMention(this MessageActivity activity) =>
            activity.RemoveMentionText(activity.Recipient.Id);

        /// <summary>
        /// Replace any mention text for given id from Text property
        /// </summary>
        /// <param name="id">id to match</param>
        /// <param name="activity"></param>
        /// <returns>new .Text property value</returns>
        public static string RemoveMentionText(this MessageActivity activity, string id)
        {
            foreach (var mention in activity.GetMentions().Where(mention => mention.Mentioned.Id == id))
            {
                activity.Text = Regex.Replace(activity.Text, mention.Text, string.Empty, RegexOptions.IgnoreCase);
            }

            return activity.Text;
        }

        /// <summary>
        /// Creates a trace activity.
        /// </summary>
        /// <param name="name">The value to assign to the new activity's <see cref="Activity.Name"/> property.</param>
        /// <param name="value">The value to assign to the new activity's <see cref="Activity.Value"/> property.</param>
        /// <param name="valueType">The value to assign to the new activity's <see cref="Activity.ValueType"/> property.
        /// Default is the type name of the <paramref name="value"/> parameter.</param>
        /// <param name="label">The value to assign to the new activity's <see cref="Activity.Label"/> property.</param>
        public static TraceActivity CreateTraceActivity(string name, string valueType = null, object value = null, [CallerMemberName] string label = null) =>
            new TraceActivity
            {
                Name = name,
                Label = label,
                ValueType = valueType ?? value?.GetType().Name,
                Value = value
            };

        /// <summary>
        /// Creates a trace activity based on this activity.
        /// </summary>
        /// <param name="name">The value to assign to the trace activity's <see cref="Activity.Name"/> property.</param>
        /// <param name="value">The value to assign to the trace activity's <see cref="Activity.Value"/> property.</param>
        /// <param name="valueType">The value to assign to the trace activity's <see cref="Activity.ValueType"/> property.</param>
        /// <param name="label">The value to assign to the trace activity's <see cref="Activity.Label"/> property.</param>
        /// <returns>The created trace activity.</returns>
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
                Value = value
            };
    }
}
