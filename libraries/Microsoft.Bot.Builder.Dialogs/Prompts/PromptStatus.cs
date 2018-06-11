// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Predefined recognition result status strings.
    /// </summary>
    /// <seealso cref="PromptResult"/>
    /// <seealso cref="BasePromptInternal{T}"/>
    public class PromptStatus
    {
        /// <summary>
        /// The input was not recognized.
        /// </summary>
        public const string NotRecognized = "NotRecognized";

        /// <summary>
        /// The input was recognized and validated.
        /// </summary>
        public const string Recognized = "Recognized";

        /// <summary>
        /// Validation failed because the recognized value is too small.
        /// </summary>
        public const string TooSmall = "TooSmall";

        /// <summary>
        /// Validation failed because the recognized value is too large.
        /// </summary>
        public const string TooBig = "TooBig";

        /// <summary>
        /// Validation failed because the recognized value is out of range.
        /// </summary>
        public const string OutOfRange = "OutOfRange";
    }
}