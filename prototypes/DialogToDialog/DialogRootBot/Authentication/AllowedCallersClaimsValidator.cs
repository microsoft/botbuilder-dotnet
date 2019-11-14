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
        public override Task<bool> ValidateClaimsAsync(List<Claim> claims)
        {
            var isValid = true;
            if (SkillValidation.IsSkillClaim(claims))
            {
                // Do allowed list check here
                var appId = JwtTokenValidation.GetAppIdFromClaims(claims);
                Console.WriteLine(appId);
            }

            return Task.FromResult(isValid);
        }
    }
}
