// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A class with dialog arguments for a <see cref="SkillDialog"/>.
    /// </summary>
    public class BeginSkillDialogOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="Activity"/> to send to the skill.
        /// </summary>
        /// <value>
        /// The <see cref="Activity"/> to send to the skill.
        /// </value>
        public Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the OAuth Connection Name, that would be used to perform Single SignOn with a skill.
        /// </summary>
        /// <value>
        /// The OAuth Connection Name for the Parent Bot.
        /// </value>
        public string ConnectionName { get; set; }
    }
}
