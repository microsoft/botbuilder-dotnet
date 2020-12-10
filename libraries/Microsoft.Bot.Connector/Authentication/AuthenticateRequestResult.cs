// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Security.Claims;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// The result from a call to authenticate a Bot Framework Protocol request.
    /// </summary>
    public class AuthenticateRequestResult
    {
        /// <summary>
        /// Gets or sets a value for the Scope.
        /// </summary>
        /// <value>
        /// A value for the Scope.
        /// </value>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets a value for the ClaimsIdentity.
        /// </summary>
        /// <value>
        /// A value for the ClaimsIdentity.
        /// </value>
        public ClaimsIdentity ClaimsIdentity { get; set; }

        /// <summary>
        /// Gets or sets a value for the CallerId.
        /// </summary>
        /// <value>
        /// A value for the CallerId.
        /// </value>
        public string CallerId { get; set; }

        /// <summary>
        /// Gets or sets a value for the ConnectorFactory.
        /// </summary>
        /// <value>
        /// A value for the ConnectorFactory.
        /// </value>
        public ConnectorFactory ConnectorFactory { get; set; }
    }
}
