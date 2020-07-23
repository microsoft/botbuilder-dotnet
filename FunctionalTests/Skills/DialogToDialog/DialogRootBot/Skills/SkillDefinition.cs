// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Skills
{
    /// <summary>
    /// Extends <see cref="BotFrameworkSkill"/> and provides methods to return the actions and the begin activity to start a skill.
    /// </summary>
    /// <remarks>
    /// This is just a temporary implementation, ideally, this should be replaced by logic that parses a manifest and creates
    /// what's needed. 
    /// </remarks>
    public class SkillDefinition : BotFrameworkSkill
    {
        public virtual IList<string> GetActions()
        {
            throw new NotImplementedException();
        }

        public virtual Activity CreateBeginActivity(string actionId)
        {
            throw new NotImplementedException();
        }
    }
}
