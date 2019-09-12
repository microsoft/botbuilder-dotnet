// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// A class wrapping Twilio request parameters.
    /// </summary>
    /// <remarks>These parameters can be included in an HTTP request that contains a Twilio message.</remarks>
    public class TwilioMessage
    {
        /// <summary>
        /// Gets or sets the Author of the message.
        /// </summary>
        /// <value>The Author of the message.</value>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the receiver's country.
        /// </summary>
        /// <value>The receiver's country, such as "US".</value>
        public string ToCountry { get; set; }

        /// <summary>
        /// Gets or sets the sender's country.
        /// </summary>
        /// <value>The sender's country, such as "US".</value>
        public string FromCountry { get; set; }

        /// <summary>
        /// Gets or sets the receiver's state or province.
        /// </summary>
        /// <value>The receiver's state or province, such as "NY".</value>
        public string ToState { get; set; }

        /// <summary>
        /// Gets or sets the `sms_id` found in the response of a phone verification start.
        /// </summary>
        /// <value>The`sms_id` found in the response of a phone verification start.</value>
        public string SmsMessageSid { get; set; }

        /// <summary>
        /// Gets or sets the number of media files associated with the message.
        /// </summary>
        /// <value>The number of media files associated with the message.</value>
        /// <remarks>A message can include up to 10 media files.</remarks>
        public string NumMedia { get; set; }

        /// <summary>
        /// Gets or sets the URLs referencing the media content included with the message, if any.
        /// </summary>
        /// <value>URLs referencing the media content included with the message.</value>
        public List<Uri> MediaUrls { get; set; }

        /// <summary>
        /// Gets or sets the content types for the media included with the message, if any.
        /// </summary>
        /// <value>The content types for the media included with the message.</value>
        public List<string> MediaContentTypes { get; set; }

        /// <summary>
        /// Gets or sets the receiver's city.
        /// </summary>
        /// <value>The receiver's city, such as "FARMINGDALE".</value>
        public string ToCity { get; set; }

        /// <summary>
        /// Gets or sets the sender's postal code.
        /// </summary>
        /// <value>The sender's postal code. </value>
        public string FromZip { get; set; }

        /// <summary>
        /// Gets or sets the SMS security identifier.
        /// </summary>
        /// <value>The SMS message security identifier.</value>
        /// <remarks>Same as the <see cref="MessageSid"/>.</remarks>
        public string SmsSid { get; set; }

        /// <summary>
        /// Gets or sets the sender's state or province.
        /// </summary>
        /// <value>The sender's state or province, such as "NY".</value>
        public string FromState { get; set; }

        /// <summary>
        /// Gets or sets the status of the message.
        /// </summary>
        /// <value>The status of the message, such as "received".</value>
        /// <remarks>See [message status values](https://aka.ms/twilio-message-status-values)
        /// for a list of the possible values.</remarks>
        public string SmsStatus { get; set; }

        /// <summary>
        /// Gets or sets the sender's city.
        /// </summary>
        /// <value>The sender's city, such as "FARMINGDALE".</value>
        public string FromCity { get; set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        /// <value>The message text. Can be up to 1,600 characters long.</value>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the phone number in E.164 format that received the message.
        /// </summary>
        /// <value>The phone number in E.164 format that received the message.</value>
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the recipient's postal code.
        /// </summary>
        /// <value>The recipient's postal code.</value>
        public string ToZip { get; set; }

        /// <summary>
        /// Gets or sets the number of segments that make up the complete message.
        /// </summary>
        /// <value>The number of segments that make up the complete message.</value>
        public string NumSegments { get; set; }

        /// <summary>
        /// Gets or sets the security identifier of the message.
        /// </summary>
        /// <value>The security identifier of the message.</value>
        /// <remarks>For more information, see [Security Identifier (SID)](https://aka.ms/twilio-sid).
        /// </remarks>
        public string MessageSid { get; set; }

        /// <summary>
        /// Gets or sets the Sid of the Account that sent the message that created the resource.
        /// </summary>
        /// <value>The security identifier of the Account that sent the message.</value>
        public string AccountSid { get; set; }

        /// <summary>
        /// Gets or sets the sender phone number.
        /// </summary>
        /// <value>The phone number (in E.164 format), alphanumeric sender ID, or Wireless SIM that initiated the message.</value>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the API version used to process the message.
        /// </summary>
        /// <value>The API version used to process the message.</value>
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the event type for using with Twilio Conversation API.
        /// </summary>
        /// <value>The type of event, e.g. "onMessageAdd", "onMessageAdded", "onConversationAdd".</value>
        public string EventType { get; set; }
    }
}
