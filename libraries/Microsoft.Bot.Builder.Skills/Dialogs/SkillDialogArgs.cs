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
        /// Gets or sets the <see cref="ActivityTypes"/> to send to the skill.
        /// </summary>
        /// <value>
        /// The <see cref="ActivityTypes"/> to send to the skill.
        /// </value>
        public string ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the name of the event or invoke activity to send to the skill (this value is ignored for other types of activities).
        /// </summary>
        /// <value>
        /// The name of the event or invoke activity to send to the skill (this value is ignored for other types of activities).
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the text property for the <see cref="ActivityTypes.Message"/> to send to the skill (ignored for other types of activities).
        /// </summary>
        /// <value>
        /// The text property for the <see cref="ActivityTypes.Message"/> to send to the skill (ignored for other types of activities).
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the value property for the activity to send to the skill.
        /// </summary>
        /// <value>
        /// The value property for the activity to send to the skill.
        /// </value>
        public object Value { get; set; }
    }
}
