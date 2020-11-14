// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

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
        /// Gets or sets an array of JWT endorsements.
        /// </summary>
        /// <value>
        /// An array of JWT endorsements.
        /// </value>
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public string[] RequiredEndorsements { get; set; } = Array.Empty<string>();
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets an <see cref="ClaimsValidator"/> instance used to validate the identity claims.
        /// </summary>
        /// <value>
        /// An <see cref="ClaimsValidator"/> instance used to validate the identity claims.
        /// </value>
        public virtual ClaimsValidator ClaimsValidator { get; set; } = null;
    }
}
