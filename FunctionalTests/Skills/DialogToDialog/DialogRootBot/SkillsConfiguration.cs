// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Skills;
using Microsoft.BotBuilderSamples.DialogRootBot.Skills;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.DialogRootBot
{
    /// <summary>
    /// A helper class that loads Skills information from configuration.
    /// </summary>
    public class SkillsConfiguration
    {
        public SkillsConfiguration(IConfiguration configuration)
        {
            var section = configuration?.GetSection("BotFrameworkSkills");
            var skills = section?.Get<BotFrameworkSkill[]>();
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    Skills.Add(skill.Id, CreateSkillDefinition(skill));
                }
            }

            var skillHostEndpoint = configuration?.GetValue<string>(nameof(SkillHostEndpoint));
            if (!string.IsNullOrWhiteSpace(skillHostEndpoint))
            {
                SkillHostEndpoint = new Uri(skillHostEndpoint);
            }
        }

        public Uri SkillHostEndpoint { get; }

        public Dictionary<string, SkillDefinition> Skills { get; } = new Dictionary<string, SkillDefinition>();

        private static SkillDefinition CreateSkillDefinition(BotFrameworkSkill skill)
        {
            // Note: we hard code this for now, we should dynamically create instances based on the manifests.
            // For now, this code creates a strong typed version of the SkillDefinition and copies the info from
            // settings into it. 
            SkillDefinition skillDefinition;
            switch (skill.Id)
            {
                case "EchoSkillBot":
                    skillDefinition = ObjectPath.Assign<EchoSkill>(new EchoSkill(), skill);
                    break;
                case "DialogSkillBot":
                    skillDefinition = ObjectPath.Assign<DialogSkill>(new DialogSkill(), skill);
                    break;
                case "TeamsSkillBot":
                    skillDefinition = ObjectPath.Assign<TeamsSkill>(new TeamsSkill(), skill);
                    break;
                default:
                    throw new Exception($"Unable to find definition class for {skill.Id}.");
            }

            return skillDefinition;
        }
    }
}
