// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// An Activity is the basic communication type for the Bot Framework 3.0 protocol.
    /// </summary>
    /// <remarks>
    /// The Activity class contains all properties that individual, more specific activities
    /// could contain. It is a superset type.
    /// </remarks>
    public partial class Activity :
        IActivity,
        IConversationUpdateActivity,
        IContactRelationUpdateActivity,
        IInstallationUpdateActivity,
        IMessageActivity,
        IMessageUpdateActivity,
        IMessageDeleteActivity,
        IMessageReactionActivity,
        ISuggestionActivity,
        ITypingActivity,
        IEndOfConversationActivity,
        IEventActivity,
        IInvokeActivity,
        ITraceActivity
    {
        /// <summary>
        /// Content-type for an <see cref="Activity"/>.
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.activity";

        partial void CustomInit()
        {
        }

        /// <summary>
        /// Creates a reply message to this message and set up the routing information 
        /// as a reply to the source message.
        /// </summary>
        /// <param name="text">The text of the reply.</param>
        /// <param name="locale">The language code for the <paramref name="text"/>.</param>
        /// <returns>A message activity set up to route back to the sender.</returns>
        public Activity CreateReply(string text = null, string locale = null)
        {
            var reply = new Activity
            {
                Type = ActivityTypes.Message,
                Timestamp = DateTime.UtcNow,
                From = new ChannelAccount(id: this.Recipient.Id, name: this.Recipient.Name),
                Recipient = new ChannelAccount(id: this.From.Id, name: this.From.Name),
                ReplyToId = this.Id,
                ServiceUrl = this.ServiceUrl,
                ChannelId = this.ChannelId,
                Conversation = new ConversationAccount(isGroup: this.Conversation.IsGroup, id: this.Conversation.Id, name: this.Conversation.Name),
                Text = text ?? string.Empty,
                Locale = locale ?? this.Locale,
                Attachments = new List<Attachment>(),
                Entities = new List<Entity>(),
            };
            return reply;
        }

        /// <summary>
        /// Creates a trace activity based on this activity.
        /// </summary>
        /// <param name="name">The value to assign to the trace activity's <see cref="Activity.Name"/> property.</param>
        /// <param name="value">The value to assign to the trace activity's <see cref="Activity.Value"/> property.</param>
        /// <param name="valueType">The value to assign to the trace activity's <see cref="Activity.ValueType"/> property.</param>
        /// <param name="label">The value to assign to the trace activity's <see cref="Activity.Label"/> property.</param>
        /// <returns>The created trace activity.</returns>
        public ITraceActivity CreateTrace(string name, object value=null, string valueType = null, [CallerMemberName] string label=null)
        {
            var reply = new Activity
            {
                Type = ActivityTypes.Trace,
                Timestamp = DateTime.UtcNow,
                From = new ChannelAccount(id: this.Recipient.Id, name: this.Recipient.Name),
                Recipient = new ChannelAccount(id: this.From.Id, name: this.From.Name),
                ReplyToId = this.Id,
                ServiceUrl = this.ServiceUrl,
                ChannelId = this.ChannelId,
                Conversation = new ConversationAccount(isGroup: this.Conversation.IsGroup, id: this.Conversation.Id, name: this.Conversation.Name),
                Name = name,
                Label = label, 
                ValueType = valueType ?? value?.GetType().Name,
                Value = value
            }.AsTraceActivity();
            return reply;
        }

        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="Activity"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();

        /// <summary>
        /// Create an instance of the Activity class with IMessageActivity masking
        /// </summary>
        public static IMessageActivity CreateMessageActivity()
        {
            return new Activity(ActivityTypes.Message)
            {
                Attachments = new List<Attachment>(),
                Entities = new List<Entity>(),
            };
        }

        /// <summary>
        /// Create an instance of the Activity class with IContactRelationUpdateActivity masking
        /// </summary>
        public static IContactRelationUpdateActivity CreateContactRelationUpdateActivity() { return new Activity(ActivityTypes.ContactRelationUpdate); }

        /// <summary>
        /// Creates a conversation update activity.
        /// </summary>
        public static IConversationUpdateActivity CreateConversationUpdateActivity()
        {
            return new Activity(ActivityTypes.ConversationUpdate)
            {
                MembersAdded = new List<ChannelAccount>(),
                MembersRemoved = new List<ChannelAccount>(),
            };
        }

        /// <summary>
        /// Creates a typing activity.
        /// </summary>
        public static ITypingActivity CreateTypingActivity() { return new Activity(ActivityTypes.Typing); }

        /// <summary>
        /// Create a ping activity.
        /// </summary>
        public static IActivity CreatePingActivity() { return new Activity(ActivityTypes.Ping); }

        /// <summary>
        /// Creates an end of conversation activity.
        /// </summary>
        public static IEndOfConversationActivity CreateEndOfConversationActivity() { return new Activity(ActivityTypes.EndOfConversation); }

        /// <summary>
        /// Creates an event activity.
        /// </summary>
        public static IEventActivity CreateEventActivity() { return new Activity(ActivityTypes.Event); }

        /// <summary>
        /// Creates an invoke activity.
        /// </summary>
        public static IInvokeActivity CreateInvokeActivity() { return new Activity(ActivityTypes.Invoke); }

        /// <summary>
        /// Creates a trace activity.
        /// </summary>
        /// <param name="name">The value to assign to the new activity's <see cref="Activity.Name"/> property.</param>
        /// <param name="value">The value to assign to the new activity's <see cref="Activity.Value"/> property.</param>
        /// <param name="valueType">The value to assign to the new activity's <see cref="Activity.ValueType"/> property.
        /// Default is the type name of the <paramref name="value"/> parameter.</param>
        /// <param name="label">The value to assign to the new activity's <see cref="Activity.Label"/> property.</param>
        public static ITraceActivity CreateTraceActivity(string name, string valueType = null, object value = null, [CallerMemberName] string label=null)
        {
            return new Activity(ActivityTypes.Trace)
            {
                Name = name,
                Label = label,
                ValueType = valueType ?? value?.GetType().Name,
                Value = value
            };
        }

        /// <summary>
        /// Indicates whether this activity is of a specified activity type.
        /// </summary>
        /// <param name="activityType">The activity type to check for.</param>
        protected bool IsActivity(string activityType)
        {
            /*
             * NOTE: While it is possible to come up with a fancy looking "one-liner" to solve 
             * this problem, this code is purposefully more verbose due to optimizations. 
             * 
             * This main goal of the optimizations was to make zero allocations because it is called 
             * by all of the .AsXXXActivity methods which are used in a pattern heavily upstream to 
             * "pseudo-cast" the activity based on its type.
             */

            var type = this.Type;

            // If there's no type set then we can't tell if it's the type they're looking for
            if (type == null)
            {
                return false;
            }

            // Check if the full type value starts with the type they're looking for
            var result = type.StartsWith(activityType, StringComparison.OrdinalIgnoreCase);

            // If the full type value starts with the type they're looking for, then we need to check a little further to check if it's definitely the right type
            if (result)
            {
                // If the lengths are equal, then it's the exact type they're looking for
                result = type.Length == activityType.Length;

                if (!result)
                {
                    // Finally, if the type is longer than the type they're looking for then we need to check if there's a / separator right after the type they're looking for
                    result = type.Length > activityType.Length
                                    &&
                            type[activityType.Length] == '/';
                }
            }

            return result;
        }

        /// <summary>
        /// Returns this activity as a message activty type; or null, if this is not that type of activity.
        /// </summary>
        public IMessageActivity AsMessageActivity() { return IsActivity(ActivityTypes.Message) ? this : null; }

        /// <summary>
        /// Returns this activity as a contact relation update activty type; or null, if this is not that type of activity.
        /// </summary>
        public IContactRelationUpdateActivity AsContactRelationUpdateActivity() { return IsActivity(ActivityTypes.ContactRelationUpdate) ? this : null; }

        /// <summary>
        /// Returns this activity as an installation update activty type; or null, if this is not that type of activity.
        /// </summary>
        public IInstallationUpdateActivity AsInstallationUpdateActivity() { return IsActivity(ActivityTypes.InstallationUpdate) ? this : null; }

        /// <summary>
        /// Returns this activity as a conversation update activty type; or null, if this is not that type of activity.
        /// </summary>
        public IConversationUpdateActivity AsConversationUpdateActivity() { return IsActivity(ActivityTypes.ConversationUpdate) ? this : null; }

        /// <summary>
        /// Returns this activity as a typing activty type; or null, if this is not that type of activity.
        /// </summary>
        public ITypingActivity AsTypingActivity() { return IsActivity(ActivityTypes.Typing) ? this : null; }

        /// <summary>
        /// Returns this activity as an end of conversation activty type; or null, if this is not that type of activity.
        /// </summary>
        public IEndOfConversationActivity AsEndOfConversationActivity() { return IsActivity(ActivityTypes.EndOfConversation) ? this : null; }

        /// <summary>
        /// Returns this activity as an event activty type; or null, if this is not that type of activity.
        /// </summary>
        public IEventActivity AsEventActivity() { return IsActivity(ActivityTypes.Event) ? this : null; }

        /// <summary>
        /// Returns this activity as an invoke activty type; or null, if this is not that type of activity.
        /// </summary>
        public IInvokeActivity AsInvokeActivity() { return IsActivity(ActivityTypes.Invoke) ? this : null; }

        /// <summary>
        /// Returns this activity as a message update activty type; or null, if this is not that type of activity.
        /// </summary>
        /// <returns></returns>
        public IMessageUpdateActivity AsMessageUpdateActivity() { return IsActivity(ActivityTypes.MessageUpdate) ? this : null; }

        /// <summary>
        /// Returns this activity as a message delete activty type; or null, if this is not that type of activity.
        /// </summary>
        /// <returns></returns>
        public IMessageDeleteActivity AsMessageDeleteActivity() { return IsActivity(ActivityTypes.MessageDelete) ? this : null; }

        /// <summary>
        /// Returns this activity as a message reaction activty type; or null, if this is not that type of activity.
        /// </summary>
        /// <returns></returns>
        public IMessageReactionActivity AsMessageReactionActivity() { return IsActivity(ActivityTypes.MessageReaction) ? this : null; }

        /// <summary>
        /// Returns this activity as a suggestion activty type; or null, if this is not that type of activity.
        /// </summary>
        /// <returns></returns>
        public ISuggestionActivity AsSuggestionActivity() { return IsActivity(ActivityTypes.Suggestion) ? this : null; }

        /// <summary>
        /// Returns this activity as a trace activty type; or null, if this is not that type of activity.
        /// </summary>
        /// <returns></returns>
        public ITraceActivity AsTraceActivity() { return IsActivity(ActivityTypes.Trace) ? this : null; }

        /// <summary>
        /// Checks whether this message activity has content.
        /// </summary>
        /// <returns>True, if this message activity has any content to send; otherwise, false.</returns>
        /// <remarks>This method is defined on the <see cref="Activity"/> class, but is only intended
        /// for use on an activity of <see cref="Activity.Type"/> <see cref="ActivityTypes.Message"/>.</remarks>
        public bool HasContent()
        {
            if (!string.IsNullOrWhiteSpace(this.Text))
                return true;

            if (!string.IsNullOrWhiteSpace(this.Summary))
                return true;

            if (this.Attachments != null && this.Attachments.Any())
                return true;

            if (this.ChannelData != null)
                return true;

            return false;
        }

        /// <summary>
        /// Resolves the mentions from the entities of this (message) activity.
        /// </summary>
        /// <returns>The array of mentions; or an empty array, if none are found.</returns>
        public Mention[] GetMentions()
        {
            return this.Entities?.Where(entity => string.Compare(entity.Type, "mention", ignoreCase: true) == 0)
                .Select(e => e.Properties.ToObject<Mention>()).ToArray() ?? new Mention[0];
        }

        /// <summary>
        /// Gets the channel data as a strongly-typed object.
        /// </summary>
        /// <typeparam name="TypeT">The type of the object to return.</typeparam>
        /// <returns>The strongly-typed object; or the type's default value, if the <see cref="ChannelData"/> is null.</returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="TryGetChannelData{TypeT}(out TypeT)"/>
        public TypeT GetChannelData<TypeT>()
        {
            if (this.ChannelData == null)
                return default(TypeT);
            if (this.ChannelData.GetType() == typeof(TypeT))
                return (TypeT)this.ChannelData;
            return ((JObject)this.ChannelData).ToObject<TypeT>();
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
        public bool TryGetChannelData<TypeT>(out TypeT instance)
        {
            instance = default(TypeT);

            try
            {
                if (this.ChannelData == null)
                    return false;

                instance = this.GetChannelData<TypeT>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a conversation reference from an activity.
        /// </summary>
        /// <returns>A conversation reference for the conversation that contains the activity.</returns>
        /// <exception cref="ArgumentNullException"/>
        public ConversationReference GetConversationReference()
        {
            ConversationReference reference = new ConversationReference
            {
                ActivityId = this.Id,
                User = this.From,
                Bot = this.Recipient,
                Conversation = this.Conversation,
                ChannelId = this.ChannelId,
                ServiceUrl = this.ServiceUrl
            };

            return reference;
        }

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
        public Activity ApplyConversationReference(ConversationReference reference, bool isIncoming = false)
        {
            this.ChannelId = reference.ChannelId;
            this.ServiceUrl = reference.ServiceUrl;
            this.Conversation = reference.Conversation;

            if (isIncoming)
            {
                this.From = reference.User;
                this.Recipient = reference.Bot;
                if (reference.ActivityId != null)
                    this.Id = reference.ActivityId;
            }
            else  // Outgoing
            {
                this.From = reference.Bot;
                this.Recipient = reference.User;
                if (reference.ActivityId != null)
                    this.ReplyToId = reference.ActivityId;
            }
            return this;
        }

    }
}
