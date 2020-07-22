// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    /// <summary>
    /// Represents information associated with a Facebook webhook event. For more information, see the Facebook
    /// [Webhook Events Reference](https://developers.facebook.com/docs/messenger-platform/reference/webhook-events).
    /// </summary>
    public class FacebookMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookMessage"/> class.
        /// </summary>
        /// <param name="recipientId">Contents of the recipient ID field.</param>
        /// <param name="message">Contents of the message field.</param>
        /// <param name="messagingType">The type of webhook event. For more information, see the Facebook
        /// [List of Webhook Events](https://developers.facebook.com/docs/messenger-platform/reference/webhook-events#event_list).</param>
        /// <param name="tag">The optional message tag string. See https://developers.facebook.com/docs/messenger-platform/send-messages/message-tags.</param>
        /// <param name="notificationType">The optional notification type: REGULAR (default value), SILENT_PUSH, NO_PUSH.</param>
        /// <param name="personaId">The persona ID.</param>
        /// <param name="senderAction">Message state to display to the user: typing_on, typing_off, mark_seen. Cannot be sent with 'message'. When used, 'recipient' should be the only other property set in the request.</param>
        /// <param name="senderId">The sender ID.</param>
        public FacebookMessage(string recipientId, Message message, string messagingType, string tag = null, string notificationType = null, string personaId = null, string senderAction = null, string senderId = null)
        {
            Recipient.Id = recipientId;
            Message = message;
            MessagingType = messagingType;
            Tag = tag;
            NotificationType = notificationType;
            PersonaId = personaId;
            SenderAction = senderAction;
            Sender.Id = senderId;
        }

        /// <summary>
        /// Gets or sets the ID of the recipient.
        /// </summary>
        /// <value>The ID of the recipient.</value>
        [JsonProperty(PropertyName = "recipient")]
        public FacebookBotUser Recipient { get; set; } = new FacebookBotUser();

        /// <summary>
        /// Gets or sets the ID of the sender.
        /// </summary>
        /// <value>The ID of the sender.</value>
        [JsonProperty(PropertyName = "sender")]
        public FacebookBotUser Sender { get; set; } = new FacebookBotUser();

        /// <summary>
        /// Gets or sets the message to be sent.
        /// </summary>
        /// <value>The message.</value>
        [JsonProperty(PropertyName = "message")]
        public Message Message { get; set; }

        /// <summary>
        /// Gets or sets the messaging type.
        /// </summary>
        /// <value>The messaging type.</value>
        [JsonProperty(PropertyName = "messaging_type")]
        public string MessagingType { get; set; }

        /// <summary>
        /// Gets or sets a tag to the message.
        /// </summary>
        /// <value>The tag.</value>
        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the notification type.
        /// </summary>
        /// <value>The notification type.</value>
        [JsonProperty(PropertyName = "notification_type")]
        public string NotificationType { get; set; }

        /// <summary>
        /// Gets or sets the persona ID.
        /// </summary>
        /// <value>The persona ID.</value>
        [JsonProperty(PropertyName = "persona_id")]
        public string PersonaId { get; set; }

        /// <summary>
        /// Gets or sets the sender action.
        /// </summary>
        /// <value>The sender action (typing_on, typing_off, mark_seen).</value>
        [JsonProperty(PropertyName = "sender_action")]
        public string SenderAction { get; set; }

        /// <summary>
        /// Gets or sets the time-stamp.
        /// </summary>
        /// <value>Time-stamp.</value>
        [JsonProperty(PropertyName = "timestamp")]
        public long TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message was received while in Standby mode.
        /// </summary>
        /// <value>Value indicating whether the message was received while in Standby mode.</value>
        [JsonIgnore]
        public bool IsStandby { get; set; }

        /// <summary>
        /// Gets or sets the value of the postback property.
        /// </summary>
        /// <value>The postback payload. A postback occurs when a postback button, **Get Started** button, or persistent menu item is tapped.</value>
        [JsonProperty(PropertyName = "postback")]
        public FacebookPostBack PostBack { get; set; }

        /// <summary>
        /// Gets or sets the value of the optin property.
        /// </summary>
        /// <value>The optin field. See https://developers.facebook.com/docs/messenger-platform/reference/webhook-events/messaging_optins. </value>
        [JsonProperty(PropertyName = "optin")]
        public FacebookRecipient OptIn { get; set; }

        /// <summary>
        /// Gets or sets the contents of the pass_thread_control property.
        /// </summary>
        /// <value>A <see cref="FacebookPassThreadControl"/> holding the contents of the pass_thread_control property.</value>.
        [JsonProperty(PropertyName = "pass_thread_control")]
        public FacebookPassThreadControl PassThreadControl { get; set; }

        /// <summary>
        /// Gets or sets the contents of the take_thread_control property.
        /// </summary>
        /// <value>A <see cref="FacebookTakeThreadControl"/> holding the contents of the pass_thread_control property.</value>.
        [JsonProperty(PropertyName = "take_thread_control")]
        public FacebookTakeThreadControl TakeThreadControl { get; set; }

        /// <summary>
        ///  Gets or sets the contents of the request_thread_control property.
        /// </summary>
        /// <value>A <see cref="FacebookRequestThreadControl"/> holding the contents of the pass_thread_control property.</value>.
        [JsonProperty(PropertyName = "request_thread_control")]
        public FacebookRequestThreadControl RequestThreadControl { get; set; }

        /// <summary>
        ///  Gets or sets the contents of the message_reads property.
        /// </summary>
        /// <value>A <see cref="FacebookRead"/> holding the contents of the message_reads property.
        /// See https://developers.facebook.com/docs/messenger-platform/reference/webhook-events/message-reads. </value>.
        [JsonProperty(PropertyName = "read")]
        public FacebookRead Reads { get; set; }
    }
}
