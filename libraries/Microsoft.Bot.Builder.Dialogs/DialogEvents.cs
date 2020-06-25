// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class DialogEvents
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Event fired when a dialog beginDialog() is called.
        /// </summary>
        public const string BeginDialog = "beginDialog";

        /// <summary>
        /// Event fired when a dialog RepromptDialog is Called.
        /// </summary>
        public const string RepromptDialog = "repromptDialog";

        /// <summary>
        /// Event fired when a dialog is canceled.
        /// </summary>
        public const string CancelDialog = "cancelDialog";

        /// <summary>
        /// Event fired when an activity is received from the adapter (or a request to reprocess an activity).
        /// </summary>
        public const string ActivityReceived = "activityReceived";

        /// <summary>
        /// Event which is fired when the system has detected that deployed code has changed the execution of dialogs between turns.
        /// </summary>
        public const string VersionChanged = "versionChanged";

        /// <summary>
        /// Event fired when there was an exception thrown in the system.
        /// </summary>
        public const string Error = "error";
    }
}
