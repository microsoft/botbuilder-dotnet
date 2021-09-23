// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

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
        /// Validate Bot Framework Protocol requests.
        /// </summary>
        /// <param name="authHeader">The http auth header.</param>
        /// <param name="channelIdHeader">The channel Id HTTP header.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Asynchronous Task with <see cref="AuthenticateRequestResult"/>.</returns>
        /// <exception cref="UnauthorizedAccessException">If the validation returns false.</exception>
        public abstract Task<AuthenticateRequestResult> AuthenticateStreamingRequestAsync(string authHeader, string channelIdHeader, CancellationToken cancellationToken);

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

        /// <summary>
        /// Creates a <see cref="BotFrameworkClient"/> used for calling Skills.
        /// </summary>
        /// <returns>A <see cref="BotFrameworkClient"/> instance to call Skills.</returns>
        public virtual BotFrameworkClient CreateBotFrameworkClient()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the originating audience from Bot OAuth scope.
        /// </summary>
        /// <returns>The originating audience.</returns>
        public virtual string GetOriginatingAudience()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authenticate Bot Framework Protocol requests to Skills.
        /// </summary>
        /// <param name="authHeader">The http auth header received in the skill request.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Asynchronous Task with <see cref="ClaimsIdentity"/>.</returns>
        public virtual Task<ClaimsIdentity> AuthenticateChannelRequestAsync(string authHeader, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a <see cref="StreamingConnection"/> that uses web sockets.
        /// </summary>
        /// <param name="httpContext"><see cref="HttpContext"/> instance on which to accept the web socket.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        /// <returns><see cref="StreamingConnection"/> that uses web socket.</returns>
        public virtual Task<StreamingConnection> CreateWebSocketConnectionAsync(HttpContext httpContext, ILogger logger)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            return Task.FromResult<StreamingConnection>(new WebSocketStreamingConnection(httpContext, logger));
        }

        /// <summary>
        /// Creates a <see cref="StreamingConnection"/> that uses named pipes.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        /// <returns>A <see cref="StreamingConnection"/> that uses named pipes.</returns>
        public virtual StreamingConnection CreateNamedPipeConnection(string pipeName, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            return new LegacyStreamingConnection(pipeName, logger);
        }

        /// <summary>
        /// Generates the appropriate callerId to write onto the activity, this might be null.
        /// </summary>
        /// <param name="credentialFactory">A <see cref="ServiceClientCredentialsFactory"/> to use.</param>
        /// <param name="claimsIdentity">The inbound claims.</param>
        /// <param name="callerId">The default callerId to use if this is not a skill.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The callerId, this might be null.</returns>
        protected internal async Task<string> GenerateCallerIdAsync(ServiceClientCredentialsFactory credentialFactory, ClaimsIdentity claimsIdentity, string callerId, CancellationToken cancellationToken)
        {
            // Is the bot accepting all incoming messages?
            if (await credentialFactory.IsAuthenticationDisabledAsync(cancellationToken).ConfigureAwait(false))
            {
                // Return null so that the callerId is cleared.
                return null;
            }

            // Is the activity from another bot?
            return SkillValidation.IsSkillClaim(claimsIdentity.Claims)
                ? $"{CallerIdConstants.BotToBotPrefix}{JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims)}"
                : callerId;
        }
    }
}
