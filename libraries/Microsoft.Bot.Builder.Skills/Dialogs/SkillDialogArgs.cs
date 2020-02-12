// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A class with dialog arguments for a <see cref="SkillDialog"/>.
    /// </summary>
    public class SkillDialogArgs
    {
        /// <summary>
        /// Gets or sets the <see cref="BotFrameworkSkill"/> that the dialog will call.
        /// </summary>
        /// <value>
        /// The <see cref="BotFrameworkSkill"/> that the dialog will call.
        /// </value>
        public BotFrameworkSkill Skill { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Activity"/> to send to the skill.
        /// </summary>
        /// <value>
        /// The <see cref="Activity"/> to send to the skill.
        /// </value>
        public Activity Activity { get; set; }
    }
}
