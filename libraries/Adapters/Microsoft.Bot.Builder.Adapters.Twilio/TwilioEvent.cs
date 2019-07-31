// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// A class wrapping all the Twilio SMS event properties.
    /// </summary>
    public class TwilioEvent
    {
        /// <summary>
        /// Gets or sets the receiver's country.
        /// </summary>
        /// <value>The receiver's country. E.g. "US".</value>
        public string ToCountry { get; set; }

        /// <summary>
        /// Gets or sets the sender's country.
        /// </summary>
        /// <value>The sender's country. E.g. "US".</value>
        public string FromCountry { get; set; }

        /// <summary>
        /// Gets or sets the receiver's geographical State.
        /// </summary>
        /// <value>The receiver's geographical State. E.g. "NY".</value>
        public string ToState { get; set; }

        /// <summary>
        /// Gets or sets a SMS message Sid.
        /// </summary>
        /// <value>The SMS message security identifier.</value>
        public string SmsMessageSid { get; set; }

        /// <summary>
        /// Gets or sets the number of media files associated with the message.
        /// </summary>
        /// <value>The number of media files associated with the message. A message can send up to 10 media files.</value>
        public string NumMedia { get; set; }

        /// <summary>
        /// Gets or sets the URL when there's media such as images, associated with the message.
        /// </summary>
        /// <value>URL of any media if present.</value>
        public string MediaUrl { get; set; }

        /// <summary>
        /// Gets or sets the receiver's city.
        /// </summary>
        /// <value>The receiver's city. E.g. "FARMINGDALE".</value>
        public string ToCity { get; set; }

        /// <summary>
        /// Gets or sets the sender's ZIP postal code.
        /// </summary>
        /// <value>The sender's ZIP postal code. </value>
        public string FromZip { get; set; }

        /// <summary>
        /// Gets or sets the sms Sid.
        /// </summary>
        /// <value>The security identifier of the sms (see https://www.twilio.com/docs/glossary/what-is-a-sid).</value>
        public string SmsSid { get; set; }

        /// <summary>
        /// Gets or sets the sender's geographical State.
        /// </summary>
        /// <value>The sender's geographical State. E.g. "NY".</value>
        public string FromState { get; set; }

        /// <summary>
        /// Gets or sets the status of the message.
        /// </summary>
        /// <value>The status of the message. E.g. "received".</value>
        public string SmsStatus { get; set; }

        /// <summary>
        /// Gets or sets the sender's city.
        /// </summary>
        /// <value>The sender's city. E.g. "FARMINGDALE".</value>
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
        /// Gets or sets the recipient's zip code.
        /// </summary>
        /// <value>The recipient's zip code.</value>
        public string ToZip { get; set; }

        /// <summary>
        /// Gets or sets the number of segments that make up the complete message.
        /// </summary>
        /// <value>The number of segments that make up the complete message.</value>
        public string NumSegments { get; set; }

        /// <summary>
        /// Gets or sets the Sid of the message.
        /// </summary>
        /// <value>The security identifier of the message (see https://www.twilio.com/docs/glossary/what-is-a-sid).</value>
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
        /// Gets or sets a specific type of event.
        /// </summary>
        /// <value>The event type.</value>
        public string EventType { get; set; }
    }
}
