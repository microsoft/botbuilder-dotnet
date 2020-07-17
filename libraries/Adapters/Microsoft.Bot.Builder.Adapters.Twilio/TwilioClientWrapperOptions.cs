// Copyright(c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Defines options for a <see cref="TwilioClientWrapper"/>.
    /// </summary>
    public class TwilioClientWrapperOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioClientWrapperOptions"/> class.
        /// </summary>
        /// <param name="twilioNumber">The twilio phone number.</param>
        /// <param name="twilioAccountSid">The account id.</param>
        /// <param name="twilioAuthToken">The authentication token.</param>
        /// <param name="twilioValidationUrl">The validation URL for incoming requests.</param>
        public TwilioClientWrapperOptions(string twilioNumber, string twilioAccountSid, string twilioAuthToken, Uri twilioValidationUrl = null)
        {
            if (string.IsNullOrWhiteSpace(twilioNumber))
            {
                throw new ArgumentException($"The {nameof(twilioNumber)} property for {nameof(TwilioAdapterOptions)} can't be empty.", nameof(twilioNumber));
            }

            if (string.IsNullOrWhiteSpace(twilioAccountSid))
            {
                throw new ArgumentException($"The {nameof(twilioNumber)} property for {nameof(TwilioAdapterOptions)} can't be empty.", nameof(twilioAccountSid));
            }

            if (string.IsNullOrWhiteSpace(twilioAuthToken))
            {
                throw new ArgumentException($"The {nameof(twilioAuthToken)} property for {nameof(TwilioAdapterOptions)} can't be empty.", nameof(twilioAuthToken));
            }

            TwilioNumber = twilioNumber;
            TwilioAccountSid = twilioAccountSid;
            TwilioAuthToken = twilioAuthToken;
            TwilioValidationUrl = twilioValidationUrl;
        }

        /// <summary>
        /// Gets or sets the phone number associated with this Twilio app.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        public string TwilioNumber { get; set; }

        /// <summary>
        /// Gets or sets the account SID from the Twilio account.
        /// </summary>
        /// <value>The account SID.</value>
        public string TwilioAccountSid { get; set; }

        /// <summary>
        /// Gets or sets the API auth token associated with the Twilio account.
        /// </summary>
        /// <value>The authentication token.</value>
        public string TwilioAuthToken { get; set; }

        /// <summary>
        /// Gets or sets an optional validation URL.
        /// </summary>
        /// <value>Optional validation URL to override the automatically generated URL signature used
        /// to validate incoming requests. See the Twilio security documentation on
        /// [validating requests](https://www.twilio.com/docs/usage/security#validating-requests).</value>
        public Uri TwilioValidationUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether incoming requests are validated as being genuine requests from Twilio.
        /// </summary>
        /// <value>
        /// A flag indicating whether incoming requests are validated as being genuine requests from Twilio.
        /// </value>
        public bool ValidateIncomingRequests { get; set; } = true;
    }
}
