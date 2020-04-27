// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    [DebuggerDisplay("[{Type}] {Text ?? System.String.Empty}")]
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
        ITraceActivity,
        IHandoffActivity
    {
        /// <summary>
        /// The HTTP <c>Content-Type</c> entity header that identifies an <see cref="Activity"/> media type resource.
        /// </summary>
        /// <remarks>In multi-part HTTP content, this header identifies the activity portion of the content.</remarks>
        public const string ContentType = "application/vnd.microsoft.activity";

        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="Activity"/> type but that
        /// might appear in the serialized REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="IMessageActivity"/> object.
        /// </summary>
        /// <returns>The new message activity.</returns>
        public static IMessageActivity CreateMessageActivity()
        {
            return new Activity(ActivityTypes.Message)
            {
                Attachments = new List<Attachment>(),
                Entities = new List<Entity>(),
            };
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="IContactRelationUpdateActivity"/> object.
        /// </summary>
        /// <returns>The new contact relation update activity.</returns>
        public static IContactRelationUpdateActivity CreateContactRelationUpdateActivity()
        {
            return new Activity(ActivityTypes.ContactRelationUpdate);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="IConversationUpdateActivity"/> object.
        /// </summary>
        /// <returns>The new conversation update activity.</returns>
        public static IConversationUpdateActivity CreateConversationUpdateActivity()
        {
            return new Activity(ActivityTypes.ConversationUpdate)
            {
                MembersAdded = new List<ChannelAccount>(),
                MembersRemoved = new List<ChannelAccount>(),
            };
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="ITypingActivity"/> object.
        /// </summary>
        /// <returns>The new typing activity.</returns>
        public static ITypingActivity CreateTypingActivity()
        {
            return new Activity(ActivityTypes.Typing);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="IHandoffActivity"/> object.
        /// </summary>
        /// <returns>The new handoff activity.</returns>
        public static IHandoffActivity CreateHandoffActivity()
        {
            return new Activity(ActivityTypes.Handoff);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="IEndOfConversationActivity"/> object.
        /// </summary>
        /// <returns>The new end of conversation activity.</returns>
        public static IEndOfConversationActivity CreateEndOfConversationActivity()
        {
            return new Activity(ActivityTypes.EndOfConversation);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="IEventActivity"/> object.
        /// </summary>
        /// <returns>The new event activity.</returns>
        public static IEventActivity CreateEventActivity()
        {
            return new Activity(ActivityTypes.Event);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="IInvokeActivity"/> object.
        /// </summary>
        /// <returns>The new invoke activity.</returns>
        public static IInvokeActivity CreateInvokeActivity()
        {
            return new Activity(ActivityTypes.Invoke);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class as an <see cref="ITraceActivity"/> object.
        /// </summary>
        /// <param name="name">The name of the trace operation to create.</param>
        /// <param name="valueType">Optional, identifier for the format of the <paramref name="value"/>.
        /// Default is the name of type of the <paramref name="value"/>.</param>
        /// <param name="value">Optional, the content for this trace operation.</param>
        /// <param name="label">Optional, a descriptive label for this trace operation.</param>
        /// <returns>The new trace activity.</returns>
        public static ITraceActivity CreateTraceActivity(string name, string valueType = null, object value = null, [CallerMemberName] string label = null)
        {
            return new Activity(ActivityTypes.Trace)
            {
                Name = name,
                Label = label,
                ValueType = valueType ?? value?.GetType().Name,
                Value = value,
            };
        }

        /// <summary>
        /// Creates a new message activity as a response to this activity.
        /// </summary>
        /// <param name="text">The text of the reply.</param>
        /// <param name="locale">The language code for the <paramref name="text"/>.</param>
        /// <returns>The new message activity.</returns>
        /// <remarks>The new activity sets up routing information based on this activity.</remarks>
        public Activity CreateReply(string text = null, string locale = null)
        {
            var reply = new Activity
            {
                Type = ActivityTypes.Message,
                Timestamp = DateTime.UtcNow,
                From = new ChannelAccount(id: this.Recipient?.Id, name: this.Recipient?.Name),
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
        /// Creates a new trace activity based on this activity.
        /// </summary>
        /// <param name="name">The name of the trace operation to create.</param>
        /// <param name="value">Optional, the content for this trace operation.</param>
        /// <param name="valueType">Optional, identifier for the format of the <paramref name="value"/>.
        /// Default is the name of type of the <paramref name="value"/>.</param>
        /// <param name="label">Optional, a descriptive label for this trace operation.</param>
        /// <returns>The new trace activity.</returns>
        public ITraceActivity CreateTrace(string name, object value = null, string valueType = null, [CallerMemberName] string label = null)
        {
            var reply = new Activity
            {
                Type = ActivityTypes.Trace,
                Timestamp = DateTime.UtcNow,
                From = new ChannelAccount(id: this.Recipient?.Id, name: this.Recipient?.Name),
                Recipient = new ChannelAccount(id: this.From?.Id, name: this.From?.Name),
                ReplyToId = this.Id,
                ServiceUrl = this.ServiceUrl,
                ChannelId = this.ChannelId,
                Conversation = this.Conversation,
                Name = name,
                Label = label,
                ValueType = valueType ?? value?.GetType().Name,
                Value = value,
            }.AsTraceActivity();
            return reply;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IMessageActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message activity; or null.</returns>
        public IMessageActivity AsMessageActivity()
        {
            return IsActivity(ActivityTypes.Message) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IContactRelationUpdateActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a contact relation update activity; or null.</returns>
        public IContactRelationUpdateActivity AsContactRelationUpdateActivity()
        {
            return IsActivity(ActivityTypes.ContactRelationUpdate) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IInstallationUpdateActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as an installation update activity; or null.</returns>
        public IInstallationUpdateActivity AsInstallationUpdateActivity()
        {
            return IsActivity(ActivityTypes.InstallationUpdate) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IConversationUpdateActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a conversation update activity; or null.</returns>
        public IConversationUpdateActivity AsConversationUpdateActivity()
        {
            return IsActivity(ActivityTypes.ConversationUpdate) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="ITypingActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a typing activity; or null.</returns>
        public ITypingActivity AsTypingActivity()
        {
            return IsActivity(ActivityTypes.Typing) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IEndOfConversationActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as an end of conversation activity; or null.</returns>
        public IEndOfConversationActivity AsEndOfConversationActivity()
        {
            return IsActivity(ActivityTypes.EndOfConversation) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IEventActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as an event activity; or null.</returns>
        public IEventActivity AsEventActivity()
        {
            return IsActivity(ActivityTypes.Event) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IInvokeActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as an invoke activity; or null.</returns>
        public IInvokeActivity AsInvokeActivity()
        {
            return IsActivity(ActivityTypes.Invoke) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IMessageUpdateActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message update request; or null.</returns>
        public IMessageUpdateActivity AsMessageUpdateActivity()
        {
            return IsActivity(ActivityTypes.MessageUpdate) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IMessageDeleteActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message delete request; or null.</returns>
        public IMessageDeleteActivity AsMessageDeleteActivity()
        {
            return IsActivity(ActivityTypes.MessageDelete) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IMessageReactionActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message reaction activity; or null.</returns>
        public IMessageReactionActivity AsMessageReactionActivity()
        {
            return IsActivity(ActivityTypes.MessageReaction) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="ISuggestionActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a suggestion activity; or null.</returns>
        public ISuggestionActivity AsSuggestionActivity()
        {
            return IsActivity(ActivityTypes.Suggestion) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="ITraceActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a trace activity; or null.</returns>
        public ITraceActivity AsTraceActivity()
        {
            return IsActivity(ActivityTypes.Trace) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="IHandoffActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a handoff activity; or null.</returns>
        public IHandoffActivity AsHandoffActivity()
        {
            return IsActivity(ActivityTypes.Handoff) ? this : null;
        }

        /// <summary>
        /// Indicates whether this activity has content.
        /// </summary>
        /// <returns>True, if this activity has any content to send; otherwise, false.</returns>
        /// <remarks>This method is defined on the <see cref="Activity"/> class, but is only intended
        /// for use with a message activity, where the activity <see cref="Activity.Type"/> is set to
        /// <see cref="ActivityTypes.Message"/>.</remarks>
        public bool HasContent()
        {
            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(this.Summary))
            {
                return true;
            }

            if (this.Attachments != null && this.Attachments.Any())
            {
                return true;
            }

            if (this.ChannelData != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resolves the mentions from the entities of this activity.
        /// </summary>
        /// <returns>The array of mentions; or an empty array, if none are found.</returns>
        /// <remarks>This method is defined on the <see cref="Activity"/> class, but is only intended
        /// for use with a message activity, where the activity <see cref="Activity.Type"/> is set to
        /// <see cref="ActivityTypes.Message"/>.</remarks>
        /// <seealso cref="Entities"/>
        /// <seealso cref="Mention"/>
        public Mention[] GetMentions()
        {
            return this.Entities?.Where(entity => string.Compare(entity.Type, "mention", ignoreCase: true) == 0)
                .Select(e => e.Properties.ToObject<Mention>()).ToArray() ?? new Mention[0];
        }

        /// <summary>
        /// Gets the channel data for this activity as a strongly-typed object.
        /// </summary>
        /// <typeparam name="TypeT">The type of the object to return.</typeparam>
        /// <returns>The strongly-typed object; or the type's default value, if the <see cref="ChannelData"/> is null.</returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="TryGetChannelData{TypeT}(out TypeT)"/>
        public TypeT GetChannelData<TypeT>()
        {
            if (this.ChannelData == null)
            {
                return default(TypeT);
            }

            if (this.ChannelData.GetType() == typeof(TypeT))
            {
                return (TypeT)this.ChannelData;
            }

            return ((JObject)this.ChannelData).ToObject<TypeT>();
        }

        /// <summary>
        /// Gets the channel data for this activity as a strongly-typed object.
        /// A return value idicates whether the operation succeeded.
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
                {
                    return false;
                }

                instance = this.GetChannelData<TypeT>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a <see cref="ConversationReference"/> based on this activity.
        /// </summary>
        /// <returns>A conversation reference for the conversation that contains this activity.</returns>
        public ConversationReference GetConversationReference()
        {
            ConversationReference reference = new ConversationReference
            {
                ActivityId = this.Id,
                User = this.From,
                Bot = this.Recipient,
                Conversation = this.Conversation,
                ChannelId = this.ChannelId,
                Locale = this.Locale,
                ServiceUrl = this.ServiceUrl,
            };

            return reference;
        }

        /// <summary>
        /// Create a ConversationReference based on this Activity's Conversation info and the ResourceResponse from sending an activity.
        /// </summary>
        /// <param name="reply">ResourceResponse returned from sendActivity.</param>
        /// <returns>A ConversationReference that can be stored and used later to delete or update the activity.</returns>
        public ConversationReference GetReplyConversationReference(ResourceResponse reply)
        {
            var reference = GetConversationReference();

            // Update the reference with the new outgoing Activity's id.
            reference.ActivityId = reply.Id;
            return reference;
        }

        /// <summary>
        /// Updates this activity with the delivery information from an existing <see cref="ConversationReference"/>.
        /// </summary>
        /// <param name="reference">The existing conversation reference.</param>
        /// <param name="isIncoming">Optional, <c>true</c> to treat the activity as an
        /// incoming activity, where the bot is the recipient; otherwise, <c>false</c>.
        /// Default is <c>false</c>, and the activity will show the bot as the sender.</param>
        /// <remarks>Call <see cref="GetConversationReference()"/> on an incoming
        /// activity to get a conversation reference that you can then use to update an
        /// outgoing activity with the correct delivery information.
        /// </remarks>
        /// <returns>This activy, updated with the delivery information.</returns>
        public Activity ApplyConversationReference(ConversationReference reference, bool isIncoming = false)
        {
            this.ChannelId = reference.ChannelId;
            this.ServiceUrl = reference.ServiceUrl;
            this.Conversation = reference.Conversation;
            this.Locale = reference.Locale;

            if (isIncoming)
            {
                this.From = reference.User;
                this.Recipient = reference.Bot;
                if (reference.ActivityId != null)
                {
                    this.Id = reference.ActivityId;
                }
            }
            else
            {// Outgoing
                this.From = reference.Bot;
                this.Recipient = reference.User;
                if (reference.ActivityId != null)
                {
                    this.ReplyToId = reference.ActivityId;
                }
            }

            return this;
        }

        /// <summary>
        /// Determine if the Activity was sent via an Http/Https connection or Streaming
        /// This can be determined by looking at the ServiceUrl property:
        /// (1) All channels that send messages via http/https are not streaming
        /// (2) Channels that send messages via streaming have a ServiceUrl that does not begin with http/https.
        /// </summary>
        /// <returns>True if the Activity originated from a streaming connection.</returns>
        public bool IsFromStreamingConnection()
        {
            var isHttp = ServiceUrl?.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);
            return isHttp.HasValue ? !isHttp.Value : false;
        }

        /// <summary>
        /// Indicates whether this activity is of a specified activity type.
        /// </summary>
        /// <param name="activityType">The activity type to check for.</param>
        /// <returns><c>true</c> if this activity is of the specified activity type; otherwise, <c>false</c>.</returns>
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

        partial void CustomInit()
        {
        }
    }
}
