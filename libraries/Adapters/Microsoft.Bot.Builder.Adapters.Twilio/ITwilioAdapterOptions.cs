// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public interface ITwilioAdapterOptions
    {
        /// <summary>
        /// Gets or sets the Twilio phone number.
        /// </summary>
        /// <value>
        /// The Twilio phone number.
        /// </value>
        string TwilioNumber { get; set; }

        /// <summary>
        /// Gets or sets the account SID.
        /// </summary>
        /// <value>The account SID.</value>
        string AccountSID { get; set; }

        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        /// <value>The authentication token.</value>
        string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the validation URL.
        /// </summary>
        /// <value>The validation URL.</value>
        string ValidationURL { get; set; }
    }
}
