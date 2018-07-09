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
    /// An Activity is the basic communication type for the Bot Framework 3.0 protocol
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
        /// Content-type for an Activity
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.activity";

        partial void CustomInit()
        {
        }

        /// <summary>
        /// Take a message and create a reply message for it with the routing information 
        /// set up to correctly route a reply to the source message
        /// </summary>
        /// <param name="text">text you want to reply with</param>
        /// <param name="locale">language of your reply</param>
        /// <returns>message set up to route back to the sender</returns>
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
                Text = text ?? String.Empty,
                Locale = locale ?? this.Locale,
                Attachments = new List<Attachment>(),
                Entities = new List<Entity>(),
            };
            return reply;
        }

        /// <summary>
        /// Create a trace activity based of this activity
        /// </summary>
        /// <param name="name">Name of the operation</param>
        /// <param name="value">value of the operation</param>
        /// <param name="valueType">valueType if helpful to identify the value schema (default is value.GetType().Name)</param>
        /// <param name="label">descritive label of context. (Default is calling function name)</param>
        /// <returns></returns>
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
        /// Extension data for overflow of properties
        /// </summary>
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
        /// Create an instance of the Activity class with IConversationUpdateActivity masking
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
        /// Create an instance of the Activity class with ITypingActivity masking
        /// </summary>
        public static ITypingActivity CreateTypingActivity() { return new Activity(ActivityTypes.Typing); }

        /// <summary>
        /// Create an instance of the Activity class with IActivity masking
        /// </summary>
        public static IActivity CreatePingActivity() { return new Activity(ActivityTypes.Ping); }

        /// <summary>
        /// Create an instance of the Activity class with IEndOfConversationActivity masking
        /// </summary>
        public static IEndOfConversationActivity CreateEndOfConversationActivity() { return new Activity(ActivityTypes.EndOfConversation); }

        /// <summary>
        /// Create an instance of the Activity class with an IEventActivity masking
        /// </summary>
        public static IEventActivity CreateEventActivity() { return new Activity(ActivityTypes.Event); }

        /// <summary>
        /// Create an instance of the Activity class with IInvokeActivity masking
        /// </summary>
        public static IInvokeActivity CreateInvokeActivity() { return new Activity(ActivityTypes.Invoke); }

        /// <summary>
        /// Create an instance of the TraceActivity 
        /// </summary>
        /// <param name="name">Name of the operation</param>
        /// <param name="value">value of the operation</param>
        /// <param name="valueType">valueType if helpful to identify the value schema (default is value.GetType().Name)</param>
        /// <param name="label">descriptive label of context. (Default is calling function name)</param>
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
        /// True if the Activity is of the specified activity type
        /// </summary>
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
        /// Return an IMessageActivity mask if this is a message activity
        /// </summary>
        public IMessageActivity AsMessageActivity() { return IsActivity(ActivityTypes.Message) ? this : null; }

        /// <summary>
        /// Return an IContactRelationUpdateActivity mask if this is a contact relation update activity
        /// </summary>
        public IContactRelationUpdateActivity AsContactRelationUpdateActivity() { return IsActivity(ActivityTypes.ContactRelationUpdate) ? this : null; }

        /// <summary>
        /// Return an IInstallationUpdateActivity mask if this is a installation update activity
        /// </summary>
        public IInstallationUpdateActivity AsInstallationUpdateActivity() { return IsActivity(ActivityTypes.InstallationUpdate) ? this : null; }

        /// <summary>
        /// Return an IConversationUpdateActivity mask if this is a conversation update activity
        /// </summary>
        public IConversationUpdateActivity AsConversationUpdateActivity() { return IsActivity(ActivityTypes.ConversationUpdate) ? this : null; }

        /// <summary>
        /// Return an ITypingActivity mask if this is a typing activity
        /// </summary>
        public ITypingActivity AsTypingActivity() { return IsActivity(ActivityTypes.Typing) ? this : null; }

        /// <summary>
        /// Return an IEndOfConversationActivity mask if this is an end of conversation activity
        /// </summary>
        public IEndOfConversationActivity AsEndOfConversationActivity() { return IsActivity(ActivityTypes.EndOfConversation) ? this : null; }

        /// <summary>
        /// Return an IEventActivity mask if this is an event activity
        /// </summary>
        public IEventActivity AsEventActivity() { return IsActivity(ActivityTypes.Event) ? this : null; }

        /// <summary>
        /// Return an IInvokeActivity mask if this is an invoke activity
        /// </summary>
        public IInvokeActivity AsInvokeActivity() { return IsActivity(ActivityTypes.Invoke) ? this : null; }

        /// <summary>
        /// Return an IMessageUpdateActivity if this is a MessageUpdate activity
        /// </summary>
        /// <returns></returns>
        public IMessageUpdateActivity AsMessageUpdateActivity() { return IsActivity(ActivityTypes.MessageUpdate) ? this : null; }

        /// <summary>
        /// Return an IMessageDeleteActivity if this is a MessageDelete activity
        /// </summary>
        /// <returns></returns>
        public IMessageDeleteActivity AsMessageDeleteActivity() { return IsActivity(ActivityTypes.MessageDelete) ? this : null; }

        /// <summary>
        /// Return an IMessageReactionActivity if this is a MessageReaction activity
        /// </summary>
        /// <returns></returns>
        public IMessageReactionActivity AsMessageReactionActivity() { return IsActivity(ActivityTypes.MessageReaction) ? this : null; }

        /// <summary>
        /// Return an ISuggestionActivity if this is a Suggestion activity
        /// </summary>
        /// <returns></returns>
        public ISuggestionActivity AsSuggestionActivity() { return IsActivity(ActivityTypes.Suggestion) ? this : null; }

        /// <summary>
        /// Return an ITraceActivity if this is a Trace activity
        /// </summary>
        /// <returns></returns>
        public ITraceActivity AsTraceActivity() { return IsActivity(ActivityTypes.Trace) ? this : null; }

        /// <summary>
        /// Checks if this (message) activity has content.
        /// </summary>
        /// <returns>Returns true, if this message has any content to send. False otherwise.</returns>
        public bool HasContent()
        {
            if (!String.IsNullOrWhiteSpace(this.Text))
                return true;

            if (!String.IsNullOrWhiteSpace(this.Summary))
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
        /// <returns>The array of mentions or an empty array, if none found.</returns>
        public Mention[] GetMentions()
        {
            return this.Entities?.Where(entity => String.Compare(entity.Type, "mention", ignoreCase: true) == 0)
                .Select(e => e.Properties.ToObject<Mention>()).ToArray() ?? new Mention[0];
        }

        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <returns>typed object or default(TypeT)</returns>
        public TypeT GetChannelData<TypeT>()
        {
            if (this.ChannelData == null)
                return default(TypeT);
            if (this.ChannelData.GetType() == typeof(TypeT))
                return (TypeT)this.ChannelData;
            return ((JObject)this.ChannelData).ToObject<TypeT>();
        }

        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <param name="instance">The resulting instance, if possible</param>
        /// <returns>
        /// <c>true</c> if value of <seealso cref="IActivity.ChannelData"/> was coerceable to <typeparamref name="TypeT"/>, <c>false</c> otherwise.
        /// </returns>
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
