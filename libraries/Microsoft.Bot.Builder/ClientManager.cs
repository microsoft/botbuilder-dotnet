// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// Helper class to manage clients such as ConnectorClient.
    /// </summary>
    public class ClientManager
    {
        private readonly BotFrameworkAdapter adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientManager"/> class,
        /// using a <see cref="BotFrameworkAdapter"/>.
        /// </summary>
        /// <param name="adapter">The BotframeworkAdapter instance.</param>
        public ClientManager(BotFrameworkAdapter adapter)
        {
            this.adapter = adapter;
        }

        /// <summary>
        /// Creates the connector client asynchronous.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="claimsIdentity">The claims claimsIdentity.</param>
        /// <param name="audience">The audience to use.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ConnectorClient instance.</returns>
        /// <exception cref="NotSupportedException">ClaimsIdentity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.</exception>
        public Task<IConnectorClient> CreateConnectorClientAsync(string serviceUrl, ClaimsIdentity claimsIdentity, string audience, CancellationToken cancellationToken = default)
        {
            return this.adapter.CreateConnectorClientAsync(serviceUrl, claimsIdentity, audience, cancellationToken);
        }
    }
}
