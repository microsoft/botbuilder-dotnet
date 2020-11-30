// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Indicates whether the semantic action is starting, continuing, or done.
    /// </summary>
    public static class SemanticActionStates
    {
        /// <summary>
        /// Semantic action is starting.
        /// </summary>
        public const string Start = "start";

        /// <summary>
        /// Semantic action is continuing.
        /// </summary>
        public const string Continue = "continue";

        /// <summary>
        /// Semantic action is done.
        /// </summary>
        public const string Done = "done";
    }
}
