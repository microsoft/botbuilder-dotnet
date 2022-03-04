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
    /// An Activity is the basic communication type for the Bot Framework 3.0
    /// protocol.
    /// </summary>
    [DebuggerDisplay("[{Type}] {Text ?? System.String.Empty}")]
    public class Activity :
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
        IHandoffActivity,
        ICommandActivity,
        ICommandResultActivity
    {
        /// <summary>
        /// The HTTP <c>Content-Type</c> entity header that identifies an <see cref="Activity"/> media type resource.
        /// </summary>
        /// <remarks>In multi-part HTTP content, this header identifies the activity portion of the content.</remarks>
        public const string ContentType = "application/vnd.microsoft.activity";

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        public Activity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        /// <param name="type">Contains the activity type. Possible values
        /// include: 'message', 'contactRelationUpdate', 'conversationUpdate',
        /// 'typing', 'endOfConversation', 'event', 'invoke', 'deleteUserData',
        /// 'messageUpdate', 'messageDelete', 'installationUpdate',
        /// 'messageReaction', 'suggestion', 'trace', 'handoff'.</param>
        /// <param name="id">Contains an ID that uniquely identifies the
        /// activity on the channel.</param>
        /// <param name="timestamp">Contains the date and time when the message
        /// was sent, in UTC, expressed in ISO-8601 format.</param>
        /// <param name="localTimestamp">Contains the date and time when the
        /// message was sent, in local time, expressed in ISO-8601 format.
        /// For example, 2016-09-23T13:07:49.4714686-07:00.</param>
        /// <param name="localTimezone">Contains the name of the timezone used
        /// to define local time for the message, expressed in IANA Time Zone
        /// database format.
        /// For example, America/Los_Angeles.</param>
        /// <param name="serviceUrl">Contains the URL that specifies the
        /// channel's service endpoint. Set by the channel.</param>
        /// <param name="channelId">Contains an ID that uniquely identifies the
        /// channel. Set by the channel.</param>
        /// <param name="from">Identifies the sender of the message.</param>
        /// <param name="conversation">Identifies the conversation to which the
        /// activity belongs.</param>
        /// <param name="recipient">Identifies the recipient of the
        /// message.</param>
        /// <param name="textFormat">Format of text fields. Default: markdown.
        /// Possible values include: 'markdown', 'plain', 'xml'.</param>
        /// <param name="attachmentLayout">The layout hint for multiple
        /// attachments. Default: list. Possible values include: 'list',
        /// 'carousel'.</param>
        /// <param name="membersAdded">The collection of members added to the
        /// conversation.</param>
        /// <param name="membersRemoved">The collection of members removed from
        /// the conversation.</param>
        /// <param name="reactionsAdded">The collection of reactions added to
        /// the conversation.</param>
        /// <param name="reactionsRemoved">The collection of reactions removed
        /// from the conversation.</param>
        /// <param name="topicName">The updated topic name of the
        /// conversation.</param>
        /// <param name="historyDisclosed">Indicates whether the prior history
        /// of the channel is disclosed.</param>
        /// <param name="locale">A locale name for the contents of the text
        /// field.
        /// The locale name is a combination of an ISO 639 two- or three-letter
        /// culture code associated with a language
        /// and an ISO 3166 two-letter subculture code associated with a
        /// country or region.
        /// The locale name can also correspond to a valid BCP-47 language
        /// tag.</param>
        /// <param name="text">The text content of the message.</param>
        /// <param name="speak">The text to speak.</param>
        /// <param name="inputHint">Indicates whether your bot is accepting,
        /// expecting, or ignoring user input after the message is delivered to
        /// the client. Possible values include: 'acceptingInput',
        /// 'ignoringInput', 'expectingInput'.</param>
        /// <param name="summary">The text to display if the channel cannot
        /// render cards.</param>
        /// <param name="suggestedActions">The suggested actions for the
        /// activity.</param>
        /// <param name="attachments">The attachments for the activity.</param>
        /// <param name="entities">Represents the entities that were mentioned
        /// in the message.</param>
        /// <param name="channelData">Contains channel-specific
        /// content.</param>
        /// <param name="action">Indicates whether the recipient of a
        /// contactRelationUpdate was added to or removed from the sender's
        /// contact list.</param>
        /// <param name="replyToId">Contains the ID of the message to which
        /// this message is a reply.</param>
        /// <param name="label">A descriptive label for the activity.</param>
        /// <param name="valueType">The type of the activity's value
        /// object.</param>
        /// <param name="value">A value that is associated with the
        /// activity.</param>
        /// <param name="name">The name of the operation associated with an
        /// invoke or event activity.</param>
        /// <param name="relatesTo">A reference to another conversation or
        /// activity.</param>
        /// <param name="code">A code for endOfConversation activities that
        /// indicates why the conversation ended. Possible values include:
        /// 'unknown', 'completedSuccessfully', 'userCancelled', 'botTimedOut',
        /// 'botIssuedInvalidMessage', 'channelFailed'.</param>
        /// <param name="expiration">The time at which the activity should be
        /// considered to be "expired" and should not be presented to the
        /// recipient.</param>
        /// <param name="importance">The importance of the activity. Possible
        /// values include: 'low', 'normal', 'high'.</param>
        /// <param name="deliveryMode">A delivery hint to signal to the
        /// recipient alternate delivery paths for the activity.
        /// The default delivery mode is "default". Possible values include:
        /// 'normal', 'notification', 'expectReplies', 'ephemeral'.</param>
        /// <param name="listenFor">List of phrases and references that speech
        /// and language priming systems should listen for.</param>
        /// <param name="textHighlights">The collection of text fragments to
        /// highlight when the activity contains a ReplyToId value.</param>
        /// <param name="semanticAction">An optional programmatic action
        /// accompanying this request.</param>
        public Activity(string type = default, string id = default, System.DateTimeOffset? timestamp = default, System.DateTimeOffset? localTimestamp = default, string serviceUrl = default, string channelId = default, ChannelAccount from = default, ConversationAccount conversation = default, ChannelAccount recipient = default, string textFormat = default, string attachmentLayout = default, IList<ChannelAccount> membersAdded = default, IList<ChannelAccount> membersRemoved = default, IList<MessageReaction> reactionsAdded = default, IList<MessageReaction> reactionsRemoved = default, string topicName = default, bool? historyDisclosed = default, string locale = default, string text = default, string speak = default, string inputHint = default, string summary = default, SuggestedActions suggestedActions = default, IList<Attachment> attachments = default, IList<Entity> entities = default, object channelData = default, string action = default, string replyToId = default, string label = default, string valueType = default, object value = default, string name = default, ConversationReference relatesTo = default, string code = default, System.DateTimeOffset? expiration = default, string importance = default, string deliveryMode = default, IList<string> listenFor = default, IList<TextHighlight> textHighlights = default, SemanticAction semanticAction = default, string localTimezone = default)
        {
            Type = type;
            Id = id;
            Timestamp = timestamp;
            LocalTimestamp = localTimestamp;
            LocalTimezone = localTimezone;
            ServiceUrl = serviceUrl;
            ChannelId = channelId;
            From = from;
            Conversation = conversation;
            Recipient = recipient;
            TextFormat = textFormat;
            AttachmentLayout = attachmentLayout;
            MembersAdded = membersAdded ?? new List<ChannelAccount>();
            MembersRemoved = membersRemoved ?? new List<ChannelAccount>();
            ReactionsAdded = reactionsAdded ?? new List<MessageReaction>();
            ReactionsRemoved = reactionsRemoved ?? new List<MessageReaction>();
            TopicName = topicName;
            HistoryDisclosed = historyDisclosed;
            Locale = locale;
            Text = text;
            Speak = speak;
            InputHint = inputHint;
            Summary = summary;
            SuggestedActions = suggestedActions;
            Attachments = attachments ?? new List<Attachment>();
            Entities = entities ?? new List<Entity>();
            ChannelData = channelData;
            Action = action;
            ReplyToId = replyToId;
            Label = label;
            ValueType = valueType;
            Value = value;
            Name = name;
            RelatesTo = relatesTo;
            Code = code;
            Expiration = expiration;
            Importance = importance;
            DeliveryMode = deliveryMode;
            ListenFor = listenFor ?? new List<string>();
            TextHighlights = textHighlights ?? new List<TextHighlight>();
            SemanticAction = semanticAction;
        }

        /// <summary>
        /// Gets or sets the activity type. Possible values include:
        /// 'message', 'contactRelationUpdate', 'conversationUpdate', 'typing',
        /// 'endOfConversation', 'event', 'invoke', 'deleteUserData',
        /// 'messageUpdate', 'messageDelete', 'installationUpdate',
        /// 'messageReaction', 'suggestion', 'trace', 'handoff'.
        /// </summary>
        /// <value>
        /// The activity type (see <see cref="ActivityTypes"/>). 
        /// </value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets an ID that uniquely identifies the activity on the
        /// channel.
        /// </summary>
        /// <value>
        /// An ID that uniquely identifies the activity on the channel.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was sent, in UTC,
        /// expressed in ISO-8601 format.
        /// </summary>
        /// <value>
        /// The date and time when the message was sent, in UTC, expressed in ISO-8601 format.
        /// </value>
        [JsonProperty(PropertyName = "timestamp")]
        public System.DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was sent, in local
        /// time, expressed in ISO-8601 format.
        /// For example, 2016-09-23T13:07:49.4714686-07:00.
        /// </summary>
        /// <value>
        /// The date and time when the message was sent, in local time, expressed in ISO-8601 format.
        /// </value>
        [JsonProperty(PropertyName = "localTimestamp")]
        public System.DateTimeOffset? LocalTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the name of the timezone used to define local time for
        /// the message, expressed in IANA Time Zone database format.
        /// For example, America/Los_Angeles.
        /// </summary>
        /// <value>
        /// The name of the timezone used to define local time for
        /// the message, expressed in IANA Time Zone database format.
        /// </value>
        [JsonProperty(PropertyName = "localTimezone")]
        public string LocalTimezone { get; set; }

        /// <summary>
        /// Gets or sets the URL that specifies the channel's service endpoint.
        /// Set by the channel.
        /// </summary>
        /// <value>
        /// The URL that specifies the channel's service endpoint.
        /// </value>
        [JsonProperty(PropertyName = "serviceUrl")]
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets an ID that uniquely identifies the channel.
        /// Set by the channel.
        /// </summary>
        /// <value>
        /// An ID that uniquely identifies the channel.
        /// </value>
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the sender of the message.
        /// </summary>
        /// <value>
        /// The <see cref="ChannelAccount"/> for the sender of the message.
        /// </value>
        [JsonProperty(PropertyName = "from")]
        public ChannelAccount From { get; set; }

        /// <summary>
        /// Gets or sets the conversation to which the activity belongs.
        /// </summary>
        /// <value>
        /// The <see cref="ConversationAccount"/> to which the activity belongs.
        /// </value>
        [JsonProperty(PropertyName = "conversation")]
        public ConversationAccount Conversation { get; set; }

        /// <summary>
        /// Gets or sets the recipient of the message.
        /// </summary>
        /// <value>
        /// The <see cref="ChannelAccount"/> for the recipient of the message.
        /// </value>
        [JsonProperty(PropertyName = "recipient")]
        public ChannelAccount Recipient { get; set; }

        /// <summary>
        /// Gets or sets the format of text fields. Default: markdown. Possible
        /// values are defined by <see cref="TextFormatTypes"/>.
        /// </summary>
        /// <value>
        /// The format of text fields (see <see cref="TextFormatTypes"/>).
        /// </value>
        [JsonProperty(PropertyName = "textFormat")]
        public string TextFormat { get; set; }

        /// <summary>
        /// Gets or sets the layout hint for multiple attachments. Default:
        /// list. Possible values are defined by <see cref="AttachmentLayoutTypes"/>.
        /// </summary>
        /// <value>
        /// The layout hint for multiple attachments.
        /// </value>
        [JsonProperty(PropertyName = "attachmentLayout")]
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets the collection of members added to the conversation.
        /// </summary>
        /// <value>
        /// The collection of members added to the conversation.
        /// </value>
        [JsonProperty(PropertyName = "membersAdded")]
        public IList<ChannelAccount> MembersAdded { get; private set; } = new List<ChannelAccount>();

        /// <summary>
        /// Gets the collection of members removed from the
        /// conversation.
        /// </summary>
        /// <value>
        /// The collection of members removed from the conversation.
        /// </value>
        [JsonProperty(PropertyName = "membersRemoved")]
        public IList<ChannelAccount> MembersRemoved { get; private set; } = new List<ChannelAccount>();

        /// <summary>
        /// Gets the collection of reactions added to the conversation.
        /// </summary>
        /// <value>
        /// The collection of reactions added to the conversation.
        /// </value>
        [JsonProperty(PropertyName = "reactionsAdded")]
        public IList<MessageReaction> ReactionsAdded { get; private set; } = new List<MessageReaction>();

        /// <summary>
        /// Gets the collection of reactions removed from the
        /// conversation.
        /// </summary>
        /// <value>
        /// The collection of reactions removed from the conversation.
        /// </value>
        [JsonProperty(PropertyName = "reactionsRemoved")]
        public IList<MessageReaction> ReactionsRemoved { get; private set; } = new List<MessageReaction>();

        /// <summary>
        /// Gets or sets the updated topic name of the conversation.
        /// </summary>
        /// <value>
        /// The updated topic name of the conversation.
        /// </value>
        [JsonProperty(PropertyName = "topicName")]
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the prior history of the
        /// channel is disclosed.
        /// </summary>
        /// <value>
        /// A value indicating whether the prior history of the channel is disclosed.
        /// </value>
        [JsonProperty(PropertyName = "historyDisclosed")]
        public bool? HistoryDisclosed { get; set; }

        /// <summary>
        /// Gets or sets a BCP-47 locale name for the contents of the text field.
        /// </summary>
        /// <value>
        /// A locale for the activity.
        /// </value>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the text content of the message.
        /// </summary>
        /// <value>
        /// The text content of the message.
        /// </value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the text to speak.
        /// </summary>
        /// <value>
        /// The text to speak.
        /// </value>
        [JsonProperty(PropertyName = "speak")]
        public string Speak { get; set; }

        /// <summary>
        /// Gets or sets a string indicating whether your bot is accepting,
        /// expecting, or ignoring user input after the message is delivered to
        /// the client (see <see cref="InputHints"/>.
        /// </summary>
        /// <value>
        /// A string indicating whether your bot is accepting,
        /// expecting, or ignoring user input after the message is delivered to
        /// the client.
        /// </value>
        [JsonProperty(PropertyName = "inputHint")]
        public string InputHint { get; set; }

        /// <summary>
        /// Gets or sets the text to display if the channel cannot render
        /// cards.
        /// </summary>
        /// <value>
        /// The text to display if the channel cannot render cards.
        /// </value>
        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the suggested actions for the activity.
        /// </summary>
        /// <value>
        /// The suggested actions for the activity.
        /// </value>
        [JsonProperty(PropertyName = "suggestedActions")]
        public SuggestedActions SuggestedActions { get; set; }

        /// <summary>
        /// Gets the attachments for the activity.
        /// </summary>
        /// <value>
        /// The attachments for the activity.
        /// </value>
        [JsonProperty(PropertyName = "attachments")]
        public IList<Attachment> Attachments { get; private set; } = new List<Attachment>();

        /// <summary>
        /// Gets the entities that were mentioned in the message.
        /// </summary>
        /// <value>
        /// The entities that were mentioned in the message.
        /// </value>
        [JsonProperty(PropertyName = "entities")]
        public IList<Entity> Entities { get; private set; } = new List<Entity>();

        /// <summary>
        /// Gets or sets channel-specific content.
        /// </summary>
        /// <value>
        /// Channel-specific content.
        /// </value>
        [JsonProperty(PropertyName = "channelData")]
#pragma warning disable CA1721 // Property names should not match get methods (we can't change this without changing binary compat).
        public object ChannelData { get; set; }
#pragma warning restore CA1721 // Property names should not match get methods

        /// <summary>
        /// Gets or sets a string indicating whether the recipient of a
        /// contactRelationUpdate was added to or removed from the sender's
        /// contact list.
        /// </summary>
        /// <value>
        /// A string indicating whether the recipient of a
        /// contactRelationUpdate was added to or removed from the sender's
        /// contact list.
        /// </value>
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the ID of the message to which this message is a
        /// reply.
        /// </summary>
        /// <value>
        /// The ID of the message to which this message is a reply.
        /// </value>
        [JsonProperty(PropertyName = "replyToId")]
        public string ReplyToId { get; set; }

        /// <summary>
        /// Gets or sets a descriptive label for the activity.
        /// </summary>
        /// <value>
        /// A descriptive label for the activity.
        /// </value>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity's value object.
        /// </summary>
        /// <value>
        /// The type of the activity's value object.
        /// </value>
        [JsonProperty(PropertyName = "valueType")]
        public string ValueType { get; set; }

        /// <summary>
        /// Gets or sets a value that is associated with the activity.
        /// </summary>
        /// <value>
        /// A value that is associated with the activity.
        /// </value>
        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the name of the operation associated with an invoke or
        /// event activity.
        /// </summary>
        /// <value>
        /// The name of the operation associated with an invoke or event activity.
        /// </value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="ConversationReference"/> to another conversation or activity.
        /// </summary>
        /// <value>
        /// A reference to another conversation or activity.
        /// </value>
        [JsonProperty(PropertyName = "relatesTo")]
        public ConversationReference RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets a code for endOfConversation activities that indicates
        /// why the conversation ended. Possible values are defined in <see cref="EndOfConversationCodes"/>.
        /// </summary>
        /// <value>
        /// A code for endOfConversation activities that indicates
        /// why the conversation ended.
        /// </value>
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the time at which the activity should be considered to
        /// be "expired" and should not be presented to the recipient.
        /// </summary>
        /// <value>
        /// The time at which the activity should be considered to
        /// be "expired" and should not be presented to the recipient.
        /// </value>
        [JsonProperty(PropertyName = "expiration")]
        public System.DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// Gets or sets the importance of the activity. Possible values
        /// are defined in <see cref="ActivityImportance"/>.
        /// </summary>
        /// <value>
        /// The importance of the activity.
        /// </value>
        [JsonProperty(PropertyName = "importance")]
        public string Importance { get; set; }

        /// <summary>
        /// Gets or sets a delivery hint to signal to the recipient alternate
        /// delivery paths for the activity.
        /// The default delivery mode is "default". Possible values are defined in <see cref="DeliveryModes"/>.
        /// </summary>
        /// <value>
        /// A delivery hint to signal to the recipient alternate
        /// delivery paths for the activity.
        /// </value>
        [JsonProperty(PropertyName = "deliveryMode")]
        public string DeliveryMode { get; set; }

        /// <summary>
        /// Gets list of phrases and references that speech and
        /// language-priming systems should listen for.
        /// </summary>
        /// <value>
        /// List of phrases and references that speech and language-priming systems should listen for.
        /// </value>
        [JsonProperty(PropertyName = "listenFor")]
        public IList<string> ListenFor { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the collection of text fragments to highlight when the
        /// activity contains a ReplyToId value.
        /// </summary>
        /// <value>
        /// The collection of text fragments to highlight when the
        /// activity contains a ReplyToId value.
        /// </value>
        [JsonProperty(PropertyName = "textHighlights")]
        public IList<TextHighlight> TextHighlights { get; private set; } = new List<TextHighlight>();

        /// <summary>
        /// Gets or sets an optional programmatic action accompanying this
        /// request.
        /// </summary>
        /// <value>
        /// An optional programmatic action accompanying this
        /// request.
        /// </value>
        [JsonProperty(PropertyName = "semanticAction")]
        public SemanticAction SemanticAction { get; set; }

        /// <summary>
        /// Gets or sets a string containing an IRI identifying the caller of a bot.
        /// This field is not intended to be transmitted over the wire, but is
        /// instead populated by bots and clients based on cryptographically
        /// verifiable data that asserts the identity of the callers (e.g. tokens).
        /// </summary>
        /// <value>
        /// A string containing an IRI identifying the caller of a bot.
        /// This field is not intended to be transmitted over the wire, but is
        /// instead populated by bots and clients based on cryptographically
        /// verifiable data that asserts the identity of the callers (e.g. tokens).
        /// </value>
        [JsonProperty(PropertyName = "callerId")]
        public string CallerId { get; set; }

        /// <summary>
        /// Gets properties that are not otherwise defined by the <see cref="Activity"/> type but that
        /// might appear in the serialized REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; private set; } = new JObject();

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
                From = new ChannelAccount(id: Recipient?.Id, name: Recipient?.Name),
                Recipient = new ChannelAccount(id: From?.Id, name: From?.Name),
                ReplyToId = !string.Equals(Type, ActivityTypes.ConversationUpdate, StringComparison.OrdinalIgnoreCase) || (!string.Equals(ChannelId, "directline", StringComparison.OrdinalIgnoreCase) && !string.Equals(ChannelId, "webchat", StringComparison.OrdinalIgnoreCase)) ? Id : null,
                ServiceUrl = ServiceUrl,
                ChannelId = ChannelId,
                Conversation = new ConversationAccount(isGroup: Conversation.IsGroup, id: Conversation.Id, name: Conversation.Name),
                Text = text ?? string.Empty,
                Locale = locale ?? Locale,
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
                From = new ChannelAccount(id: Recipient?.Id, name: Recipient?.Name),
                Recipient = new ChannelAccount(id: From?.Id, name: From?.Name),
                ReplyToId = !string.Equals(Type, ActivityTypes.ConversationUpdate, StringComparison.OrdinalIgnoreCase) || (!string.Equals(ChannelId, "directline", StringComparison.OrdinalIgnoreCase) && !string.Equals(ChannelId, "webchat", StringComparison.OrdinalIgnoreCase)) ? Id : null,
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
        /// Returns this activity as an <see cref="ICommandActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a command activity; or null.</returns>
        public ICommandActivity AsCommandActivity()
        {
            return IsActivity(ActivityTypes.Command) ? this : null;
        }

        /// <summary>
        /// Returns this activity as an <see cref="ICommandResultActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a command result activity; or null.</returns>
        public ICommandResultActivity AsCommandResultActivity()
        {
            return IsActivity(ActivityTypes.CommandResult) ? this : null;
        }

        /// <summary>
        /// Indicates whether this activity has content.
        /// </summary>
        /// <returns>True, if this activity has any content to send; otherwise, false.</returns>
        /// <remarks>This method is defined on the <see cref="Activity"/> class, but is only intended
        /// for use with a message activity, where the activity <see cref="Type"/> is set to
        /// <see cref="ActivityTypes.Message"/>.</remarks>
        public bool HasContent()
        {
            if (!string.IsNullOrWhiteSpace(Text))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(Summary))
            {
                return true;
            }

            if (Attachments != null && Attachments.Count > 0)
            {
                return true;
            }

            if (ChannelData != null)
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
        /// for use with a message activity, where the activity <see cref="Type"/> is set to
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
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="TryGetChannelData{TypeT}(out TypeT)"/>
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

            return ((JObject)ChannelData).ToObject<T>();
        }

        /// <summary>
        /// Gets the channel data for this activity as a strongly-typed object.
        /// A return value idicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="instance">When this method returns, contains the strongly-typed object if the operation succeeded,
        /// or the type's default value if the operation failed.</param>
        /// <returns>
        /// <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="GetChannelData{TType}"/>
        public bool TryGetChannelData<T>(out T instance)
        {
            instance = default;

            try
            {
                if (ChannelData == null)
                {
                    return false;
                }

                instance = GetChannelData<T>();
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types (we just return false here if the conversion fails for any reason)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
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
            var reference = new ConversationReference
            {
                ActivityId = !string.Equals(Type, ActivityTypes.ConversationUpdate, StringComparison.OrdinalIgnoreCase) || (!string.Equals(ChannelId, "directline", StringComparison.OrdinalIgnoreCase) && !string.Equals(ChannelId, "webchat", StringComparison.OrdinalIgnoreCase)) ? Id : null,
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
            {// Outgoing
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

            var type = Type;

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
