// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A JWT token processing class that gets identity information and performs security token validation.
    /// </summary>
    public class JwtTokenExtractor
    {
        /// <summary>
        /// Cache for OpenIdConnect configuration managers (one per metadata URL).
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _openIdMetadataCache =
            new ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();

        /// <summary>
        /// Cache for Endorsement configuration managers (one per metadata URL).
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConfigurationManager<IDictionary<string, HashSet<string>>>> _endorsementsCache =
            new ConcurrentDictionary<string, ConfigurationManager<IDictionary<string, HashSet<string>>>>();

        /// <summary>
        /// Token validation parameters for this instance.
        /// </summary>
        private readonly TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// OpenIdConnect configuration manager for this instance.
        /// </summary>
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _openIdMetadata;

        /// <summary>
        /// Endorsements configuration manager for this instance.
        /// </summary>
        private readonly ConfigurationManager<IDictionary<string, HashSet<string>>> _endorsementsData;

        /// <summary>
        /// Allowed signing algorithms.
        /// </summary>
        private readonly HashSet<string> _allowedSigningAlgorithms;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenExtractor"/> class.
        /// Extracts relevant data from JWT Tokens.
        /// </summary>
        /// <param name="httpClient">As part of validating JWT Tokens, endorsements need to be fetched from
        /// sources specified by the relevant security URLs. This HttpClient is used to allow for resource
        /// pooling around those retrievals. As those resources require TLS sharing the HttpClient is
        /// important to overall performance.</param>
        /// <param name="tokenValidationParameters">tokenValidationParameters.</param>
        /// <param name="metadataUrl">metadataUrl.</param>
        /// <param name="allowedSigningAlgorithms">allowedSigningAlgorithms.</param>
        public JwtTokenExtractor(
            HttpClient httpClient,
            TokenValidationParameters tokenValidationParameters,
            string metadataUrl,
            HashSet<string> allowedSigningAlgorithms)
        {
            // Make our own copy so we can edit it
            _tokenValidationParameters = tokenValidationParameters.Clone();
            _tokenValidationParameters.RequireSignedTokens = true;
            _allowedSigningAlgorithms = allowedSigningAlgorithms;

            _openIdMetadata = _openIdMetadataCache.GetOrAdd(metadataUrl, key =>
            {
                return new ConfigurationManager<OpenIdConnectConfiguration>(metadataUrl, new OpenIdConnectConfigurationRetriever(), httpClient);
            });

            _endorsementsData = _endorsementsCache.GetOrAdd(metadataUrl, key =>
            {
                var retriever = new EndorsementsRetriever(httpClient);
                return new ConfigurationManager<IDictionary<string, HashSet<string>>>(metadataUrl, retriever, retriever);
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenExtractor"/> class.
        /// Extracts relevant data from JWT Tokens.
        /// </summary>
        /// <param name="httpClient">As part of validating JWT Tokens, endorsements need to be fetched from
        /// sources specified by the relevant security URLs. This HttpClient is used to allow for resource
        /// pooling around those retrievals. As those resources require TLS sharing the HttpClient is
        /// important to overall performance.</param>
        /// <param name="tokenValidationParameters">tokenValidationParameters.</param>
        /// <param name="metadataUrl">metadataUrl.</param>
        /// <param name="allowedSigningAlgorithms">allowedSigningAlgorithms.</param>
        /// <param name="customEndorsementsConfig">Custom endorsement configuration to be used by the JwtTokenExtractor.</param>
        public JwtTokenExtractor(
            HttpClient httpClient,
            TokenValidationParameters tokenValidationParameters,
            string metadataUrl,
            HashSet<string> allowedSigningAlgorithms,
            ConfigurationManager<IDictionary<string, HashSet<string>>> customEndorsementsConfig)
        {
            // Make our own copy so we can edit it
            _tokenValidationParameters = tokenValidationParameters.Clone();
            _tokenValidationParameters.RequireSignedTokens = true;
            _allowedSigningAlgorithms = allowedSigningAlgorithms;

            _openIdMetadata = _openIdMetadataCache.GetOrAdd(metadataUrl, key =>
            {
                return new ConfigurationManager<OpenIdConnectConfiguration>(metadataUrl, new OpenIdConnectConfigurationRetriever(), httpClient);
            });

            _endorsementsData = customEndorsementsConfig ?? throw new ArgumentNullException(nameof(customEndorsementsConfig));
        }

        /// <summary>
        /// Gets the claims identity associated with a request.
        /// </summary>
        /// <param name="authorizationHeader">The raw HTTP header in the format: "Bearer [longString]".</param>
        /// <param name="channelId">The Id of the channel being validated in the original request.</param>
        /// <returns>A <see cref="Task{ClaimsIdentity}"/> object.</returns>
        public async Task<ClaimsIdentity> GetIdentityAsync(string authorizationHeader, string channelId)
        {
            return await GetIdentityAsync(authorizationHeader, channelId, Array.Empty<string>()).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the claims identity associated with a request.
        /// </summary>
        /// <param name="authorizationHeader">The raw HTTP header in the format: "Bearer [longString]".</param>
        /// <param name="channelId">The Id of the channel being validated in the original request.</param>
        /// <param name="requiredEndorsements">The required JWT endorsements.</param>
        /// <returns>A <see cref="Task{ClaimsIdentity}"/> object.</returns>
        public async Task<ClaimsIdentity> GetIdentityAsync(string authorizationHeader, string channelId, string[] requiredEndorsements)
        {
            if (authorizationHeader == null)
            {
                return null;
            }

            string[] parts = authorizationHeader?.Split(' ');
            if (parts.Length == 2)
            {
                return await GetIdentityAsync(parts[0], parts[1], channelId, requiredEndorsements).ConfigureAwait(false);
            }

            return null;
        }

        /// <summary>
        /// Gets the claims identity associated with a request.
        /// </summary>
        /// <param name="scheme">The associated scheme.</param>
        /// <param name="parameter">The token.</param>
        /// <param name="channelId">The Id of the channel being validated in the original request.</param>
        /// <returns>A <see cref="Task{ClaimsIdentity}"/> object.</returns>
        public async Task<ClaimsIdentity> GetIdentityAsync(string scheme, string parameter, string channelId)
        {
            return await GetIdentityAsync(scheme, parameter, channelId, Array.Empty<string>()).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the claims identity associated with a request.
        /// </summary>
        /// <param name="scheme">The associated scheme.</param>
        /// <param name="parameter">The token.</param>
        /// <param name="channelId">The Id of the channel being validated in the original request.</param>
        /// <param name="requiredEndorsements">The required JWT endorsements.</param>
        /// <returns>A <see cref="Task{ClaimsIdentity}"/> object.</returns>
        public async Task<ClaimsIdentity> GetIdentityAsync(string scheme, string parameter, string channelId, string[] requiredEndorsements)
        {
            if (requiredEndorsements == null)
            {
                throw new ArgumentNullException(nameof(requiredEndorsements));
            }

            // No header in correct scheme or no token
            if (scheme != "Bearer" || string.IsNullOrEmpty(parameter))
            {
                return null;
            }

            // Issuer isn't allowed? No need to check signature
            if (!HasAllowedIssuer(parameter))
            {
                return null;
            }

            try
            {
                var claimsPrincipal = await ValidateTokenAsync(parameter, channelId, requiredEndorsements).ConfigureAwait(false);
                return claimsPrincipal.Identities.OfType<ClaimsIdentity>().FirstOrDefault();
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Invalid token. " + e.ToString());
                throw;
            }
        }

        private bool HasAllowedIssuer(string jwtToken)
        {
            if (!_tokenValidationParameters.ValidateIssuer)
            {
                return true;
            }

            JwtSecurityToken token = new JwtSecurityToken(jwtToken);

            if (_tokenValidationParameters.ValidIssuer != null && _tokenValidationParameters.ValidIssuer == token.Issuer)
            {
                return true;
            }

            if ((_tokenValidationParameters.ValidIssuers ?? Enumerable.Empty<string>()).Contains(token.Issuer))
            {
                return true;
            }

            return false;
        }

        private async Task<ClaimsPrincipal> ValidateTokenAsync(string jwtToken, string channelId, string[] requiredEndorsements)
        {
            if (requiredEndorsements == null)
            {
                throw new ArgumentNullException(nameof(requiredEndorsements));
            }

            // _openIdMetadata only does a full refresh when the cache expires every 5 days
            OpenIdConnectConfiguration config = null;
            try
            {
                config = await _openIdMetadata.GetConfigurationAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error refreshing OpenId configuration: {e}");

                // No config? We can't continue
                if (config == null)
                {
                    throw;
                }
            }

            // Update the signing tokens from the last refresh
            _tokenValidationParameters.IssuerSigningKeys = config.SigningKeys;
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(jwtToken, _tokenValidationParameters, out SecurityToken parsedToken);
                var parsedJwtToken = parsedToken as JwtSecurityToken;

                // Validate Channel / Token Endorsements. For this, the channelID present on the Activity
                // needs to be matched by an endorsement.
                var keyId = (string)parsedJwtToken?.Header?[AuthenticationConstants.KeyIdHeader];
                var endorsements = await _endorsementsData.GetConfigurationAsync().ConfigureAwait(false);

                // Note: On the Emulator Code Path, the endorsements collection is empty so the validation code
                // below won't run. This is normal.
                if (!string.IsNullOrEmpty(keyId) && endorsements.TryGetValue(keyId, out var endorsementsForKey))
                {
                    // Verify that channelId is included in endorsements
                    var isEndorsed = EndorsementsValidator.Validate(channelId, endorsementsForKey);

                    if (!isEndorsed)
                    {
                        throw new UnauthorizedAccessException($"Could not validate endorsement for key: {keyId} with endorsements: {string.Join(",", endorsementsForKey)}");
                    }

                    // Verify that additional endorsements are satisfied. If no additional endorsements are expected, the requirement is satisfied as well
                    var additionalEndorsementsSatisfied = requiredEndorsements.All(
                            endorsement => EndorsementsValidator.Validate(endorsement, endorsementsForKey));

                    if (!additionalEndorsementsSatisfied)
                    {
                        throw new UnauthorizedAccessException($"Could not validate additional endorsement for key: {keyId} with endorsements: {string.Join(",", endorsementsForKey)}. Expected endorsements: {string.Join(",", requiredEndorsements)}");
                    }
                }

                if (_allowedSigningAlgorithms != null)
                {
                    var algorithm = parsedJwtToken?.Header?.Alg;
                    if (!_allowedSigningAlgorithms.Contains(algorithm))
                    {
                        throw new UnauthorizedAccessException($"Token signing algorithm '{algorithm}' not in allowed list");
                    }
                }

                return principal;
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                var keys = string.Join(", ", (config?.SigningKeys ?? Enumerable.Empty<SecurityKey>()).Select(t => t.KeyId));
                Trace.TraceError("Error finding key for token. Available keys: " + keys);
                throw;
            }
        }
    }
}
