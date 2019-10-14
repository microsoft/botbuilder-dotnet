// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Condition of the input.
    /// </summary>
    public enum InputState
    {
        /// <summary>
        /// Input missing.
        /// </summary>
        Missing,

        /// <summary>
        /// Input not recognized.
        /// </summary>
        Unrecognized,

        /// <summary>
        /// Input not valid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Input valid.
        /// </summary>
        Valid
    }
}
