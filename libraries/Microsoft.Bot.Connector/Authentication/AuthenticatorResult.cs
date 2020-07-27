// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Represents the result of an authentication process. Includes a security token and its expiration time.
    /// </summary>
    public class AuthenticatorResult
    {
        /// <summary>
        /// Gets or sets the value of the access token resulting from an authentication process.
        /// </summary>
        /// <value>
        /// The value of the access token resulting from an authentication process.
        /// </value>
        public string AccessToken { get; set;  }

        /// <summary>
        /// Gets or sets the date and time of expiration relative to Coordinated Universal Time (UTC).
        /// </summary>
        /// <value>
        /// The date and time of expiration relative to Coordinated Universal Time (UTC).
        /// </value>
        public DateTimeOffset ExpiresOn { get; set; }
    }
}
