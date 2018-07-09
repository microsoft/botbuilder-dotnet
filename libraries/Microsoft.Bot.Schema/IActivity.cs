// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Shared properties for all activities
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// Activity type
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// ID for the activity
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Service URL where responses to this activity should be sent
        /// </summary>
        string ServiceUrl { get; set; }

        /// <summary>
        /// Timestamp when this message was sent (UTC)
        /// </summary>
        DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Client time when message was sent (local time or UTC)
        /// </summary>
        DateTimeOffset? LocalTimestamp { get; set; }

        /// <summary>
        /// Channel this activity is associated with
        /// </summary>
        string ChannelId { get; set; }

        /// <summary>
        /// Sender address
        /// </summary>
        ChannelAccount From { get; set; }

        /// <summary>
        /// Address for the conversation that this activity is associated with
        /// </summary>
        ConversationAccount Conversation { get; set; }
        
        /// <summary>
        /// Address that received the message
        /// </summary>
        ChannelAccount Recipient { get; set; }

        /// <summary>
        /// The original ID this activity is a response to
        /// </summary>
        string ReplyToId { get; set; }

        /// <summary>
        /// Collection of Entity objects, each of which contains metadata about this activity. Each Entity object is typed.
        /// </summary>
        IList<Entity> Entities { get; set; }

        /// <summary>
        /// Channel-specific payload
        /// </summary>
        /// <remarks>
        /// Some channels will provide channel specific data.
        /// 
        /// For a message originating in the channel it might provide the original native schema object for the channel. 
        /// 
        /// For a message coming into the channel it might accept a payload allowing you to create a "native" response for the channel.
        /// 
        /// Example:
        /// * Email - The Email Channel will put the original Email metadata into the ChannelData object for outgoing messages, and will accept
        /// on incoming message a Subject property, and a HtmlBody which can contain Html.  
        /// 
        /// The channel data essentially allows a bot to have access to native functionality on a per channel basis.
        /// </remarks>
        dynamic ChannelData { get; set; }

        /// <summary>
        /// Gets the channel data as strongly typed object.
        /// </summary>
        /// <typeparam name="TypeT">The expected type of the object.</typeparam>
        /// <returns>The strongly typed channel data.</returns>
        TypeT GetChannelData<TypeT>();

        /// <summary>
        /// Try to get the channeldata as a strongly typed object 
        /// </summary>
        /// <typeparam name="TypeT"></typeparam>
        /// <param name="instance"></param>
        /// <returns>false if there is no valid channeldata available</returns>
        bool TryGetChannelData<TypeT>(out TypeT instance);

        /// <summary>
        /// Return IMessageActivity if this is a message activity, null otherwise
        /// </summary>
        IMessageActivity AsMessageActivity();

        /// <summary>
        /// Return IContactRelationUpdateActivity if this is a contactRelationUpdate activity, null otherwise
        /// </summary>
        IContactRelationUpdateActivity AsContactRelationUpdateActivity();

        /// <summary>
        /// Return IInstallationUpdateActivity if this is a installationUpdate activity, null otherwise
        /// </summary>
        IInstallationUpdateActivity AsInstallationUpdateActivity();

        /// <summary>
        /// Return IConversationUpdateActivity if this is a conversationUpdate activity, null otherwise
        /// </summary>
        IConversationUpdateActivity AsConversationUpdateActivity();

        /// <summary>
        /// Return ITypingActivity if this is a typing activity, null otherwise
        /// </summary>
        ITypingActivity AsTypingActivity();

        /// <summary>
        /// Return IEndOfConversationActivity if this is an end-of-conversation activity, null otherwise
        /// </summary>
        IEndOfConversationActivity AsEndOfConversationActivity();

        /// <summary>
        /// Returns IEventActivity if this is an event activity, null otherwise
        /// </summary>
        IEventActivity AsEventActivity();

        /// <summary>
        /// Returns IInvokeActivity if this is an invoke activity, null otherwise
        /// </summary>
        IInvokeActivity AsInvokeActivity();

        /// <summary>
        /// Returns IMessageUpdateActivity if this is a message update activity, null otherwise
        /// </summary>
        IMessageUpdateActivity AsMessageUpdateActivity();

        /// <summary>
        /// Returns IMessageDeleteActivity if this is a message delete activity, null otherwise
        /// </summary>
        IMessageDeleteActivity AsMessageDeleteActivity();

        /// <summary>
        /// Returns IMessageDeleteActivity if this is a message delete activity, null otherwise
        /// </summary>
        IMessageReactionActivity AsMessageReactionActivity();

        /// <summary>
        /// Returns IMessageDeleteActivity if this is a message delete activity, null otherwise
        /// </summary>
        ISuggestionActivity AsSuggestionActivity();

        /// <summary>
        /// Gets a conversation reference from an activity.
        /// </summary>
        /// <returns>A conversation reference for the conversation that contains the activity.</returns>
        ConversationReference GetConversationReference();

        /// <summary>
        /// Updates an activity with the delivery information from an existing 
        /// conversation reference.
        /// </summary>
        /// <param name="reference">The conversation reference.</param>
        /// <param name="isIncoming">(Optional) <c>true</c> to treat the activity as an 
        /// incoming activity, where the bot is the recipient; otherwaire <c>false</c>.
        /// Default is <c>false</c>, and the activity will show the bot as the sender.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reference"/> is null.</exception>
        /// <remarks>Call <see cref="GetConversationReference"/> on an incoming
        /// activity to get a conversation reference that you can then use to update an
        /// outgoing activity with the correct delivery information.
        /// </remarks>
        Activity ApplyConversationReference(ConversationReference reference, bool isIncoming = false);
    }
}
