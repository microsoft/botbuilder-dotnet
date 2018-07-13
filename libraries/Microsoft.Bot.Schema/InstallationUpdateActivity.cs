// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A bot was installed or removed from a channel.
    /// </summary>
    public class InstallationUpdateActivity : Activity
    {
        public InstallationUpdateActivity() : base(ActivityTypes.InstallationUpdate)
        {
        }

        /// <summary>
        /// Gets or sets the added/removed action
        /// </summary>
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }
    }
}
