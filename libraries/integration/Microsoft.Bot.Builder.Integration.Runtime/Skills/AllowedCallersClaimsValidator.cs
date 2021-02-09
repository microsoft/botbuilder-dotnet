// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Runtime.Integration.Skills
{
    public class AllowedCallersClaimsValidator : ClaimsValidator
    {
        private readonly IList<string> _allowedCallers;

        public AllowedCallersClaimsValidator(IList<string> allowedCallers)
        {
            _allowedCallers = allowedCallers ?? new List<string>();
        }

        /// <summary>
        /// Validate a list of claims and throw an exception if it fails.
        /// </summary>
        /// <param name="claims">The list of claims to validate.</param>
        /// <returns>True if the validation is successful, false if not.</returns>
        public override Task ValidateClaimsAsync(IList<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            // If _allowedCallers contains an "*", allow all callers.
            if (SkillValidation.IsSkillClaim(claims) &&
                !_allowedCallers.Contains("*"))
            {
                // Check that the appId claim in the skill request is in the list of callers configured for this bot.
                var applicationId = JwtTokenValidation.GetAppIdFromClaims(claims);
                if (!_allowedCallers.Contains(applicationId))
                {
                    throw new UnauthorizedAccessException(
                        $"Received a request from a bot with an app ID of \"{applicationId}\". To enable requests from this caller, add the app ID to your \"${ConfigurationConstants.RuntimeSettingsKey}:skill:allowedCallers\" configuration.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
