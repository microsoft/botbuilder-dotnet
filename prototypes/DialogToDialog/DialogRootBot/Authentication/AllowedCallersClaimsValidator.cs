// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace DialogRootBot.Authentication
{
    /// <summary>
    /// Sample claims validator that loads an allowed list from configuration if present.
    /// </summary>
    public class AllowedCallersClaimsValidator : ClaimsValidator
    {
        public override Task ValidateClaimsAsync(IList<Claim> claims)
        {
            if (SkillValidation.IsSkillClaim(claims))
            {
                var appId = JwtTokenValidation.GetAppIdFromClaims(claims);

                // TODO: check the app Id against the allowed list throw an UnauthorizedAccessException if it fails.
                Console.WriteLine(appId);
            }

            return Task.CompletedTask;
        }
    }
}
