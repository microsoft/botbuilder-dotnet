// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings
{
    /// <summary>
    /// Definition of a skill entry in the appsettings.json file.
    /// </summary>
    public class SkillConfigurationEntry
    {
        /// <summary>
        /// Gets or sets of sets the MSAppId for the entry.
        /// </summary>
        /// <value>
        /// The MSAppId for the entry.
        /// </value>
        public string MsAppId { get; set; }
    }
}
