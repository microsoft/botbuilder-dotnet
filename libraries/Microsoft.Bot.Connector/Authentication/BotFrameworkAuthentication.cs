// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Represents a Cloud Environment used to authenticate Bot Framework Protocol network calls within this environment.
    /// </summary>
    public abstract class BotFrameworkAuthentication
    {
        /// <summary>
        /// Validate Bot Framework Protocol requests.
        /// </summary>
        /// <param name="activity">The inbound Activity.</param>
        /// <param name="authHeader">The http auth header.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Asynchronous Task with <see cref="AuthenticateRequestResult"/>.</returns>
        /// <exception cref="UnauthorizedAccessException">If the validation returns false.</exception>
        public abstract Task<AuthenticateRequestResult> AuthenticateRequestAsync(Activity activity, string authHeader, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a <see cref="ConnectorFactory"/> that can be used to create <see cref="IConnectorClient"/> that use credentials from this particular cloud environment.
        /// </summary>
        /// <param name="claimsIdentity">The inbound <see cref="Activity"/>'s <see cref="ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="ConnectorFactory"/>.</returns>
        public abstract ConnectorFactory CreateConnectorFactory(ClaimsIdentity claimsIdentity);

        /// <summary>
        /// Creates the appropriate <see cref="UserTokenClient" /> instance.
        /// </summary>
        /// <param name="claimsIdentity">The inbound <see cref="Activity"/>'s <see cref="ClaimsIdentity"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Asynchronous Task with <see cref="UserTokenClient" /> instance.</returns>
        public abstract Task<UserTokenClient> CreateUserTokenClientAsync(ClaimsIdentity claimsIdentity, CancellationToken cancellationToken);
    }
}
