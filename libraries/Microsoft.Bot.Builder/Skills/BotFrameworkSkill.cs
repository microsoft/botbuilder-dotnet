// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills
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
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets appId of the skill.
        /// </summary>
        /// <value>
        /// AppId of the skill.
        /// </value>
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets /api/messages endpoint for the skill.
        /// </summary>
        /// <value>
        /// /api/messages endpoint for the skill.
        /// </value>
        [JsonProperty("skillEndpoint")]
        public Uri SkillEndpoint { get; set; }
    }
}
