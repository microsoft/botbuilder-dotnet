// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A factory class used to create ConnectorClients with appropriate credentials for the current appId.
    /// </summary>
    public abstract class ConnectorFactory
    {
        /// <summary>
        /// A factory method used to create <see cref="IConnectorClient"/> instances.
        /// </summary>
        /// <param name="serviceUrl">The url for the client.</param>
        /// <param name="audience">The audience for the credentials the client will use.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A Task of <see cref="IConnectorClient"/>.</returns>
        public abstract Task<IConnectorClient> CreateAsync(string serviceUrl, string audience, CancellationToken cancellationToken);
    }
}
