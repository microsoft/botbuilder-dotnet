// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Skills
{
    public class EchoSkill : SkillDefinition
    {
        private const string SkillActionMessage = "Message";

        public override IList<string> GetActions()
        {
            return new List<string> { SkillActionMessage };
        }

        public override Activity CreateBeginActivity(string actionId)
        {
            if (actionId.Equals(SkillActionMessage, StringComparison.CurrentCultureIgnoreCase))
            {
                var activity = (Activity)Activity.CreateMessageActivity();
                activity.Name = SkillActionMessage;
                activity.Text = "Begin the Echo Skill.";
                return activity;
            }

            throw new InvalidOperationException($"Unable to create begin activity for \"{actionId}\".");
        }
    }
}
