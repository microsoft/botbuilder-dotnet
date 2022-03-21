// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Client
{
    /// <summary>
    /// A factory to create REST clients to interact with Azure Bot Service.
    /// </summary>
    public abstract class ConnectorFactory
    {
        /// <summary>
        /// Creates a <see cref="ConnectorClient"/> that can access Bot Framework Protocol REST APIs.
        /// </summary>
        /// <param name="messagingEndpoint">The endpoint to connect to Bot Framework Protocol REST APIs.</param>
        /// <returns>A <see cref="ConnectorClient"/> instance.</returns>
        public virtual ConnectorClient CreateConnectorClient(Uri messagingEndpoint)
        {
            return new ConnectorClientImpl(messagingEndpoint);
        }

        /// <summary>
        /// Creates a <see cref="UserTokenClient"/> that can access Bot Framework Protocol Token APIs.
        /// </summary>
        /// <param name="tokenEndpoint">The endpoint to connect to Bot Framework Protocol Token APIs.</param>
        /// <param name="appId">The ID of the app to obtain token.</param>
        /// <returns>A <see cref="UserTokenClient"/> instance.</returns>
        public virtual UserTokenClient CreateUserTokenClient(Uri tokenEndpoint, string appId)
        {
            return new UserTokenClientImpl(tokenEndpoint, appId);
        }
    }
}
