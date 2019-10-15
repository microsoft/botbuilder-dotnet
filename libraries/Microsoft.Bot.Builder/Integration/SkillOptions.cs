// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Integration
{
    /// <summary>
    /// Registration for a BotFrameworkHttpProtocol based Skill endpoint.
    /// </summary>
    public class SkillOptions
    {
        /// <summary>
        /// Gets or sets name of the skill.
        /// </summary>
        /// <value>
        /// Name of the skill.
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
