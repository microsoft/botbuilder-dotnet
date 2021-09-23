// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Creates a <see cref="BotFrameworkAuthentication"/> instance from configuration.
    /// </summary>
    public class ConfigurationBotFrameworkAuthentication : BotFrameworkAuthentication
    {
        private readonly BotFrameworkAuthentication _inner;
        private readonly bool _useLegacyStreamingConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBotFrameworkAuthentication"/> class.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="credentialsFactory">An <see cref="ServiceClientCredentialsFactory"/> instance.</param>
        /// <param name="authConfiguration">An <see cref="AuthenticationConfiguration"/> instance.</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> to use.</param>
        /// <param name="logger">The ILogger instance to use.</param>
        public ConfigurationBotFrameworkAuthentication(IConfiguration configuration, ServiceClientCredentialsFactory credentialsFactory = null, AuthenticationConfiguration authConfiguration = null, IHttpClientFactory httpClientFactory = null, ILogger logger = null)
        {
            var channelService = configuration.GetSection("ChannelService")?.Value;
            var validateAuthority = configuration.GetSection("ValidateAuthority")?.Value;
            var toChannelFromBotLoginUrl = configuration.GetSection("ToChannelFromBotLoginUrl")?.Value;
            var toChannelFromBotOAuthScope = configuration.GetSection("ToChannelFromBotOAuthScope")?.Value;
            var toBotFromChannelTokenIssuer = configuration.GetSection("ToBotFromChannelTokenIssuer")?.Value;
            var oAuthUrl = configuration.GetSection("OAuthUrl")?.Value;
            var toBotFromChannelOpenIdMetadataUrl = configuration.GetSection("ToBotFromChannelOpenIdMetadataUrl")?.Value;
            var toBotFromEmulatorOpenIdMetadataUrl = configuration.GetSection("ToBotFromEmulatorOpenIdMetadataUrl")?.Value;
            var callerId = configuration.GetSection("CallerId")?.Value;

            _inner = BotFrameworkAuthenticationFactory.Create(
                channelService,
                bool.Parse(validateAuthority ?? "true"),
                toChannelFromBotLoginUrl,
                toChannelFromBotOAuthScope,
                toBotFromChannelTokenIssuer,
                oAuthUrl,
                toBotFromChannelOpenIdMetadataUrl,
                toBotFromEmulatorOpenIdMetadataUrl,
                callerId,
                credentialsFactory ?? new ConfigurationServiceClientCredentialFactory(configuration),
                authConfiguration ?? new AuthenticationConfiguration(),
                httpClientFactory,
                logger);

            // A circuit breaker / emergency switch to go back to the legacy implementation of streaming connection
            // while we transition to the new pipelines-based implementation.
            var useLegacyStreamingConnection = configuration.GetSection("UseLegacyStreamingConnection")?.Value;
            _useLegacyStreamingConnection = bool.Parse(useLegacyStreamingConnection ?? bool.FalseString);
        }

        /// <inheritdoc />
        public override string GetOriginatingAudience()
        {
            return _inner.GetOriginatingAudience();
        }

        /// <inheritdoc />
        public override Task<ClaimsIdentity> AuthenticateChannelRequestAsync(string authHeader, CancellationToken cancellationToken)
        {
            return _inner.AuthenticateChannelRequestAsync(authHeader, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<AuthenticateRequestResult> AuthenticateRequestAsync(Activity activity, string authHeader, CancellationToken cancellationToken)
        {
            return _inner.AuthenticateRequestAsync(activity, authHeader, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<AuthenticateRequestResult> AuthenticateStreamingRequestAsync(string authHeader, string channelIdHeader, CancellationToken cancellationToken)
        {
            return _inner.AuthenticateStreamingRequestAsync(authHeader, channelIdHeader, cancellationToken);
        }

        /// <inheritdoc/>
        public override ConnectorFactory CreateConnectorFactory(ClaimsIdentity claimsIdentity)
        {
            return _inner.CreateConnectorFactory(claimsIdentity);
        }

        /// <inheritdoc/>
        public override Task<UserTokenClient> CreateUserTokenClientAsync(ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
        {
            return _inner.CreateUserTokenClientAsync(claimsIdentity, cancellationToken);
        }

        /// <inheritdoc/>
        public override BotFrameworkClient CreateBotFrameworkClient()
        {
            return _inner.CreateBotFrameworkClient();
        }

        /// <inheritdoc />
        public override async Task<StreamingConnection> CreateWebSocketConnectionAsync(HttpContext httpContext, ILogger logger)
        {
            if (_useLegacyStreamingConnection)
            {
                var socket = await httpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                return new LegacyStreamingConnection(socket, logger);
            }

            return await base.CreateWebSocketConnectionAsync(httpContext, logger).ConfigureAwait(false);
        }
    }
}
