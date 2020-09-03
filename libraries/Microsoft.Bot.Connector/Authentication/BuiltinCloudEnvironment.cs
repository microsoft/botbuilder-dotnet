// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    internal abstract class BuiltinCloudEnvironment : ICloudEnvironment
    {
        private string _toChannelFromBotOAuthScope;
        private string _callerId;

        public BuiltinCloudEnvironment(string toChannelFromBotOAuthScope, string callerId)
        {
            _toChannelFromBotOAuthScope = toChannelFromBotOAuthScope;
            _callerId = callerId;
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

        public async Task<(ClaimsIdentity claimsIdentity, ServiceClientCredentials credentials, string scope, string callerId)> AuthenticateRequestAsync(Activity activity, string authHeader, ICredentialProvider credentialProvider, AuthenticationConfiguration authConfiguration, HttpClient httpClient, ILogger logger)
        {
            var channelProvider = GetChannelProvider();

            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authHeader, credentialProvider, channelProvider, authConfiguration, httpClient).ConfigureAwait(false);

            var scope = SkillValidation.IsSkillClaim(claimsIdentity.Claims) ? JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims) : _toChannelFromBotOAuthScope;
            
            var callerId = await GenerateCallerIdAsync(credentialProvider, claimsIdentity).ConfigureAwait(false);

            var appId = GetAppId(claimsIdentity);

            var credentials = await CreateAppCredentialsAsync(credentialProvider, appId, httpClient, logger, scope).ConfigureAwait(false);

            return (claimsIdentity, credentials, scope, callerId);
        }

        protected abstract IChannelProvider GetChannelProvider();

        protected abstract ServiceClientCredentials CreateServiceClientCredentials(string appId, string appPassword, HttpClient httpClient, ILogger logger, string scope);

        private async Task<string> GenerateCallerIdAsync(ICredentialProvider credentialProvider, ClaimsIdentity claimsIdentity)
        {
            // Is the bot accepting all incoming messages?
            if (await credentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false))
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

        private async Task<ServiceClientCredentials> CreateAppCredentialsAsync(ICredentialProvider credentialProvider, string appId, HttpClient httpClient, ILogger logger, string scope)
        {
            if (appId == null)
            {
                return MicrosoftAppCredentials.Empty;
            }
            else
            {
                // Get the password from the credential provider.
                var appPassword = await credentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);

                // Construct an AppCredentials using the app + password combination.
                return CreateServiceClientCredentials(appId, appPassword, httpClient, logger, scope);
            }
        }
    }
}
