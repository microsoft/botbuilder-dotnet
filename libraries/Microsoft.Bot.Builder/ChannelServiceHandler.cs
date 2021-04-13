// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A class to help with the implementation of the Bot Framework protocol.
    /// </summary>
    public class ChannelServiceHandler : ChannelServiceHandlerBase
    {
        private readonly AuthenticationConfiguration _authConfiguration;
        private readonly ICredentialProvider _credentialProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelServiceHandler"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfiguration">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
        public ChannelServiceHandler(
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfiguration,
            IChannelProvider channelProvider = null)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _authConfiguration = authConfiguration ?? throw new ArgumentNullException(nameof(authConfiguration));
            ChannelProvider = channelProvider;
        }

        /// <summary>
        /// Gets the channel provider that implements <see cref="IChannelProvider"/>.
        /// </summary>
        /// <value>
        /// The channel provider that implements <see cref="IChannelProvider"/>.
        /// </value>
        protected IChannelProvider ChannelProvider { get; }

        /// <summary>
        /// Helper to authenticate the header.
        /// </summary>
        /// <remarks>
        /// This code is very similar to the code in <see cref="JwtTokenValidation.AuthenticateRequest(IActivity, string, ICredentialProvider, IChannelProvider, AuthenticationConfiguration, HttpClient)"/>,
        /// we should move this code somewhere in that library when we refactor auth, for now we keep it private to avoid adding more public static
        /// functions that we will need to deprecate later.
        /// </remarks>
        /// <param name="authHeader">The auth header containing JWT token.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="ClaimsIdentity"/> representing the claims associated with given header.</returns>
        internal override async Task<ClaimsIdentity> AuthenticateAsync(string authHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                var isAuthDisabled = await _credentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false);
                if (!isAuthDisabled)
                {
                    // No auth header. Auth is required. Request is not authorized.
                    throw new UnauthorizedAccessException();
                }

                // In the scenario where auth is disabled, we still want to have the
                // IsAuthenticated flag set in the ClaimsIdentity.
                // To do this requires adding in an empty claim.
                // Since ChannelServiceHandler calls are always a skill callback call, we set the skill claim too.
                return SkillValidation.CreateAnonymousSkillClaim();
            }

            // Validate the header and extract claims.
            return await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialProvider, ChannelProvider, "unknown", _authConfiguration).ConfigureAwait(false);
        }
    }
}
