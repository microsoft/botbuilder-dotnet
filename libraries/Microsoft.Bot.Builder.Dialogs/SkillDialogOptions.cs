// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Skills;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines the options that will be used to execute a <see cref="SkillDialog"/>.
    /// </summary>
    public class SkillDialogOptions
    {
        /// <summary>
        /// Gets or sets the Microsoft app ID of the bot calling the skill.
        /// </summary>
        /// <value>
        /// The the Microsoft app ID of the bot calling the skill.
        /// </value>
        public string BotId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BotFrameworkClient"/> used to call the remote skill.
        /// </summary>
        /// <value>
        /// The <see cref="BotFrameworkClient"/> used to call the remote skill.
        /// </value>
        public BotFrameworkClient SkillClient { get; set; }

        /// <summary>
        /// Gets or sets the callback Url for the skill host.
        /// </summary>
        /// <value>
        /// The callback Url for the skill host.
        /// </value>
        public Uri SkillHostEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BotFrameworkSkill"/> that the dialog will call.
        /// </summary>
        /// <value>
        /// The <see cref="BotFrameworkSkill"/> that the dialog will call.
        /// </value>
        public BotFrameworkSkill Skill { get; set; }

        /// <summary>
        /// Gets or sets an instance of a <see cref="SkillConversationIdFactoryBase"/> used to generate conversation IDs for interacting with the skill.
        /// </summary>
        /// <value>
        /// An instance of a <see cref="SkillConversationIdFactoryBase"/> used to generate conversation IDs for interacting with the skill.
        /// </value>
        public SkillConversationIdFactoryBase ConversationIdFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ConversationState"/> to be used by the dialog.
        /// </summary>
        /// <value>
        /// The <see cref="ConversationState"/> to be used by the dialog.
        /// </value>
        public ConversationState ConversationState { get; set; }

        /// <summary>
        /// Gets or sets the OAuth Connection Name, that would be used to perform Single SignOn with a skill.
        /// </summary>
        /// <value>
        /// The OAuth Connection Name for the Parent Bot.
        /// </value>
        public string ConnectionName { get; set; }
    }
}
