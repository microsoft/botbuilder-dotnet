using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples
{
    public class AllowedSkillClaimsValidator : ClaimsValidator
    {
        private readonly BotFrameworkSkill _botFrameworkSkill;

        public AllowedSkillClaimsValidator(IConfiguration configuration)
        {
            // Load the appIds for the configured skills (we will only allow responses from skills we have configured).
            var section = configuration.GetSection("BotFrameworkSkill");
            _botFrameworkSkill = section?.Get<BotFrameworkSkill>();
        }

        public override Task ValidateClaimsAsync(IList<Claim> claims)
        {
            if (SkillValidation.IsSkillClaim(claims))
            {
                // Check that the appId claim in the skill request is in the list of skills configured for this bot.
                var appId = JwtTokenValidation.GetAppIdFromClaims(claims);
                if (appId != _botFrameworkSkill?.AppId)
                {
                    throw new UnauthorizedAccessException($"Received a request from an application with an appID of \"{appId}\". To enable requests from this skill, add the skill to your configuration file.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
