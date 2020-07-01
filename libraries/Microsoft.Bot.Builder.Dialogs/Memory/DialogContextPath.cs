// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class DialogContextPath
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Memory Path to dialogContext's active dialog.
        /// </summary>
        public const string ActiveDialog = "dialogcontext.activeDialog";

        /// <summary>
        /// Memory Path to dialogContext's parent dialog.
        /// </summary>
        public const string Parent = "dialogcontext.parent";

        /// <summary>
        /// Memory Path to dialogContext's stack.
        /// </summary>
        public const string Stack = "dialogContext.stack";
    }
}
