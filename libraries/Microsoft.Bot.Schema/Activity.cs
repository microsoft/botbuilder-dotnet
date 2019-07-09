// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
#pragma warning disable SA1609 // PropertyDocumentationMustHaveValue, temporary ignore, needs to be fixed later
    /// <summary>
    /// An Activity is the basic communication type for the Bot Framework 3.0
    /// protocol.
    /// </summary>
    public partial class Activity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        public Activity()
        {
            CustomInit();
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
        /// 'normal', 'notification'.</param>
        /// <param name="listenFor">List of phrases and references that speech
        /// and language priming systems should listen for.</param>
        /// <param name="textHighlights">The collection of text fragments to
        /// highlight when the activity contains a ReplyToId value.</param>
        /// <param name="semanticAction">An optional programmatic action
        /// accompanying this request.</param>
        public Activity(string type = default(string), string id = default(string), System.DateTimeOffset? timestamp = default(System.DateTimeOffset?), System.DateTimeOffset? localTimestamp = default(System.DateTimeOffset?), string serviceUrl = default(string), string channelId = default(string), ChannelAccount from = default(ChannelAccount), ConversationAccount conversation = default(ConversationAccount), ChannelAccount recipient = default(ChannelAccount), string textFormat = default(string), string attachmentLayout = default(string), IList<ChannelAccount> membersAdded = default(IList<ChannelAccount>), IList<ChannelAccount> membersRemoved = default(IList<ChannelAccount>), IList<MessageReaction> reactionsAdded = default(IList<MessageReaction>), IList<MessageReaction> reactionsRemoved = default(IList<MessageReaction>), string topicName = default(string), bool? historyDisclosed = default(bool?), string locale = default(string), string text = default(string), string speak = default(string), string inputHint = default(string), string summary = default(string), SuggestedActions suggestedActions = default(SuggestedActions), IList<Attachment> attachments = default(IList<Attachment>), IList<Entity> entities = default(IList<Entity>), object channelData = default(object), string action = default(string), string replyToId = default(string), string label = default(string), string valueType = default(string), object value = default(object), string name = default(string), ConversationReference relatesTo = default(ConversationReference), string code = default(string), System.DateTimeOffset? expiration = default(System.DateTimeOffset?), string importance = default(string), string deliveryMode = default(string), IList<string> listenFor = default(IList<string>), IList<TextHighlight> textHighlights = default(IList<TextHighlight>), SemanticAction semanticAction = default(SemanticAction), string localTimezone = default(string))
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
            MembersAdded = membersAdded;
            MembersRemoved = membersRemoved;
            ReactionsAdded = reactionsAdded;
            ReactionsRemoved = reactionsRemoved;
            TopicName = topicName;
            HistoryDisclosed = historyDisclosed;
            Locale = locale;
            Text = text;
            Speak = speak;
            InputHint = inputHint;
            Summary = summary;
            SuggestedActions = suggestedActions;
            Attachments = attachments;
            Entities = entities;
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
            ListenFor = listenFor;
            TextHighlights = textHighlights;
            SemanticAction = semanticAction;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the activity type. Possible values include:
        /// 'message', 'contactRelationUpdate', 'conversationUpdate', 'typing',
        /// 'endOfConversation', 'event', 'invoke', 'deleteUserData',
        /// 'messageUpdate', 'messageDelete', 'installationUpdate',
        /// 'messageReaction', 'suggestion', 'trace', 'handoff'.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets an ID that uniquely identifies the activity on the
        /// channel.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was sent, in UTC,
        /// expressed in ISO-8601 format.
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public System.DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was sent, in local
        /// time, expressed in ISO-8601 format.
        /// For example, 2016-09-23T13:07:49.4714686-07:00.
        /// </summary>
        [JsonProperty(PropertyName = "localTimestamp")]
        public System.DateTimeOffset? LocalTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the name of the timezone used to define local time for
        /// the message, expressed in IANA Time Zone database format.
        /// For example, America/Los_Angeles.
        /// </summary>
        [JsonProperty(PropertyName = "localTimezone")]
        public string LocalTimezone { get; set; }

        /// <summary>
        /// Gets or sets the URL that specifies the channel's service endpoint.
        /// Set by the channel.
        /// </summary>
        [JsonProperty(PropertyName = "serviceUrl")]
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets an ID that uniquely identifies the channel.
        /// Set by the channel.
        /// </summary>
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the sender of the message.
        /// </summary>
        [JsonProperty(PropertyName = "from")]
        public ChannelAccount From { get; set; }

        /// <summary>
        /// Gets or sets the conversation to which the activity belongs.
        /// </summary>
        [JsonProperty(PropertyName = "conversation")]
        public ConversationAccount Conversation { get; set; }

        /// <summary>
        /// Gets or sets the recipient of the message.
        /// </summary>
        [JsonProperty(PropertyName = "recipient")]
        public ChannelAccount Recipient { get; set; }

        /// <summary>
        /// Gets or sets the format of text fields. Default: markdown. Possible
        /// values include: 'markdown', 'plain', 'xml'.
        /// </summary>
        [JsonProperty(PropertyName = "textFormat")]
        public string TextFormat { get; set; }

        /// <summary>
        /// Gets or sets the layout hint for multiple attachments. Default:
        /// list. Possible values include: 'list', 'carousel'.
        /// </summary>
        [JsonProperty(PropertyName = "attachmentLayout")]
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets the collection of members added to the conversation.
        /// </summary>
        [JsonProperty(PropertyName = "membersAdded")]
        public IList<ChannelAccount> MembersAdded { get; set; }

        /// <summary>
        /// Gets or sets the collection of members removed from the
        /// conversation.
        /// </summary>
        [JsonProperty(PropertyName = "membersRemoved")]
        public IList<ChannelAccount> MembersRemoved { get; set; }

        /// <summary>
        /// Gets or sets the collection of reactions added to the conversation.
        /// </summary>
        [JsonProperty(PropertyName = "reactionsAdded")]
        public IList<MessageReaction> ReactionsAdded { get; set; }

        /// <summary>
        /// Gets or sets the collection of reactions removed from the
        /// conversation.
        /// </summary>
        [JsonProperty(PropertyName = "reactionsRemoved")]
        public IList<MessageReaction> ReactionsRemoved { get; set; }

        /// <summary>
        /// Gets or sets the updated topic name of the conversation.
        /// </summary>
        [JsonProperty(PropertyName = "topicName")]
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the prior history of the
        /// channel is disclosed.
        /// </summary>
        [JsonProperty(PropertyName = "historyDisclosed")]
        public bool? HistoryDisclosed { get; set; }

        /// <summary>
        /// Gets or sets a locale name for the contents of the text field.
        /// The locale name is a combination of an ISO 639 two- or three-letter
        /// culture code associated with a language
        /// and an ISO 3166 two-letter subculture code associated with a
        /// country or region.
        /// The locale name can also correspond to a valid BCP-47 language tag.
        /// </summary>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the text content of the message.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the text to speak.
        /// </summary>
        [JsonProperty(PropertyName = "speak")]
        public string Speak { get; set; }

        /// <summary>
        /// Gets or sets a string indicating whether your bot is accepting,
        /// expecting, or ignoring user input after the message is delivered to
        /// the client. Possible values include: 'acceptingInput',
        /// 'ignoringInput', 'expectingInput'.
        /// </summary>
        [JsonProperty(PropertyName = "inputHint")]
        public string InputHint { get; set; }

        /// <summary>
        /// Gets or sets the text to display if the channel cannot render
        /// cards.
        /// </summary>
        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the suggested actions for the activity.
        /// </summary>
        [JsonProperty(PropertyName = "suggestedActions")]
        public SuggestedActions SuggestedActions { get; set; }

        /// <summary>
        /// Gets or sets the attachments for the activity.
        /// </summary>
        [JsonProperty(PropertyName = "attachments")]
        public IList<Attachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the entities that were mentioned in the message.
        /// </summary>
        [JsonProperty(PropertyName = "entities")]
        public IList<Entity> Entities { get; set; }

        /// <summary>
        /// Gets or sets channel-specific content.
        /// </summary>
        [JsonProperty(PropertyName = "channelData")]
        public object ChannelData { get; set; }

        /// <summary>
        /// Gets or sets a string indicating whether the recipient of a
        /// contactRelationUpdate was added to or removed from the sender's
        /// contact list.
        /// </summary>
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the ID of the message to which this message is a
        /// reply.
        /// </summary>
        [JsonProperty(PropertyName = "replyToId")]
        public string ReplyToId { get; set; }

        /// <summary>
        /// Gets or sets a descriptive label for the activity.
        /// </summary>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity's value object.
        /// </summary>
        [JsonProperty(PropertyName = "valueType")]
        public string ValueType { get; set; }

        /// <summary>
        /// Gets or sets a value that is associated with the activity.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the name of the operation associated with an invoke or
        /// event activity.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a reference to another conversation or activity.
        /// </summary>
        [JsonProperty(PropertyName = "relatesTo")]
        public ConversationReference RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets a code for endOfConversation activities that indicates
        /// why the conversation ended. Possible values include:
        /// 'unknown', 'completedSuccessfully', 'userCancelled', 'botTimedOut',
        /// 'botIssuedInvalidMessage', 'channelFailed'.
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the time at which the activity should be considered to
        /// be "expired" and should not be presented to the recipient.
        /// </summary>
        [JsonProperty(PropertyName = "expiration")]
        public System.DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// Gets or sets the importance of the activity. Possible values
        /// include: 'low', 'normal', 'high'.
        /// </summary>
        [JsonProperty(PropertyName = "importance")]
        public string Importance { get; set; }

        /// <summary>
        /// Gets or sets a delivery hint to signal to the recipient alternate
        /// delivery paths for the activity.
        /// The default delivery mode is "default". Possible values include:
        /// 'normal', 'notification'.
        /// </summary>
        [JsonProperty(PropertyName = "deliveryMode")]
        public string DeliveryMode { get; set; }

        /// <summary>
        /// Gets or sets list of phrases and references that speech and
        /// language-priming systems should listen for.
        /// </summary>
        [JsonProperty(PropertyName = "listenFor")]
        public IList<string> ListenFor { get; set; }

        /// <summary>
        /// Gets or sets the collection of text fragments to highlight when the
        /// activity contains a ReplyToId value.
        /// </summary>
        [JsonProperty(PropertyName = "textHighlights")]
        public IList<TextHighlight> TextHighlights { get; set; }

        /// <summary>
        /// Gets or sets an optional programmatic action accompanying this
        /// request.
        /// </summary>
        [JsonProperty(PropertyName = "semanticAction")]
        public SemanticAction SemanticAction { get; set; }

        /// <summary>
        /// Gets or sets a string containing an IRI identifying the caller of a bot.
        /// This field is not intended to be transmitted over the wire, but is
        /// instead populated by bots and clients based on cryptographically
        /// verifiable data that asserts the identity of the callers (e.g. tokens).
        /// </summary>
        [JsonProperty(PropertyName = "callerId")]
        public string CallerId { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
#pragma warning restore SA1609 // PropertyDocumentationMustHaveValue
}
