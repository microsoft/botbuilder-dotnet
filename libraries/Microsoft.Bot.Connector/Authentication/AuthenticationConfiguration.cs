// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        /// Initializes a new instance of the <see cref="AuthenticationConfiguration"/> class.
        /// </summary>
        public AuthenticationConfiguration()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationConfiguration"/> class.
        /// </summary>
        /// <param name="claimsValidator">A <see cref="ClaimsValidator"/> instance used to validate claims.</param>
        public AuthenticationConfiguration(ClaimsValidator claimsValidator)
        {
            ClaimsValidator = claimsValidator;
        }

        public string[] RequiredEndorsements { get; set; } = { };

        /// <summary>
        /// Gets an <see cref="ClaimsValidator"/> instance used to validate the identity claims.
        /// </summary>
        /// <value>
        /// An <see cref="ClaimsValidator"/> instance used to validate the identity claims.
        /// </value>
        public virtual ClaimsValidator ClaimsValidator { get; }
    }
}
