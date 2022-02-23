// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class ParameterizedBotFrameworkAuthentication : BotFrameworkAuthentication
    {
        private static readonly HttpClient _authHttpClient = new HttpClient();

        private readonly bool _validateAuthority;
        private readonly string _toChannelFromBotLoginUrl;
        private readonly string _toChannelFromBotOAuthScope;
        private readonly string _toBotFromChannelTokenIssuer;
        private readonly string _oAuthUrl;
        private readonly string _toBotFromChannelOpenIdMetadataUrl;
        private readonly string _toBotFromEmulatorOpenIdMetadataUrl;
        private readonly string _callerId;
        private readonly ServiceClientCredentialsFactory _credentialsFactory;
        private readonly AuthenticationConfiguration _authConfiguration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public ParameterizedBotFrameworkAuthentication(
            bool validateAuthority,
            string toChannelFromBotLoginUrl,
            string toChannelFromBotOAuthScope,
            string toBotFromChannelTokenIssuer,
            string oAuthUrl,
            string toBotFromChannelOpenIdMetadataUrl,
            string toBotFromEmulatorOpenIdMetadataUrl,
            string callerId,
            ServiceClientCredentialsFactory credentialsFactory,
            AuthenticationConfiguration authConfiguration,
            IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            _validateAuthority = validateAuthority;
            _toChannelFromBotLoginUrl = toChannelFromBotLoginUrl;
            _toChannelFromBotOAuthScope = toChannelFromBotOAuthScope;
            _toBotFromChannelTokenIssuer = toBotFromChannelTokenIssuer;
            _oAuthUrl = oAuthUrl;
            _toBotFromChannelOpenIdMetadataUrl = toBotFromChannelOpenIdMetadataUrl;
            _toBotFromEmulatorOpenIdMetadataUrl = toBotFromEmulatorOpenIdMetadataUrl;
            _callerId = callerId;
            _credentialsFactory = credentialsFactory;
            _authConfiguration = authConfiguration;
            _httpClientFactory = httpClientFactory;
            _logger = logger ?? NullLogger.Instance;
        }

        public override string GetOriginatingAudience()
        {
            return _toChannelFromBotOAuthScope;
        }

        public override async Task<ClaimsIdentity> AuthenticateChannelRequestAsync(string authHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                var isAuthDisabled = await _credentialsFactory.IsAuthenticationDisabledAsync(cancellationToken).ConfigureAwait(false);
                if (!isAuthDisabled)
                {
                    // No auth header. Auth is required. Request is not authorized.
                    throw new UnauthorizedAccessException();
                }

                // In the scenario where auth is disabled, we still want to have the
                // IsAuthenticated flag set in the ClaimsIdentity.
                // To do this requires adding in an empty claim.
                // Since ChannelServiceHandler calls are always a skill callback call, we set the skill claim too.
                var anonymousSkillClaim = new Claim(AuthenticationConstants.AppIdClaim, AuthenticationConstants.AnonymousSkillAppId);
                return new ClaimsIdentity(new List<Claim> { anonymousSkillClaim }, AuthenticationConstants.AnonymousAuthType);
            }

            return await JwtTokenValidation_ValidateAuthHeaderAsync(authHeader, "unknown", null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<AuthenticateRequestResult> AuthenticateRequestAsync(Activity activity, string authHeader, CancellationToken cancellationToken)
        {
            var claimsIdentity = await JwtTokenValidation_AuthenticateRequestAsync(activity, authHeader, cancellationToken).ConfigureAwait(false);

            var outboundAudience = claimsIdentity.Claims.IsSkillClaim() ? claimsIdentity.Claims.GetAppIdFromClaims() : _toChannelFromBotOAuthScope;

            var callerId = await GenerateCallerIdAsync(_credentialsFactory, claimsIdentity, _callerId, cancellationToken).ConfigureAwait(false);

            var connectorFactory = new ConnectorFactoryImpl(GetAppId(claimsIdentity), _toChannelFromBotOAuthScope, _toChannelFromBotLoginUrl, _validateAuthority, _credentialsFactory, _httpClientFactory, _logger);

            return new AuthenticateRequestResult { ClaimsIdentity = claimsIdentity, Audience = outboundAudience, CallerId = callerId, ConnectorFactory = connectorFactory };
        }

        public override async Task<AuthenticateRequestResult> AuthenticateStreamingRequestAsync(string authHeader, string channelIdHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(channelIdHeader) && !await _credentialsFactory.IsAuthenticationDisabledAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new UnauthorizedAccessException();
            }

            var claimsIdentity = await JwtTokenValidation_ValidateAuthHeaderAsync(authHeader, channelIdHeader, null, cancellationToken).ConfigureAwait(false);

            var outboundAudience = claimsIdentity.Claims.IsSkillClaim() ? claimsIdentity.Claims.GetAppIdFromClaims() : _toChannelFromBotOAuthScope;

            var callerId = await GenerateCallerIdAsync(_credentialsFactory, claimsIdentity, _callerId, cancellationToken).ConfigureAwait(false);

            return new AuthenticateRequestResult { ClaimsIdentity = claimsIdentity, Audience = outboundAudience, CallerId = callerId };
        }

        public override ConnectorFactory CreateConnectorFactory(ClaimsIdentity claimsIdentity)
        {
            return new ConnectorFactoryImpl(GetAppId(claimsIdentity), _toChannelFromBotOAuthScope, _toChannelFromBotLoginUrl, _validateAuthority, _credentialsFactory, _httpClientFactory, _logger);
        }

        public override async Task<UserTokenClient> CreateUserTokenClientAsync(ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
        {
            var appId = GetAppId(claimsIdentity);

            var credentials = await _credentialsFactory.CreateCredentialsAsync(appId, _toChannelFromBotOAuthScope, _toChannelFromBotLoginUrl, _validateAuthority, cancellationToken).ConfigureAwait(false);

            return new UserTokenClientImpl(appId, credentials, _oAuthUrl, _httpClientFactory?.CreateClient(), _logger);
        }

        public override BotFrameworkClient CreateBotFrameworkClient()
        {
            return new BotFrameworkClientImpl(_credentialsFactory, _httpClientFactory, _toChannelFromBotLoginUrl, _logger);
        }

        private static string GetAppId(ClaimsIdentity claimsIdentity)
        {
            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For
            // unauthenticated requests we have anonymous claimsIdentity provided auth is disabled.
            // For Activities coming from Emulator AppId claim contains the Bot's AAD AppId.
            var botAppIdClaim =
                claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim) ??
                claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim);

            return botAppIdClaim?.Value;
        }

        private static bool IsValidTokenFormat(string authHeader)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                // No token, not valid.
                return false;
            }

            var parts = authHeader.Split(' ');
            if (parts.Length != 2)
            {
                // Tokens MUST have exactly 2 parts. If we don't have 2 parts, it's not a valid token
                return false;
            }

            // We now have an array that should be:
            // [0] = "Bearer"
            // [1] = "[Big Long String]"
            var authScheme = parts[0];
            if (!authScheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                // The scheme MUST be "Bearer"
                return false;
            }

            return true;
        }

        private static bool IsTokenFromSkill(string authHeader)
        {
            if (!IsValidTokenFormat(authHeader))
            {
                return false;
            }

            // We know is a valid token, split it and work with it:
            // [0] = "Bearer"
            // [1] = "[Big Long String]"
            var bearerToken = authHeader.Split(' ')[1];

            // Parse the Big Long String into an actual token.
            var token = new JwtSecurityToken(bearerToken);

            return token.Claims.IsSkillClaim();
        }

        private static TokenValidationParameters GetEmulatorTokenValidationParameters()
        {
#pragma warning disable CA5404 // Do not disable token validation checks
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    // TODO: presumably this table should also come from configuration
                    "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/", // Auth v3.1, 1.0 token
                    "https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0", // Auth v3.1, 2.0 token
                    "https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/", // Auth v3.2, 1.0 token
                    "https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0", // Auth v3.2, 2.0 token
                    "https://sts.windows.net/cab8a31a-1906-4287-a0d8-4eef66b95f6e/", // Auth for US Gov, 1.0 token
                    "https://login.microsoftonline.us/cab8a31a-1906-4287-a0d8-4eef66b95f6e/v2.0", // Auth for US Gov, 2.0 token
                },
                ValidateAudience = false, // Audience validation takes place manually in code.
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
            };
#pragma warning restore CA5404 // Do not disable token validation checks
        }

        private static bool IsTokenFromEmulator(string authHeader)
        {
            if (!IsValidTokenFormat(authHeader))
            {
                return false;
            }

            // We know is a valid token, split it and work with it:
            // [0] = "Bearer"
            // [1] = "[Big Long String]"
            var bearerToken = authHeader.Split(' ')[1];

            // Parse the Big Long String into an actual token.
            var token = new JwtSecurityToken(bearerToken);

            // Is there an Issuer?
            if (string.IsNullOrWhiteSpace(token.Issuer))
            {
                // No Issuer, means it's not from the Emulator.
                return false;
            }

            // Is the token issued by a source we consider to be the emulator?
            var emulatorTokenValidationParameters = GetEmulatorTokenValidationParameters();
            if (!emulatorTokenValidationParameters.ValidIssuers.Contains(token.Issuer))
            {
                // Not a Valid Issuer. This is NOT a Bot Framework Emulator Token.
                return false;
            }

            // The Token is from the Bot Framework Emulator. Success!
            return true;
        }

        // The following code is based on JwtTokenValidation.AuthenticateRequest
        private async Task<ClaimsIdentity> JwtTokenValidation_AuthenticateRequestAsync(Activity activity, string authHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                var isAuthDisabled = await _credentialsFactory.IsAuthenticationDisabledAsync(cancellationToken).ConfigureAwait(false);
                if (!isAuthDisabled)
                {
                    // No Auth Header. Auth is required. Request is not authorized.
                    throw new UnauthorizedAccessException();
                }

                // Check if the activity is for a skill call and is coming from the Emulator.
                if (activity.ChannelId == Channels.Emulator && activity.Recipient?.Role == RoleTypes.Skill)
                {
                    // Return an anonymous claim with an anonymous skill AppId
                    var anonymousSkillClaim = new Claim(AuthenticationConstants.AppIdClaim, AuthenticationConstants.AnonymousSkillAppId);
                    return new ClaimsIdentity(new List<Claim> { anonymousSkillClaim }, AuthenticationConstants.AnonymousAuthType);
                }

                // In the scenario where Auth is disabled, we still want to have the
                // IsAuthenticated flag set in the ClaimsIdentity. To do this requires
                // adding in an empty claim.
                return new ClaimsIdentity(new List<Claim>(), AuthenticationConstants.AnonymousAuthType);
            }

            // Validate the header and extract claims.
            var claimsIdentity = await JwtTokenValidation_ValidateAuthHeaderAsync(authHeader, activity.ChannelId, activity.ServiceUrl, cancellationToken).ConfigureAwait(false);
            return claimsIdentity;
        }

        private async Task<ClaimsIdentity> JwtTokenValidation_ValidateAuthHeaderAsync(string authHeader, string channelId, string serviceUrl, CancellationToken cancellationToken)
        {
            var identity = await JwtTokenValidation_AuthenticateTokenAsync(authHeader, channelId, serviceUrl, cancellationToken).ConfigureAwait(false);

            await JwtTokenValidation_ValidateClaimsAsync(identity.Claims).ConfigureAwait(false);

            return identity;
        }

        private async Task JwtTokenValidation_ValidateClaimsAsync(IEnumerable<Claim> claims)
        {
            if (_authConfiguration.ClaimsValidator != null)
            {
                // Call the validation method if defined (it should throw an exception if the validation fails)
                var claimsList = claims as IList<Claim> ?? claims.ToList();
                await _authConfiguration.ClaimsValidator.ValidateClaimsAsync(claimsList).ConfigureAwait(false);
            }
            else if (claims.IsSkillClaim())
            {
                throw new UnauthorizedAccessException("ClaimsValidator is required for validation of Skill Host calls.");
            }
        }

        private async Task<ClaimsIdentity> JwtTokenValidation_AuthenticateTokenAsync(string authHeader, string channelId, string serviceUrl, CancellationToken cancellationToken)
        {
            if (IsTokenFromSkill(authHeader))
            {
                return await AuthenticateSkillTokenAsync(authHeader, channelId, cancellationToken).ConfigureAwait(false);
            }

            if (IsTokenFromEmulator(authHeader))
            {
                return await AuthenticateEmulatorTokenAsync(authHeader, channelId, cancellationToken).ConfigureAwait(false);
            }

            return await AuthenticateChannelTokenAsync(authHeader, serviceUrl, channelId, cancellationToken).ConfigureAwait(false);
        }

        // The following code is based on SkillValidation.AuthenticateChannelToken
        private async Task<ClaimsIdentity> AuthenticateSkillTokenAsync(string authHeader, string channelId, CancellationToken cancellationToken)
        {
#pragma warning disable CA5404 // Do not disable token validation checks
            var skillTokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    // TODO: presumably this table should also come from configuration
                    "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/", // Auth v3.1, 1.0 token
                    "https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0", // Auth v3.1, 2.0 token
                    "https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/", // Auth v3.2, 1.0 token
                    "https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0", // Auth v3.2, 2.0 token
                    "https://sts.windows.net/cab8a31a-1906-4287-a0d8-4eef66b95f6e/", // Auth for US Gov, 1.0 token
                    "https://login.microsoftonline.us/cab8a31a-1906-4287-a0d8-4eef66b95f6e/v2.0" // Auth for US Gov, 2.0 token
                },
                ValidateAudience = false, // Audience validation takes place manually in code.
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true
            };
#pragma warning restore CA5404 // Do not disable token validation checks

            // Add allowed token issuers from configuration (if present)
            if (_authConfiguration.ValidTokenIssuers != null && _authConfiguration.ValidTokenIssuers.Any())
            {
                var validIssuers = skillTokenValidationParameters.ValidIssuers.ToList();
                validIssuers.AddRange(_authConfiguration.ValidTokenIssuers);
                skillTokenValidationParameters.ValidIssuers = validIssuers;
            }

            var tokenExtractor = new JwtTokenExtractor(
                _authHttpClient,
                skillTokenValidationParameters,
                _toBotFromEmulatorOpenIdMetadataUrl,
                AuthenticationConstants.AllowedSigningAlgorithms);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader, channelId, _authConfiguration.RequiredEndorsements.ToArray()).ConfigureAwait(false);

            await ValidateSkillIdentityAsync(identity, cancellationToken).ConfigureAwait(false);

            return identity;
        }

        private async Task ValidateSkillIdentityAsync(ClaimsIdentity identity, CancellationToken cancellationToken)
        {
            if (identity == null)
            {
                // No valid identity. Not Authorized.
                throw new UnauthorizedAccessException("Invalid Identity");
            }

            if (!identity.IsAuthenticated)
            {
                // The token is in some way invalid. Not Authorized.
                throw new UnauthorizedAccessException("Token Not Authenticated");
            }

            var versionClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.VersionClaim);
            if (versionClaim == null)
            {
                // No version claim
                throw new UnauthorizedAccessException($"'{AuthenticationConstants.VersionClaim}' claim is required on skill Tokens.");
            }

            // Look for the "aud" claim, but only if issued from the Bot Framework
            var audienceClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AudienceClaim)?.Value;
            if (string.IsNullOrWhiteSpace(audienceClaim))
            {
                // Claim is not present or doesn't have a value. Not Authorized.
                throw new UnauthorizedAccessException($"'{AuthenticationConstants.AudienceClaim}' claim is required on skill Tokens.");
            }

            if (!await _credentialsFactory.IsValidAppIdAsync(audienceClaim, cancellationToken).ConfigureAwait(false))
            {
                // The AppId is not valid. Not Authorized.
                throw new UnauthorizedAccessException("Invalid audience.");
            }

            var appId = identity.Claims.GetAppIdFromClaims();
            if (string.IsNullOrWhiteSpace(appId))
            {
                // Invalid appId
                throw new UnauthorizedAccessException("Invalid appId.");
            }
        }

        // The following code is based on EmulatorValidation.AuthenticateEmulatorToken
        private async Task<ClaimsIdentity> AuthenticateEmulatorTokenAsync(string authHeader, string channelId, CancellationToken cancellationToken)
        {
            var emulatorTokenValidationParameters = GetEmulatorTokenValidationParameters();

            // Add allowed token issuers from configuration (if present)
            if (_authConfiguration.ValidTokenIssuers != null && _authConfiguration.ValidTokenIssuers.Any())
            {
                var validIssuers = emulatorTokenValidationParameters.ValidIssuers.ToList();
                validIssuers.AddRange(_authConfiguration.ValidTokenIssuers);
                emulatorTokenValidationParameters.ValidIssuers = validIssuers;
            }

            var tokenExtractor = new JwtTokenExtractor(
                _authHttpClient,
                emulatorTokenValidationParameters,
                _toBotFromEmulatorOpenIdMetadataUrl,
                AuthenticationConstants.AllowedSigningAlgorithms);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader, channelId, _authConfiguration.RequiredEndorsements.ToArray()).ConfigureAwait(false);
            if (identity == null)
            {
                // No valid identity. Not Authorized.
                throw new UnauthorizedAccessException("Invalid Identity");
            }

            if (!identity.IsAuthenticated)
            {
                // The token is in some way invalid. Not Authorized.
                throw new UnauthorizedAccessException("Token Not Authenticated");
            }

            // Now check that the AppID in the claimset matches
            // what we're looking for. Note that in a multi-tenant bot, this value
            // comes from developer code that may be reaching out to a service, hence the
            // Async validation.
            var versionClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.VersionClaim);
            if (versionClaim == null)
            {
                throw new UnauthorizedAccessException("'ver' claim is required on Emulator Tokens.");
            }

            var tokenVersion = versionClaim.Value;
            string appId;

            // The Emulator, depending on Version, sends the AppId via either the
            // appid claim (Version 1) or the Authorized Party claim (Version 2).
            if (string.IsNullOrWhiteSpace(tokenVersion) || tokenVersion == "1.0")
            {
                // either no Version or a version of "1.0" means we should look for
                // the claim in the "appid" claim.
                var appIdClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AppIdClaim);
                if (appIdClaim == null)
                {
                    // No claim around AppID. Not Authorized.
                    throw new UnauthorizedAccessException("'appid' claim is required on Emulator Token version '1.0'.");
                }

                appId = appIdClaim.Value;
            }
            else if (tokenVersion == "2.0")
            {
                // Emulator, "2.0" puts the AppId in the "azp" claim.
                var appZClaim = identity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AuthorizedParty);
                if (appZClaim == null)
                {
                    // No claim around AppID. Not Authorized.
                    throw new UnauthorizedAccessException("'azp' claim is required on Emulator Token version '2.0'.");
                }

                appId = appZClaim.Value;
            }
            else
            {
                // Unknown Version. Not Authorized.
                throw new UnauthorizedAccessException($"Unknown Emulator Token version '{tokenVersion}'.");
            }

            if (!await _credentialsFactory.IsValidAppIdAsync(appId, cancellationToken).ConfigureAwait(false))
            {
                throw new UnauthorizedAccessException($"Invalid AppId passed on token: {appId}");
            }

            return identity;
        }

        // The following code is based on GovernmentChannelValidation.AuthenticateChannelToken
        private async Task<ClaimsIdentity> AuthenticateChannelTokenAsync(string authHeader, string serviceUrl, string channelId, CancellationToken cancellationToken)
        {
#pragma warning disable CA5404 // Do not disable token validation checks
            var channelTokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { _toBotFromChannelTokenIssuer },
                ValidateAudience = false, // Audience validation takes place in JwtTokenExtractor
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
            };
#pragma warning restore CA5404 // Do not disable token validation checks

            // Add allowed token issuers from configuration (if present)
            if (_authConfiguration.ValidTokenIssuers != null && _authConfiguration.ValidTokenIssuers.Any())
            {
                var validIssuers = channelTokenValidationParameters.ValidIssuers.ToList();
                validIssuers.AddRange(_authConfiguration.ValidTokenIssuers);
                channelTokenValidationParameters.ValidIssuers = validIssuers;
            }

            var tokenExtractor = new JwtTokenExtractor(
                _authHttpClient,
                channelTokenValidationParameters,
                _toBotFromChannelOpenIdMetadataUrl,
                AuthenticationConstants.AllowedSigningAlgorithms);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader, channelId, _authConfiguration.RequiredEndorsements.ToArray()).ConfigureAwait(false);

            await ValidateChannelIdentityAsync(identity, serviceUrl, cancellationToken).ConfigureAwait(false);

            return identity;
        }

        private async Task ValidateChannelIdentityAsync(ClaimsIdentity identity, string serviceUrl, CancellationToken cancellationToken)
        {
            if (identity == null)
            {
                // No valid identity. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            if (!identity.IsAuthenticated)
            {
                // The token is in some way invalid. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            // Now check that the AppID in the claimset matches
            // what we're looking for. Note that in a multi-tenant bot, this value
            // comes from developer code that may be reaching out to a service, hence the
            // Async validation.

            // Look for the "aud" claim, but only if issued from the Bot Framework
            var audienceClaim = identity.Claims.FirstOrDefault(
                c => c.Issuer == _toBotFromChannelTokenIssuer && c.Type == AuthenticationConstants.AudienceClaim);

            if (audienceClaim == null)
            {
                // The relevant audience Claim MUST be present. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            // The AppId from the claim in the token must match the AppId specified by the developer.
            // In this case, the token is destined for the app, so we find the app ID in the audience claim.
            var appIdFromClaim = audienceClaim.Value;
            if (string.IsNullOrWhiteSpace(appIdFromClaim))
            {
                // Claim is present, but doesn't have a value. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            if (!await _credentialsFactory.IsValidAppIdAsync(appIdFromClaim, cancellationToken).ConfigureAwait(false))
            {
                // The AppId is not valid. Not Authorized.
                throw new UnauthorizedAccessException($"Invalid AppId passed on token: {appIdFromClaim}");
            }

            if (serviceUrl != null)
            {
                var serviceUrlClaim = identity.Claims.FirstOrDefault(claim => claim.Type == AuthenticationConstants.ServiceUrlClaim)?.Value;
                if (string.IsNullOrWhiteSpace(serviceUrlClaim))
                {
                    // Claim must be present. Not Authorized.
                    throw new UnauthorizedAccessException();
                }

                if (!string.Equals(serviceUrlClaim, serviceUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // Claim must match. Not Authorized.
                    throw new UnauthorizedAccessException();
                }
            }
        }
    }
}
