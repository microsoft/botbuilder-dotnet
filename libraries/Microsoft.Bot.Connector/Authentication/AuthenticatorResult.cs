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
#pragma warning disable SA1609 // Property documentation should have value
        public string AccessToken { get; set;  }
#pragma warning restore SA1609 // Property documentation should have value

        /// <summary>
        /// Gets or sets the date and time of expiration relative to Coordinated Universal Time (UTC).
        /// </summary>
#pragma warning disable SA1609 // Property documentation should have value
        public DateTimeOffset ExpiresOn { get; set; }
#pragma warning restore SA1609 // Property documentation should have value
    }
}
