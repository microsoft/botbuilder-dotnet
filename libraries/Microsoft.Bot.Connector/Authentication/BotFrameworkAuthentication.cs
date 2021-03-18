// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
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
        /// Gets the originating audience from Bot OAuth scope.
        /// </summary>
        /// <returns>The originating audience.</returns>
        public abstract string GetOriginatingAudience();

        /// <summary>
        /// Gets the AAD app credentials for Bot Framework protocol requests.
        /// </summary>
        /// <param name="appId">The Microsoft app ID.</param>
        /// <param name="client">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="oAuthScope">The scope for the token.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The app credentials.</returns>
        public abstract Task<AppCredentials> GetAppCredentialsAsync(string appId, HttpClient client, string oAuthScope, CancellationToken cancellationToken);

        /// <summary>
        /// Validate Bot Framework Protocol requests.
        /// </summary>
        /// <param name="authHeader">The http auth header.</param>
        /// <param name="isSkillCallback">Whether this is call is from a skill callback.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Asynchronous Task with <see cref="ClaimsIdentity"/>.</returns>
        public abstract Task<ClaimsIdentity> ValidateAuthHeaderAsync(string authHeader, bool isSkillCallback, CancellationToken cancellationToken);

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
