// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A class defining the parameters used in <see cref="SkillConversationIdFactoryBase.CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions,System.Threading.CancellationToken)"/>.
    /// </summary>
    public class SkillConversationIdFactoryOptions
    {
        /// <summary>
        /// Gets or sets the oauth audience scope, used during token retrieval (either https://api.botframework.com or bot app id).
        /// </summary>
        /// <value>
        /// The oauth audience scope, used during token retrieval (either https://api.botframework.com or bot app id if this is a skill calling another skill).
        /// </value>
        public string FromBotOAuthScope { get; set; }

        /// <summary>
        /// Gets or sets the id of the parent bot that is messaging the skill.
        /// </summary>
        /// <value>
        /// The id of the parent bot that is messaging the skill.
        /// </value>
        public string FromBotId { get; set; }

        /// <summary>
        /// Gets or sets the activity which will be sent to the skill.
        /// </summary>
        /// <value>
        /// The activity which will be sent to the skill.
        /// </value>
        public Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the skill to create the conversation Id for.
        /// </summary>
        /// <value>
        /// The skill to create the conversation Id for.
        /// </value>
        public BotFrameworkSkill BotFrameworkSkill { get; set; }
    }
}
