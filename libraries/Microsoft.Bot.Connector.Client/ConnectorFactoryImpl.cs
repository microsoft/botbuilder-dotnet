// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Connector.Client.Authentication;

namespace Microsoft.Bot.Connector.Client
{
    /// <inheritdoc />
    internal class ConnectorFactoryImpl : ConnectorFactory
    {
        private readonly BotFrameworkCredential _credential;

        public ConnectorFactoryImpl(BotFrameworkCredential credential)
        {
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        }

        /// <inheritdoc />
        public override ConnectorClient CreateConnectorClient(Uri serviceUrl, string audience)
        {
            return new ConnectorClientImpl(_credential, audience, serviceUrl);
        }

        /// <inheritdoc />
        public override UserTokenClient CreateUserTokenClient(Uri oAuthUrl, string appId)
        {
            return new UserTokenClientImpl(_credential, appId, oAuthUrl);
        }
    }
}
