// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Client.Models;

namespace Microsoft.Bot.Connector.Client.Authentication
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
        /// Validate Bot Framework Protocol requests.
        /// </summary>
        /// <param name="authHeader">The http auth header.</param>
        /// <param name="channelIdHeader">The channel Id HTTP header.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Asynchronous Task with <see cref="AuthenticateRequestResult"/>.</returns>
        /// <exception cref="UnauthorizedAccessException">If the validation returns false.</exception>
        public abstract Task<AuthenticateRequestResult> AuthenticateStreamingRequestAsync(string authHeader, string channelIdHeader, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a <see cref="ConnectorFactory"/> that can be used to create <see cref="ConnectorClient"/> and <see cref="UserTokenClient"/> that use credentials from this particular cloud environment.
        /// </summary>
        /// <param name="claimsIdentity">The inbound <see cref="Activity"/>'s <see cref="ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="ConnectorFactoryImpl"/>.</returns>
        public abstract ConnectorFactory CreateConnectorFactory(ClaimsIdentity claimsIdentity);

        /// <summary>
        /// Creates the appropriate <see cref="UserTokenClient" /> instance.
        /// </summary>
        /// <param name="claimsIdentity">The inbound <see cref="Activity"/>'s <see cref="ClaimsIdentity"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Asynchronous Task with <see cref="UserTokenClient" /> instance.</returns>
        public abstract Task<UserTokenClient> CreateUserTokenClientAsync(ClaimsIdentity claimsIdentity, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a <see cref="BotFrameworkClient"/> used for calling Skills.
        /// </summary>
        /// <returns>A <see cref="BotFrameworkClient"/> instance to call Skills.</returns>
        public abstract BotFrameworkClient CreateBotFrameworkClient();

        /// <summary>
        /// Gets the originating audience from Bot OAuth scope.
        /// </summary>
        /// <returns>The originating audience.</returns>
        public abstract string GetOriginatingAudience();

        /// <summary>
        /// Authenticate Bot Framework Protocol requests to Skills.
        /// </summary>
        /// <param name="authHeader">The http auth header received in the skill request.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Asynchronous Task with <see cref="ClaimsIdentity"/>.</returns>
        public abstract Task<ClaimsIdentity> AuthenticateChannelRequestAsync(string authHeader, CancellationToken cancellationToken);
    }
}
