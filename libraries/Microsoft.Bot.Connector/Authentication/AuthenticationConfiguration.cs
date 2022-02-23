// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// General configuration settings for authentication.
    /// </summary>
    /// <remarks>
    /// Note that this is explicitly a class and not an interface,
    /// since interfaces don't support default values, after the initial release any change would break backwards compatibility.
    /// </remarks>
    public class AuthenticationConfiguration
    {
        /// <summary>
        /// Gets an array of JWT endorsements.
        /// </summary>
        /// <value>
        /// An array of JWT endorsements.
        /// </value>
        public Collection<string> RequiredEndorsements { get; } = new Collection<string>();

        /// <summary>
        /// Gets or sets an <see cref="ClaimsValidator"/> instance used to validate the identity claims.
        /// </summary>
        /// <value>
        /// An <see cref="ClaimsValidator"/> instance used to validate the identity claims.
        /// </value>
        public virtual ClaimsValidator ClaimsValidator { get; set; }

        /// <summary>
        /// Gets or sets a collection of valid JWT token issuers.
        /// </summary>
        /// <value>
        /// A collection of valid JWT token issuers.
        /// </value>
        public IEnumerable<string> ValidTokenIssuers { get; set; }
    }
}
