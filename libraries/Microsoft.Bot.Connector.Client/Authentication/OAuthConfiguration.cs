// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Client.Authentication
{
    /// <summary>
    /// Configuration for OAuth client credential authentication.
    /// </summary>
    public class OAuthConfiguration
    {
        /// <summary>
        /// Gets or sets oAuth Authority for authentication.
        /// </summary>
        /// <value>
        /// OAuth Authority for authentication.
        /// </value>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Authority should be validated.
        /// </summary>
        /// <value>
        /// Boolean value indicating whether the Authority should be validated.
        /// </value>
        public bool ValidateAuthority { get; set; } = true;

        /// <summary>
        /// Gets or sets oAuth scope for authentication.
        /// </summary>
        /// <value>
        /// OAuth scope for authentication.
        /// </value>
        public string Scope { get; set; }
    }
}
