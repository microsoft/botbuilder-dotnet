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
    public class JwtTokenExtractor
    {
        /// <summary>
        /// The endorsements validator delegate.
        /// </summary>
        /// <param name="endorsements"> The endorsements used for validation.</param>
        /// <returns>true if validation passes; false otherwise.</returns>
        public delegate bool EndorsementsValidator(string[] endorsements);

        /// <summary>
        /// Cache for OpenIdConnect configuration managers (one per metadata URL)
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _openIdMetadataCache =
            new ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();

        /// <summary>
        /// Cache for Endorsement configuration managers (one per metadata URL)
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConfigurationManager<IDictionary<string, string[]>>> _endorsementsCache =
            new ConcurrentDictionary<string, ConfigurationManager<IDictionary<string, string[]>>>();

        /// <summary>
        /// Token validation parameters for this instance
        /// </summary>
        private readonly TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// OpenIdConnect configuration manager for this instance
        /// </summary>
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _openIdMetadata;

        /// <summary>
        /// Endorsements configuration manager for this instance
        /// </summary>
        private readonly ConfigurationManager<IDictionary<string, string[]>> _endorsementsData;

        /// <summary>
        /// Allowed signing algorithms
        /// </summary>
        private readonly string[] _allowedSigningAlgorithms;

        /// <summary>
        /// Delegate for validating endorsements extracted from JwtToken
        /// </summary>
        private readonly EndorsementsValidator _validator;

        public JwtTokenExtractor(TokenValidationParameters tokenValidationParameters, string metadataUrl, string[] allowedSigningAlgorithms, EndorsementsValidator validator)
        {
            // Make our own copy so we can edit it
            _tokenValidationParameters = tokenValidationParameters.Clone();
            _tokenValidationParameters.RequireSignedTokens = true;
            _allowedSigningAlgorithms = allowedSigningAlgorithms;
            _validator = validator;

            _openIdMetadata = _openIdMetadataCache.GetOrAdd(metadataUrl, key =>
            {
                return new ConfigurationManager<OpenIdConnectConfiguration>(metadataUrl, new OpenIdConnectConfigurationRetriever());
            });

            _endorsementsData = _endorsementsCache.GetOrAdd(metadataUrl, key =>
            {
                var retriever = new EndorsementsRetriever();
                return new ConfigurationManager<IDictionary<string, string[]>>(metadataUrl, retriever, retriever);
            });
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(HttpRequestMessage request)
        {
            if (request.Headers.Authorization != null)
                return await GetIdentityAsync(
                    request.Headers.Authorization.Scheme,
                    request.Headers.Authorization.Parameter).ConfigureAwait(false);

            return null;
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(string authorizationHeader)
        {
            if (authorizationHeader == null)
                return null;

            string[] parts = authorizationHeader?.Split(' ');
            if (parts.Length == 2)
                return await GetIdentityAsync(parts[0], parts[1]).ConfigureAwait(false);

            return null;
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(string scheme, string parameter)
        {
            // No header in correct scheme or no token
            if (scheme != "Bearer" || string.IsNullOrEmpty(parameter))
                return null;

            // Issuer isn't allowed? No need to check signature
            if (!HasAllowedIssuer(parameter))
                return null;

            try
            {
                ClaimsPrincipal claimsPrincipal = await ValidateTokenAsync(parameter).ConfigureAwait(false);
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
            JwtSecurityToken token = new JwtSecurityToken(jwtToken);
            if (_tokenValidationParameters.ValidIssuer != null && _tokenValidationParameters.ValidIssuer == token.Issuer)
                return true;

            if ((_tokenValidationParameters.ValidIssuers ?? Enumerable.Empty<string>()).Contains(token.Issuer))
                return true;

            return false;
        }

        //public string GetAppIdFromClaimsIdentity(ClaimsIdentity identity)
        //{
        //    if (identity == null)
        //        return null;

        //    Claim botClaim = identity.Claims.FirstOrDefault(c => _tokenValidationParameters.ValidIssuers.Contains(c.Issuer) && c.Type == "aud");
        //    return botClaim?.Value;
        //}

        //public string GetAppIdFromEmulatorClaimsIdentity(ClaimsIdentity identity)
        //{
        //    if (identity == null)
        //        return null;

        //    Claim versionClaim = identity.Claims.FirstOrDefault(c => c.Type == "ver");

        //    Claim appIdClaim = identity.Claims.FirstOrDefault(c => _tokenValidationParameters.ValidIssuers.Contains(c.Issuer) &&
        //        ((versionClaim != null && versionClaim.Value == "2.0" && c.Type == "azp") || c.Type == "appid"));
        //    if (appIdClaim == null)
        //        return null;

        //    // v3.1 or v3.2 emulator token
        //    if (identity.Claims.Any(c => c.Type == "aud" && c.Value == appIdClaim.Value))
        //        return appIdClaim.Value;

        //    // v3.0 emulator token -- allow this
        //    if (identity.Claims.Any(c => c.Type == "aud" && c.Value == "https://graph.microsoft.com"))
        //        return appIdClaim.Value;

        //    return null;
        //}


        /// <summary>
        /// From RFC 7517
        ///     https://tools.ietf.org/html/rfc7515#section-4.1.4
        /// The "kid" (key ID) Header Parameter is a hint indicating which key
        /// was used to secure the JWS. This parameter allows originators to
        /// explicitly signal a change of key to recipients. The structure of
        /// the "kid" value is unspecified. Its value MUST be a case-sensitive
        /// string. Use of this Header Parameter is OPTIONAL.
        /// When used with a JWK, the "kid" value is used to match a JWK "kid"
        /// parameter value.
        /// </summary>
        private const string KeyIdHeader = "kid";

        private async Task<ClaimsPrincipal> ValidateTokenAsync(string jwtToken)
        {
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
                    throw;
            }

            // Update the signing tokens from the last refresh
            _tokenValidationParameters.IssuerSigningKeys = config.SigningKeys;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(jwtToken, _tokenValidationParameters, out SecurityToken parsedToken);
                var parsedJwtToken = parsedToken as JwtSecurityToken;

                if (_validator != null)
                {
                    string keyId = (string)parsedJwtToken?.Header?[KeyIdHeader];
                    var endorsements = await _endorsementsData.GetConfigurationAsync();
                    if (!string.IsNullOrEmpty(keyId) && endorsements.ContainsKey(keyId))
                    {
                        if (!_validator(endorsements[keyId]))
                        {
                            throw new ArgumentException($"Could not validate endorsement for key: {keyId} with endorsements: {string.Join(",", endorsements[keyId])}");
                        }
                    }
                }

                if (_allowedSigningAlgorithms != null)
                {
                    string algorithm = parsedJwtToken?.Header?.Alg;
                    if (!_allowedSigningAlgorithms.Contains(algorithm))
                    {
                        throw new ArgumentException($"Token signing algorithm '{algorithm}' not in allowed list");
                    }
                }
                return principal;
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                string keys = string.Join(", ", ((config?.SigningKeys) ?? Enumerable.Empty<SecurityKey>()).Select(t => t.KeyId));
                Trace.TraceError("Error finding key for token. Available keys: " + keys);
                throw;
            }
        }
    }
}