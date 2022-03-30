// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// An Activity is the basic communication type for the Bot Framework protocol.
    /// </summary>
    [DebuggerDisplay("[{Type}] {Text ?? System.String.Empty}")]
    public partial class Activity
    {
        /// <summary>
        /// Gets properties that are not otherwise defined by the <see cref="Activity"/> type but that
        /// might appear in the serialized REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Properties { get; } = new Dictionary<string, JsonElement>();

        /// <summary>
        /// Creates a <see cref="ConversationReference"/> based on this activity.
        /// </summary>
        /// <returns>A conversation reference for the conversation that contains this activity.</returns>
        public ConversationReference GetConversationReference()
        {
            var reference = new ConversationReference
            {
                ActivityId = !string.Equals(Type.ToString(), ActivityTypes.ConversationUpdate.ToString(), StringComparison.OrdinalIgnoreCase) || (!string.Equals(ChannelId, "directline", StringComparison.OrdinalIgnoreCase) && !string.Equals(ChannelId, "webchat", StringComparison.OrdinalIgnoreCase)) ? Id : null,
                User = From,
                Bot = Recipient,
                Conversation = Conversation,
                ChannelId = ChannelId,
                Locale = Locale,
                ServiceUrl = ServiceUrl,
            };

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
        /// <returns>This activity, updated with the delivery information.</returns>
        public Activity ApplyConversationReference(ConversationReference reference, bool isIncoming = false)
        {
            ChannelId = reference.ChannelId;
            ServiceUrl = reference.ServiceUrl;
            Conversation = reference.Conversation;
            Locale = reference.Locale ?? Locale;

            if (isIncoming)
            {
                From = reference.User;
                Recipient = reference.Bot;
                if (reference.ActivityId != null)
                {
                    Id = reference.ActivityId;
                }
            }
            else
            {
                // Outgoing
                From = reference.Bot;
                Recipient = reference.User;
                if (reference.ActivityId != null)
                {
                    ReplyToId = reference.ActivityId;
                }
            }

            return this;
        }

        /// <summary>
        /// Returns this activity if it is <see cref="ActivityTypes.Message"/>; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message activity; or null.</returns>
        public Activity AsMessageActivity()
        {
            return IsActivity(ActivityTypes.Message.ToString()) ? this : null;
        }

        /// <summary>
        /// Returns this activity if it is <see cref="ActivityTypes.MessageDelete"/>; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message delete request; or null.</returns>
        public Activity AsMessageDeleteActivity()
        {
            return IsActivity(ActivityTypes.MessageDelete.ToString()) ? this : null;
        }

        /// <summary>
        /// Returns this activity if it is <see cref="ActivityTypes.Trace"/>; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message activity; or null.</returns>
        public Activity AsTraceActivity()
        {
            return IsActivity(ActivityTypes.Trace.ToString()) ? this : null;
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
        public Activity CreateTrace(string name, object value = null, string valueType = null, [CallerMemberName] string label = null)
        {
            var reply = new Activity
            {
                Type = ActivityTypes.Trace,
                Timestamp = DateTime.UtcNow,
                From = new ChannelAccount { Id = Recipient?.Id, Name = Recipient?.Name },
                Recipient = new ChannelAccount { Id = From?.Id, Name = From?.Name },
                ReplyToId = !string.Equals(Type.ToString(), ActivityTypes.ConversationUpdate.ToString(), StringComparison.OrdinalIgnoreCase) || (!string.Equals(ChannelId, "directline", StringComparison.OrdinalIgnoreCase) && !string.Equals(ChannelId, "webchat", StringComparison.OrdinalIgnoreCase)) ? Id : null,
                ServiceUrl = ServiceUrl,
                ChannelId = ChannelId,
                Conversation = Conversation,
                Name = name,
                Label = label,
                ValueType = valueType ?? value?.GetType().Name,
                Value = value,
            }.AsTraceActivity();
            return reply;
        }

        /// <summary>
        /// Determine if the Activity was sent via an Http/Https connection or Streaming.
        /// This can be determined by looking at the ServiceUrl property:
        /// (1) All channels that send messages via http/https are not streaming
        /// (2) Channels that send messages via streaming have a ServiceUrl that does not begin with http/https.
        /// </summary>
        /// <returns>True if the Activity is originated from a streaming connection.</returns>
        public bool IsFromStreamingConnection()
        {
            var isHttp = ServiceUrl?.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);
            return isHttp.HasValue && !isHttp.Value;
        }

        /// <summary>
        /// Remove recipient mention text from Text property.
        /// Use with caution because this function is altering the text on the Activity.
        /// </summary>
        /// <returns>new .Text property value.</returns>
        public string RemoveRecipientMention()
        {
            return RemoveMentionText(Recipient.Id);
        }

        /// <summary>
        /// Remove any mention text for given id from the Activity.Text property.  For example, given the message
        /// @echoBot Hi Bot, this will remove "@echoBot", leaving "Hi Bot".
        /// </summary>
        /// <description>
        /// Typically this would be used to remove the mention text for the target recipient (the bot usually), though
        /// it could be called for each member.  For example:
        ///    turnContext.Activity.RemoveMentionText(turnContext.Activity.Recipient.Id);
        /// The format of a mention Activity.Entity is dependent on the Channel.  But in all cases we
        /// expect the Mention.Text to contain the exact text for the user as it appears in
        /// Activity.Text.
        /// For example, Teams uses &lt;at&gt;username&lt;/at&gt;, whereas slack use @username. It
        /// is expected that text is in Activity.Text and this method will remove that value from
        /// Activity.Text.
        /// </description>
        /// <param name="id">id to match.</param>
        /// <returns>new Activity.Text property value.</returns>
        public string RemoveMentionText(string id)
        {
            foreach (var mention in GetMentions().Where(mention => mention.Mentioned.Id == id))
            {
                if (mention.Text == null)
                {
                    Text = Regex.Replace(Text, "<at>" + Regex.Escape(mention.Mentioned.Name) + "</at>", string.Empty, RegexOptions.IgnoreCase).Trim();
                }
                else
                {
                    Text = Regex.Replace(Text, Regex.Escape(mention.Text), string.Empty, RegexOptions.IgnoreCase).Trim();
                }
            }

            return Text;
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
            return Entities?.Where(entity => string.Compare(entity.Type, "mention", StringComparison.OrdinalIgnoreCase) == 0)
                .Select(e => e.Properties.ToObject<Mention>()).ToArray() ?? Array.Empty<Mention>();
        }

        /// <summary>
        /// Gets the channel data for this activity as a strongly-typed object.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <returns>The strongly-typed object; or the type's default value, if the <see cref="ChannelData"/> is null.</returns>
        public T GetChannelData<T>()
        {
            if (ChannelData == null)
            {
                return default;
            }

            if (ChannelData.GetType() == typeof(T))
            {
                return (T)ChannelData;
            }

            return ChannelData.ToObject<T>();
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class with type <see cref="ActivityTypes.Message"/>.
        /// </summary>
        /// <returns>The new message activity.</returns>
        public static Activity CreateMessageActivity()
        {
            return new Activity { Type = ActivityTypes.Message };
        }

        /// <summary>
        /// Creates an instance of the <see cref="Activity"/> class with type <see cref="ActivityTypes.Event"/>.
        /// </summary>
        /// <returns>The new event activity.</returns>
        public static Activity CreateEventActivity()
        {
            return new Activity { Type = ActivityTypes.Event };
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

            var type = Type?.ToString();

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
                    result = type[activityType.Length] == '/';
                }
            }

            return result;
        }
    }
}
