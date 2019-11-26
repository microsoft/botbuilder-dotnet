// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Registration for a BotFrameworkHttpProtocol based Skill endpoint.
    /// </summary>
    public class BotFrameworkSkill
    {
        /// <summary>
        /// Gets or sets Id of the skill.
        /// </summary>
        /// <value>
        /// Id of the skill.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets appId of the skill.
        /// </summary>
        /// <value>
        /// AppId of the skill.
        /// </value>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets /api/messages endpoint for the skill.
        /// </summary>
        /// <value>
        /// /api/messages endpoint for the skill.
        /// </value>
        public Uri SkillEndpoint { get; set; }
    }
}
