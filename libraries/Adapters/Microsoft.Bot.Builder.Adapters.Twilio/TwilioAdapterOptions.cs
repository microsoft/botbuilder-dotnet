// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Defines values that a <see cref="TwilioAdapter"/> can use to connect to Twilio's SMS service.
    /// </summary>
    public class TwilioAdapterOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioAdapterOptions"/> class.
        /// </summary>
        /// <param name="twilioNumber">The twilio phone number.</param>
        /// <param name="accountSid">The account id.</param>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="validationUrl">validation URL for incoming requests.</param>
        public TwilioAdapterOptions(string twilioNumber, string accountSid, string authToken, Uri validationUrl = null)
        {
            TwilioNumber = twilioNumber;
            AccountSid = accountSid;
            AuthToken = authToken;
            ValidationUrl = validationUrl;
        }

        /// <summary>
        /// Gets or sets the phone number associated with this Twilio app.
        /// </summary>
        /// <value>
        /// The phone number, in the format 1XXXYYYZZZZ.
        /// </value>
        public string TwilioNumber { get; set; }

        /// <summary>
        /// Gets or sets the account SID from the Twilio account.
        /// </summary>
        /// <value>The account SID.</value>
        public string AccountSid { get; set; }

        /// <summary>
        /// Gets or sets the API auth token associated with the Twilio account.
        /// </summary>
        /// <value>The authentication token.</value>
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets an optional validation URL.
        /// </summary>
        /// <value>Optional validation URL to override the automatically generated URL signature used
        /// to validate incoming requests. See the Twilio security documentation on
        /// [validating requests](https://www.twilio.com/docs/usage/security#validating-requests).</value>
        public Uri ValidationUrl { get; set; }
    }
}
