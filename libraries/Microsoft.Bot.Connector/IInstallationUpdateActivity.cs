// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// A bot was installed or removed from a channel
    /// </summary>
    public interface IInstallationUpdateActivity : IActivity
    {
        /// <summary>
        /// add|remove
        /// </summary>
        string Action { get; set; }
    }
}
