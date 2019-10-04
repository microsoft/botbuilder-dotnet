// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Skill class for BotframeworkAdapter.
    /// </summary>
    public partial class BotFrameworkAdapter
    {
        /// <summary>
        /// Registration for a BotFrameworkHttpProtocol based Skill endpoint.
        /// </summary>
        public class Skill
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
            /// Gets or sets /api/messages endpoint for the skill.
            /// </summary>
            public string ServiceUrl { get; set; }
        }
    }
}
