// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public enum AllowInterruptions
    {
        /// <summary>
        /// Always consult parent dialogs before taking the input.
        /// </summary>
        Always,

        /// <summary>
        /// Never consult parent dialogs.
        /// </summary>
        Never,

        /// <summary>
        /// Recognize the input first, only consult parent dilaogs when notRecognized.
        /// </summary>
        NotRecognized
    }
}
