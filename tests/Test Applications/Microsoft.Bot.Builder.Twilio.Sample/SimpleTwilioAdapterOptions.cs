// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using Microsoft.Bot.Builder.Adapters.Twilio;

namespace Microsoft.Bot.Builder.Twilio.Sample
{
    public class SimpleTwilioAdapterOptions : ITwilioAdapterOptions
    {
        public SimpleTwilioAdapterOptions(string twilioNumber, string accountSid, string authToken, string validationUrl)
        {
            this.TwilioNumber = twilioNumber;
            this.AccountSid = accountSid;
            this.AuthToken = authToken;
            this.ValidationUrl = validationUrl;
        }

        public string TwilioNumber { get; set; }

        public string AccountSid { get; set; }

        public string AuthToken { get; set; }

        public string ValidationUrl { get; set; }
    }
}
