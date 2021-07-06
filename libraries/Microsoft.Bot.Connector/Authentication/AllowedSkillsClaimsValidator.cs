// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A claims validator that loads an allowed list from a provided list of allowed AppIds
    /// and checks that responses are coming from configured skills.
    /// </summary>
    public class AllowedSkillsClaimsValidator : ClaimsValidator
    {
        private readonly IList<string> _allowedSkills;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedSkillsClaimsValidator"/> class.
        /// </summary>
        /// <param name="allowedSkillAppIds">List of allowed callers referenced by appId.</param>
        public AllowedSkillsClaimsValidator(IList<string> allowedSkillAppIds)
        {
            _allowedSkills = allowedSkillAppIds ?? new List<string>();
        }

        /// <inheritdoc/>
        public override Task ValidateClaimsAsync(IList<Claim> claims)
        {
            if (SkillValidation.IsSkillClaim(claims))
            {
                // Check that the appId claim in the skill request is in the list of skills configured for this bot.
                var appId = JwtTokenValidation.GetAppIdFromClaims(claims);
                if (!_allowedSkills.Contains(appId))
                {
                    throw new UnauthorizedAccessException($"Received a request from an application with an appID of \"{appId}\". To enable requests from this skill, add the skill to your configuration file.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
