// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Interface to represent the format of the options for the Twilio adapter.
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
        /// Gets or sets the account Sid from the Twilio account.
        /// </summary>
        /// <value>The account Sid.</value>
        string AccountSid { get; set; }

        /// <summary>
        /// Gets or sets the API auth token associated with the Twilio account.
        /// </summary>
        /// <value>The authentication token.</value>
        string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets an optional validation url.
        /// </summary>
        /// <value>An optional url to override the automatically generated url signature used to validate incoming requests. See Twilio docs (https://www.twilio.com/docs/usage/security#validating-requests).</value>
        string ValidationUrl { get; set; }
    }
}
