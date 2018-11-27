// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// This enumeration is used by TrackTrace to identify severity level.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// Verbose severity level.
        /// </summary>
        Verbose = 0,

        /// <summary>
        /// Information severity level.
        /// </summary>
        Information = 1,

        /// <summary>
        /// Warning severity level.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Error severity level.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Critical severity level.
        /// </summary>
        Critical = 4,
    }
}
