// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Runtime.Authentication
{
    public class AllowedCallersClaimsValidator : ClaimsValidator
    {
        public const string DefaultAllowedCallersKey = "skillConfiguration:allowedCallers";

        private readonly List<string> _allowedCallers = new List<string>();

        public AllowedCallersClaimsValidator(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // AllowedCallers is the setting in the appsettings.json file
            // that consists of the list of consumer bot application ids that are allowed to access the skill.
            var allowedCallersList = configuration.GetSection(DefaultAllowedCallersKey).Get<string[]>();
            if (allowedCallersList != null)
            {
                _allowedCallers = new List<string>(allowedCallersList);
            }
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
                        $"Received a request from a bot with an app ID of \"{applicationId}\". To enable requests from this caller, add the app ID to your ${DefaultAllowedCallersKey} configuration.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
