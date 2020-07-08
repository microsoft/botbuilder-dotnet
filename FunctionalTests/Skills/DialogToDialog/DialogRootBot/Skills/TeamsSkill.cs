// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Skills
{
    public class TeamsSkill : SkillDefinition
    {
        private const string SkillActionTeamsTaskModule = "TeamsTaskModule";
        private const string SkillActionTeamsCardAction = "TeamsCardAction";

        public override IList<string> GetActions()
        {
            return new List<string>
            {
                SkillActionTeamsTaskModule,
                SkillActionTeamsCardAction
            };
        }

        public override Activity CreateBeginActivity(string actionId)
        {
            Activity activity;

            if (actionId.Equals(SkillActionTeamsTaskModule, StringComparison.CurrentCultureIgnoreCase))
            {
                activity = (Activity)Activity.CreateInvokeActivity();
                activity.Name = SkillActionTeamsTaskModule;
                return activity;
            }

            if (actionId.Equals(SkillActionTeamsCardAction, StringComparison.CurrentCultureIgnoreCase))
            {
                activity = (Activity)Activity.CreateInvokeActivity();
                activity.Name = SkillActionTeamsCardAction;
                return activity;
            }

            throw new InvalidOperationException($"Unable to create begin activity for \"{actionId}\".");
        }
    }
}
