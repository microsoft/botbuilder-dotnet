// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Defines feedback loop type. Depending on the type, the feedback window will have a different structure. 
    /// </summary>
    public static class FeedbackInfoTypes
    {
        /// <summary>
        /// The type value for default feedback window form.
        /// </summary>
        public const string Default = "default";

        /// <summary>
        /// The type value for custom feedback window, can be either an AdaptiveCard or website.
        /// </summary>
        public const string Custom = "custom";
    }
}
