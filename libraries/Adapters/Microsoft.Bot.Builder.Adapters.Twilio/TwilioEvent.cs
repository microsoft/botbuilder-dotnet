// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public class TwilioEvent
    {
        public string ToCountry { get; set; }

        public string ToState { get; set; }

        public string SmsMessageSid { get; set; }

        public string NumMedia { get; set; }

        public string ToCity { get; set; }

        public string FromZip { get; set; }

        public string SmsSid { get; set; }

        public string FromState { get; set; }

        public string SmsStatus { get; set; }

        public string FromCity { get; set; }

        public string Body { get; set; }

        public string To { get; set; }

        public string ToZip { get; set; }

        public string NumSegments { get; set; }

        public string MessageSid { get; set; }

        public string AccountSid { get; set; }

        public string From { get; set; }

        public string ApiVersion { get; set; }
    }
}
