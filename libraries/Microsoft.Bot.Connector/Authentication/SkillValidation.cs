// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Validates JWT tokens sent to and from a Skill.
    /// </summary>
    public static class SkillValidation
    {
        /// <summary>
        /// Checks if the given list of claims represents a skill.
        /// </summary>
        /// <remarks>
        /// A skill claim should contain:
        ///     An <see cref="AuthenticationConstants.VersionClaim"/> claim.
        ///     An <see cref="AuthenticationConstants.AudienceClaim"/> claim.
        ///     An <see cref="AuthenticationConstants.AppIdClaim"/> claim (v1) or an a <see cref="AuthenticationConstants.AuthorizedParty"/> claim (v2).
        /// And the appId claim should be different than the audience claim.
        /// When a channel (webchat, teams, etc.) invokes a bot, the <see cref="AuthenticationConstants.AudienceClaim"/>
        /// is set to <see cref="AuthenticationConstants.ToBotFromChannelTokenIssuer"/> but when a bot calls another bot,
        /// the audience claim is set to the appId of the bot being invoked.
        /// The protocol supports v1 and v2 tokens:
        /// For v1 tokens, the  <see cref="AuthenticationConstants.AppIdClaim"/> is present and set to the app Id of the calling bot.
        /// For v2 tokens, the  <see cref="AuthenticationConstants.AuthorizedParty"/> is present and set to the app Id of the calling bot.
        /// </remarks>
        /// <param name="claims">A list of claims.</param>
        /// <returns>True if the list of claims is a skill claim, false if is not.</returns>
        public static bool IsSkillClaim(IEnumerable<Claim> claims)
        {
            var claimsList = claims.ToList();

            if (claimsList.Any(c => c.Value == AuthenticationConstants.AnonymousSkillAppId && c.Type == AuthenticationConstants.AppIdClaim))
            {
                return true;
            }

            var version = claimsList.FirstOrDefault(claim => claim.Type == AuthenticationConstants.VersionClaim);
            if (string.IsNullOrWhiteSpace(version?.Value))
            {
                // Must have a version claim.
                return false;
            }

            var audience = claimsList.FirstOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)?.Value;
            if (string.IsNullOrWhiteSpace(audience) || AuthenticationConstants.ToBotFromChannelTokenIssuer.Equals(audience, StringComparison.OrdinalIgnoreCase))
            {
                // The audience is https://api.botframework.com and not an appId.
                return false;
            }

            var appId = JwtTokenValidation.GetAppIdFromClaims(claimsList);
            if (string.IsNullOrWhiteSpace(appId))
            {
                return false;
            }

            // Skill claims must contain and app ID and the AppID must be different than the audience.
            return appId != audience;
        }
    }
}
