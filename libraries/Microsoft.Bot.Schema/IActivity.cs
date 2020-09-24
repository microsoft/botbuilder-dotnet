// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Shared properties for all activities.
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// Gets or sets activity type.
        /// </summary>
        /// <value>
        /// Activity type.
        /// </value>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets iD for the activity.
        /// </summary>
        /// <value>
        /// ID for the activity.
        /// </value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets service URL where responses to this activity should be sent.
        /// </summary>
        /// <value>
        /// Service URL where responses to this activity should be sent.
        /// </value>
#pragma warning disable CA1056 // Uri properties should not be strings (we can't change this without breaking binary compat)
        string ServiceUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets timestamp when this message was sent (UTC).
        /// </summary>
        /// <value>
        /// Timestamp when this message was sent (UTC).
        /// </value>
        DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the local date and time of the message,
        /// expressed in ISO-8601 format.
        /// For example, 2016-09-23T13:07:49.4714686-07:00.
        /// </summary>
        /// <value>
        /// Local date and time of the message,
        /// expressed in ISO-8601 format.
        /// </value>
        DateTimeOffset? LocalTimestamp { get; set; }

        /// <summary>
        /// Gets or sets Channel this activity is associated with.
        /// </summary>
        /// <value>
        /// Channel this activity is associated with.
        /// </value>
        string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets Sender address.
        /// </summary>
        /// <value>
        /// Sender address.
        /// </value>
        ChannelAccount From { get; set; }

        /// <summary>
        /// Gets or sets Address for the conversation that this activity is associated with.
        /// </summary>
        /// <value>
        /// Address for the conversation that this activity is associated with.
        /// </value>
        ConversationAccount Conversation { get; set; }

        /// <summary>
        /// Gets or sets address that received the message.
        /// </summary>
        /// <value>
        /// Address that received the message.
        /// </value>
        ChannelAccount Recipient { get; set; }

        /// <summary>
        /// Gets or sets the original ID this activity is a response to.
        /// </summary>
        /// <value>
        /// The original ID this activity is a response to.
        /// </value>
        string ReplyToId { get; set; }

        /// <summary>
        /// Gets or sets collection of Entity objects, each of which contains metadata about this activity. Each Entity object is typed.
        /// </summary>
        /// <value>
        /// Collection of Entity objects, each of which contains metadata about this activity. Each Entity object is typed.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        IList<Entity> Entities { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets channel-specific payload.
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
        /// <value>
        /// Channel-specific payload.
        /// </value>
#pragma warning disable CA1721 // Property names should not match get methods (we can't change this without breaking binary compat)
        dynamic ChannelData { get; set; }
#pragma warning restore CA1721 // Property names should not match get methods

        /// <summary>
        /// Gets the channel data as strongly typed object.
        /// </summary>
        /// <typeparam name="TypeT">The expected type of the object.</typeparam>
        /// <returns>The strongly typed channel data.</returns>
#pragma warning disable CA1715 // Identifiers should have correct prefix (we can't change this without breaking binary compat)
        TypeT GetChannelData<TypeT>();
#pragma warning restore CA1715 // Identifiers should have correct prefix

        /// <summary>
        /// Try to get the channeldata as a strongly typed object.
        /// </summary>
        /// <typeparam name="TypeT">Type T.</typeparam>
        /// <param name="instance">instance.</param>
        /// <returns>false if there is no valid channeldata available.</returns>
#pragma warning disable CA1715 // Identifiers should have correct prefix (we can't change this without breaking binary compat)
        bool TryGetChannelData<TypeT>(out TypeT instance);
#pragma warning restore CA1715 // Identifiers should have correct prefix

        /// <summary>
        /// Return IMessageActivity if this is a message activity, null otherwise.
        /// </summary>
        /// <returns>IMessageActivity.</returns>
        IMessageActivity AsMessageActivity();

        /// <summary>
        /// Return IContactRelationUpdateActivity if this is a contactRelationUpdate activity, null otherwise.
        /// </summary>
        /// <returns>IContactRelationUpdateActivity.</returns>
        IContactRelationUpdateActivity AsContactRelationUpdateActivity();

        /// <summary>
        /// Return IInstallationUpdateActivity if this is a installationUpdate activity, null otherwise.
        /// </summary>
        /// <returns>IInstallationUpdateActivity.</returns>
        IInstallationUpdateActivity AsInstallationUpdateActivity();

        /// <summary>
        /// Return IConversationUpdateActivity if this is a conversationUpdate activity, null otherwise.
        /// </summary>
        /// <returns>IConversationUpdateActivity.</returns>
        IConversationUpdateActivity AsConversationUpdateActivity();

        /// <summary>
        /// Return ITypingActivity if this is a typing activity, null otherwise.
        /// </summary>
        /// <returns>ITypingActivity.</returns>
        ITypingActivity AsTypingActivity();

        /// <summary>
        /// Return IEndOfConversationActivity if this is an end-of-conversation activity, null otherwise.
        /// </summary>
        /// <returns>IEndOfConversationActivity.</returns>
        IEndOfConversationActivity AsEndOfConversationActivity();

        /// <summary>
        /// Returns IEventActivity if this is an event activity, null otherwise.
        /// </summary>
        /// <returns>IEventActivity.</returns>
        IEventActivity AsEventActivity();

        /// <summary>
        /// Returns IInvokeActivity if this is an invoke activity, null otherwise.
        /// </summary>
        /// <returns>IInvokeActivity.</returns>
        IInvokeActivity AsInvokeActivity();

        /// <summary>
        /// Returns IMessageUpdateActivity if this is a message update activity, null otherwise.
        /// </summary>
        /// <returns>IMessageUpdateActivity.</returns>
        IMessageUpdateActivity AsMessageUpdateActivity();

        /// <summary>
        /// Returns IMessageDeleteActivity if this is a message delete activity, null otherwise.
        /// </summary>
        /// <returns>IMessageDeleteActivity.</returns>
        IMessageDeleteActivity AsMessageDeleteActivity();

        /// <summary>
        /// Returns IMessageDeleteActivity if this is a message reaction activity, null otherwise.
        /// </summary>
        /// <returns>IMessageReactionActivity.</returns>
        IMessageReactionActivity AsMessageReactionActivity();

        /// <summary>
        /// Returns IMessageDeleteActivity if this is a suggestion activity, null otherwise.
        /// </summary>
        /// <returns>ISuggestionActivity.</returns>
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
        /// <returns>Activity.</returns>
        Activity ApplyConversationReference(ConversationReference reference, bool isIncoming = false);
    }
}
