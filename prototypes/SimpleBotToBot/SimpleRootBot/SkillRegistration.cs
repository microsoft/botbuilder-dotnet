// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace SimpleRootBot
{
    public class SkillRegistration
    {
        /// <summary>
        /// Gets or sets id of the skill.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets appId of the skill.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the /api/messages endpoint for the skill.
        /// </summary>
        public string ServiceUrl { get; set; }
    }
}
