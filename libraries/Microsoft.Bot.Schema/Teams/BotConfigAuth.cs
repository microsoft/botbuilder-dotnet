// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies bot config auth, including type and suggestedActions.
    /// </summary>
    public partial class BotConfigAuth
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotConfigAuth"/> class.
        /// </summary>
        public BotConfigAuth()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets type of bot config auth.
        /// </summary>
        /// <value>
        /// The type of bot config auth.
        /// </value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "auth";

        /// <summary>
        /// Gets or sets suggested actions. 
        /// </summary>
        /// <value>
        /// The suggested actions of bot config auth.
        /// </value>
        [JsonProperty(PropertyName = "suggestedActions")]
        public SuggestedActions SuggestedActions { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
