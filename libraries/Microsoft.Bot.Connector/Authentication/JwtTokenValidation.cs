// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Contains helper methods for authenticating incoming HTTP requests.
    /// </summary>
    public static class JwtTokenValidation
    {
        /// <summary>
        /// Gets the AppId from a claims list.
        /// </summary>
        /// <remarks>
        /// In v1 tokens the AppId is in the the <see cref="AuthenticationConstants.AppIdClaim"/> claim.
        /// In v2 tokens the AppId is in the azp <see cref="AuthenticationConstants.AuthorizedParty"/> claim.
        /// If the <see cref="AuthenticationConstants.VersionClaim"/> is not present, this method will attempt to
        /// obtain the attribute from the <see cref="AuthenticationConstants.AppIdClaim"/> or if present.
        /// </remarks>
        /// <param name="claims">A list of <see cref="Claim"/> instances.</param>
        /// <returns>The value of the appId claim if found (null if it can't find a suitable claim).</returns>
        public static string GetAppIdFromClaims(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var claimsList = claims as IList<Claim> ?? claims.ToList();
            string appId = null;

            // Depending on Version, the is either in the
            // appid claim (Version 1) or the Authorized Party claim (Version 2).
            var tokenVersion = claimsList.FirstOrDefault(claim => claim.Type == AuthenticationConstants.VersionClaim)?.Value;
            if (string.IsNullOrWhiteSpace(tokenVersion) || tokenVersion == "1.0")
            {
                // either no Version or a version of "1.0" means we should look for
                // the claim in the "appid" claim.
                var appIdClaim = claimsList.FirstOrDefault(c => c.Type == AuthenticationConstants.AppIdClaim);
                appId = appIdClaim?.Value;
            }
            else if (tokenVersion == "2.0")
            {
                // "2.0" puts the AppId in the "azp" claim.
                var appZClaim = claimsList.FirstOrDefault(c => c.Type == AuthenticationConstants.AuthorizedParty);
                appId = appZClaim?.Value;
            }

            return appId;
        }
    }
}
