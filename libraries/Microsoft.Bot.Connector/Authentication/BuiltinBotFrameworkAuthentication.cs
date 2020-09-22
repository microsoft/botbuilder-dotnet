// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    internal abstract class BuiltinBotFrameworkAuthentication : BotFrameworkAuthentication
    {
        private readonly string _toChannelFromBotOAuthScope;
        private readonly string _loginEndpoint;
        private readonly string _callerId;
        private readonly string _channelService;
        private readonly ServiceClientCredentialsFactory _credentialFactory;
        private readonly AuthenticationConfiguration _authConfiguration;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        protected BuiltinBotFrameworkAuthentication(string toChannelFromBotOAuthScope, string loginEndpoint, string callerId, string channelService, ServiceClientCredentialsFactory credentialFactory, AuthenticationConfiguration authConfiguration, HttpClient httpClient, ILogger logger)
        {
            _toChannelFromBotOAuthScope = toChannelFromBotOAuthScope;
            _loginEndpoint = loginEndpoint;
            _callerId = callerId;
            _channelService = channelService;
            _credentialFactory = credentialFactory;
            _authConfiguration = authConfiguration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public static string GetAppId(ClaimsIdentity claimsIdentity)
        {
            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For
            // unauthenticated requests we have anonymous claimsIdentity provided auth is disabled.
            // For Activities coming from Emulator AppId claim contains the Bot's AAD AppId.
            var botAppIdClaim = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim);
            if (botAppIdClaim == null)
            {
                botAppIdClaim = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim);
            }

            return botAppIdClaim?.Value;
        }

        public override async Task<AuthenticateRequestResult> AuthenticateRequestAsync(Activity activity, string authHeader, CancellationToken cancellationToken)
        {
            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authHeader, new DelegatingCredentialProvider(_credentialFactory), GetChannelProvider(), _authConfiguration, _httpClient).ConfigureAwait(false);

            var scope = SkillValidation.IsSkillClaim(claimsIdentity.Claims) ? JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims) : _toChannelFromBotOAuthScope;
            
            var callerId = await GenerateCallerIdAsync(_credentialFactory, claimsIdentity, cancellationToken).ConfigureAwait(false);

            var appId = GetAppId(claimsIdentity);

            var credentials = await _credentialFactory.CreateCredentialsAsync(appId, scope, _loginEndpoint, true, cancellationToken).ConfigureAwait(false);

            return new AuthenticateRequestResult { ClaimsIdentity = claimsIdentity, Credentials = credentials, Scope = scope, CallerId = callerId };
        }

        public override Task<ServiceClientCredentials> GetProactiveCredentialsAsync(ClaimsIdentity claimsIdentity, string audience, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private IChannelProvider GetChannelProvider()
        {
            return _channelService != null ? new SimpleChannelProvider(_channelService) : null;
        }

        private async Task<string> GenerateCallerIdAsync(ServiceClientCredentialsFactory credentialFactory, ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
        {
            // Is the bot accepting all incoming messages?
            if (await credentialFactory.IsAuthenticationDisabledAsync(cancellationToken).ConfigureAwait(false))
            {
                // Return null so that the callerId is cleared.
                return null;
            }

            // Is the activity from another bot?
            if (SkillValidation.IsSkillClaim(claimsIdentity.Claims))
            {
                return $"{CallerIdConstants.BotToBotPrefix}{JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims)}";
            }

            return _callerId;
        }
    }
}
