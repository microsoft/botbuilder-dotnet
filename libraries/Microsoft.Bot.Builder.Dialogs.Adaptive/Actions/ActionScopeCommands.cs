// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class ActionScopeCommands
    {
        /// <summary>
        /// Change execution order to the action by id.
        /// </summary>
        public const string GotoAction = "goto";

        /// <summary>
        /// Break out of the current loop.
        /// </summary>
        public const string BreakLoop = "break";

        /// <summary>
        /// Continue executing at the begining of the loop.
        /// </summary>
        public const string ContinueLoop = "continue";
    }
}
