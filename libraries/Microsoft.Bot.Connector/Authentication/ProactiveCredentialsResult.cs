// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// The result from a call to create credentials for making a proactive Bot Framework Protocol request.
    /// </summary>
    public class ProactiveCredentialsResult
    {
        /// <summary>
        /// Gets or sets a value for the Credentials.
        /// </summary>
        /// <value>
        /// A value for the Credentials.
        /// </value>
        public ServiceClientCredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets a value for the Scope.
        /// </summary>
        /// <value>
        /// A value for the Scope.
        /// </value>
        public string Scope { get; set; }
    }
}
