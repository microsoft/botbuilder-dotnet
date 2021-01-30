// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

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
        [JsonProperty("botId")]
        public string BotId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BotFrameworkClient"/> used to call the remote skill.
        /// </summary>
        /// <value>
        /// The <see cref="BotFrameworkClient"/> used to call the remote skill.
        /// </value>
        [JsonIgnore]
        public BotFrameworkClient SkillClient { get; set; }

        /// <summary>
        /// Gets or sets the callback Url for the skill host.
        /// </summary>
        /// <value>
        /// The callback Url for the skill host.
        /// </value>
        [JsonProperty("skillHostEndpoint")]
        public Uri SkillHostEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BotFrameworkSkill"/> that the dialog will call.
        /// </summary>
        /// <value>
        /// The <see cref="BotFrameworkSkill"/> that the dialog will call.
        /// </value>
        [JsonProperty("skill")]
        public BotFrameworkSkill Skill { get; set; }

        /// <summary>
        /// Gets or sets an instance of a <see cref="SkillConversationIdFactoryBase"/> used to generate conversation IDs for interacting with the skill.
        /// </summary>
        /// <value>
        /// An instance of a <see cref="SkillConversationIdFactoryBase"/> used to generate conversation IDs for interacting with the skill.
        /// </value>
        [JsonIgnore]
        public SkillConversationIdFactoryBase ConversationIdFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ConversationState"/> to be used by the dialog.
        /// </summary>
        /// <value>
        /// The <see cref="ConversationState"/> to be used by the dialog.
        /// </value>
        [JsonIgnore]
        public ConversationState ConversationState { get; set; }

        /// <summary>
        /// Gets or sets the OAuth Connection Name, that would be used to perform Single SignOn with a skill.
        /// </summary>
        /// <value>
        /// The OAuth Connection Name for the Parent Bot.
        /// </value>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the default delivery mode to use for communication with the skill.
        /// </summary>
        /// <remarks>
        /// This value will be used for activities that do not explicitly set deliveryMode.
        /// </remarks>
        /// <value>
        /// A deliveryMode constant [normal|expectReplies].
        /// </value>
        [JsonProperty("deliveryMode")]
        public string DeliveryMode { get; set; } = DeliveryModes.Normal;
    }
}
