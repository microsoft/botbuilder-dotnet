// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A bot was installed or removed from a channel.
    /// </summary>
    public interface IInstallationUpdateActivity : IActivity
    {
        /// <summary>
        /// Gets or Sets add|remove.
        /// </summary>
        /// <value>Action.</value>
        string Action { get; set; }
    }
}
