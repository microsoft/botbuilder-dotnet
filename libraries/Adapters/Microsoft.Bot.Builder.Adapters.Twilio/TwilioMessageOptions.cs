using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public class TwilioMessageOptions
    {
        /// <summary>The destination phone number.</summary>
        public string To { get; set;  }

        /// <summary>The phone number that initiated the message.</summary>
        public string From { get; set; }

        /// <summary>
        /// The text of the message you want to send. Can be up to 1,600 characters in length.
        /// </summary>
        public string Body { get; set; }

        /// <summary>The URL of the media to send with the message.</summary>
        public List<Uri> MediaUrl { get; } = new List<Uri>();

        /// <summary>The application to use for callbacks.</summary>
        public string ApplicationSid { get; set; }
    }
}
