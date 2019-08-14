// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Defines values that a <see cref="TwilioAdapter"/> can use to connect to Twilio's SMS service.
    /// </summary>
    public interface ITwilioAdapterOptions
    {
        /// <summary>
        /// Gets or sets the phone number associated with this Twilio app.
        /// </summary>
        /// <value>
        /// The phone number, in the format 1XXXYYYZZZZ.
        /// </value>
        string TwilioNumber { get; set; }

        /// <summary>
        /// Gets or sets the account SID from the Twilio account.
        /// </summary>
        /// <value>The account SID.</value>
        string AccountSid { get; set; }

        /// <summary>
        /// Gets or sets the API auth token associated with the Twilio account.
        /// </summary>
        /// <value>The authentication token.</value>
        string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets an optional validation URL.
        /// </summary>
        /// <value>Optional validation URL to override the automatically generated URL signature used
        /// to validate incoming requests. See the Twilio security documentation on
        /// [validating requests](https://www.twilio.com/docs/usage/security#validating-requests).</value>
        string ValidationUrl { get; set; }
    }
}
