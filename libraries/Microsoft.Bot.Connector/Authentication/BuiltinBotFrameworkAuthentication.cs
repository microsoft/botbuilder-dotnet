﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Note regarding porting to other languages.
    /// 
    /// This INTERNAL code leverages the existing auth implementation in the .NET repo. Ultimately, the "parameterized"
    /// version of this code can do everything this code does. In the future this "buildin" implementation will be
    /// replaced with the "parameterized" version appropriately parameterized with the builtin constants.
    /// </summary>
    internal abstract class BuiltinBotFrameworkAuthentication : BotFrameworkAuthentication
    {
        private static readonly HttpClient _authHttpClient = new HttpClient();

        private readonly string _toChannelFromBotOAuthScope;
        private readonly string _loginEndpoint;
        private readonly string _callerId;
        private readonly string _channelService;
        private readonly string _oauthEndpoint;
        private readonly ServiceClientCredentialsFactory _credentialsFactory;
        private readonly AuthenticationConfiguration _authConfiguration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        protected BuiltinBotFrameworkAuthentication(string toChannelFromBotOAuthScope, string loginEndpoint, string callerId, string channelService, string oauthEndpoint, ServiceClientCredentialsFactory credentialsFactory, AuthenticationConfiguration authConfiguration, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _toChannelFromBotOAuthScope = toChannelFromBotOAuthScope;
            _loginEndpoint = loginEndpoint;
            _callerId = callerId;
            _channelService = channelService;
            _oauthEndpoint = oauthEndpoint;
            _credentialsFactory = credentialsFactory;
            _authConfiguration = authConfiguration;
            _httpClientFactory = httpClientFactory;
            _logger = logger ?? NullLogger.Instance;
        }

        public static string GetAppId(ClaimsIdentity claimsIdentity)
        {
            var appId = JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims);

            if (string.IsNullOrEmpty(appId))
            {
                // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For
                // unauthenticated requests we have anonymous claimsIdentity provided auth is disabled.
                // For Activities coming from Emulator AppId claim contains the Bot's AAD AppId.
                appId = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)?.Value;
            }

            return appId;
        }

        public override string GetOriginatingAudience()
        {
            return _toChannelFromBotOAuthScope;
        }

        public override async Task<ClaimsIdentity> AuthenticateChannelRequestAsync(string authHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                var isAuthDisabled = await new DelegatingCredentialProvider(_credentialsFactory).IsAuthenticationDisabledAsync().ConfigureAwait(false);
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

            return await JwtTokenValidation
                .ValidateAuthHeader(authHeader, new DelegatingCredentialProvider(_credentialsFactory), GetChannelProvider(), "unknown", _authConfiguration)
                .ConfigureAwait(false);
        }

        public override async Task<AuthenticateRequestResult> AuthenticateRequestAsync(Activity activity, string authHeader, CancellationToken cancellationToken)
        {
            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authHeader, new DelegatingCredentialProvider(_credentialsFactory), GetChannelProvider(), _authConfiguration, _authHttpClient).ConfigureAwait(false);

            var outboundAudience = SkillValidation.IsSkillClaim(claimsIdentity.Claims) ? JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims) : _toChannelFromBotOAuthScope;

            var callerId = await GenerateCallerIdAsync(_credentialsFactory, claimsIdentity, _callerId, cancellationToken).ConfigureAwait(false);

            var connectorFactory = new ConnectorFactoryImpl(GetAppId(claimsIdentity), _toChannelFromBotOAuthScope, _loginEndpoint, true, _credentialsFactory, _httpClientFactory, _logger);

            return new AuthenticateRequestResult { ClaimsIdentity = claimsIdentity, Audience = outboundAudience, CallerId = callerId, ConnectorFactory = connectorFactory };
        }

        public override async Task<AuthenticateRequestResult> AuthenticateStreamingRequestAsync(string authHeader, string channelIdHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(channelIdHeader) && !await _credentialsFactory.IsAuthenticationDisabledAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new UnauthorizedAccessException();
            }

            var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, new DelegatingCredentialProvider(_credentialsFactory), GetChannelProvider(), channelIdHeader, httpClient: _authHttpClient).ConfigureAwait(false);

            var outboundAudience = SkillValidation.IsSkillClaim(claimsIdentity.Claims) ? JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims) : _toChannelFromBotOAuthScope;

            var callerId = await GenerateCallerIdAsync(_credentialsFactory, claimsIdentity, _callerId, cancellationToken).ConfigureAwait(false);

            return new AuthenticateRequestResult { ClaimsIdentity = claimsIdentity, Audience = outboundAudience, CallerId = callerId };
        }

        public override ConnectorFactory CreateConnectorFactory(ClaimsIdentity claimsIdentity)
        {
            return new ConnectorFactoryImpl(GetAppId(claimsIdentity), _toChannelFromBotOAuthScope, _loginEndpoint, true, _credentialsFactory, _httpClientFactory, _logger);
        }

        public override async Task<UserTokenClient> CreateUserTokenClientAsync(ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
        {
            var appId = GetAppId(claimsIdentity);

            var credentials = await _credentialsFactory.CreateCredentialsAsync(appId, _toChannelFromBotOAuthScope, _loginEndpoint, true, cancellationToken).ConfigureAwait(false);

            return new UserTokenClientImpl(appId, credentials, _oauthEndpoint, _httpClientFactory?.CreateClient(), _logger);
        }

        public override BotFrameworkClient CreateBotFrameworkClient()
        {
            return new BotFrameworkClientImpl(_credentialsFactory, _httpClientFactory, _loginEndpoint, _logger);
        }

        private IChannelProvider GetChannelProvider()
        {
            return _channelService != null ? new SimpleChannelProvider(_channelService) : null;
        }
    }
}
