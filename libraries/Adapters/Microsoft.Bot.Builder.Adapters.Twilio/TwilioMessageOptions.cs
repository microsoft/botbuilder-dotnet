// Copyright(c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Represents an outgoing message content and options for Twilio.
    /// </summary>
    public class TwilioMessageOptions
    {
        /// <summary>
        /// Gets or sets the destination phone number.
        /// </summary>
        /// <value>
        /// The destination phone number.
        /// </value>
        public string To { get; set;  }

        /// <summary>
        /// Gets or sets the phone number that initiated the message.
        /// </summary>
        /// <value>
        /// The phone number that initiated the message.
        /// </value>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the text of the message you want to send. Can be up to 1,600 characters in length.
        /// </summary>
        /// <value>
        /// The text of the message you want to send. Can be up to 1,600 characters in length.
        /// </value>
        public string Body { get; set; }

        /// <summary>
        /// Gets the URL of the media to send with the message.
        /// </summary>
        /// <value>
        /// The URL of the media to send with the message.
        /// </value>
        public List<Uri> MediaUrl { get; } = new List<Uri>();

        /// <summary>
        /// Gets or sets the application to use for callbacks.
        /// </summary>
        /// <value>
        /// The application to use for callbacks.
        /// </value>
        public string ApplicationSid { get; set; }
    }
}
