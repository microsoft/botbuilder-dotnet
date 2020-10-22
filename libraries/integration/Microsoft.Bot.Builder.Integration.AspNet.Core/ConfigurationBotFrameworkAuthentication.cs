// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Creates a <see cref="BotFrameworkAuthentication"/> instance from configuration.
    /// </summary>
    public class ConfigurationBotFrameworkAuthentication : BotFrameworkAuthentication
    {
        private readonly BotFrameworkAuthentication _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBotFrameworkAuthentication"/> class.
        /// </summary>
        /// <param name="configuration">An IConfiguration instance.</param>
        /// <param name="credentialsFactory">An IServiceClientCredentialsFactory instance.</param>
        /// <param name="authConfiguration">An AuthenticationConfiguration instance.</param>
        /// <param name="httpClient">A custom HttpClient to use.</param>
        /// <param name="logger">The ILogger instance to use.</param>
        public ConfigurationBotFrameworkAuthentication(IConfiguration configuration, ServiceClientCredentialsFactory credentialsFactory = null, AuthenticationConfiguration authConfiguration = null, HttpClient httpClient = null, ILogger logger = null)
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
                httpClient,
                logger);
        }

        /// <inheritdoc/>
        public override Task<AuthenticateRequestResult> AuthenticateRequestAsync(Activity activity, string authHeader, CancellationToken cancellationToken)
        {
            return _inner.AuthenticateRequestAsync(activity, authHeader, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<ProactiveCredentialsResult> GetProactiveCredentialsAsync(ClaimsIdentity claimsIdentity, string audience, CancellationToken cancellationToken)
        {
            return _inner.GetProactiveCredentialsAsync(claimsIdentity, audience, cancellationToken);
        }
    }
}
