// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    public enum DialogReason
    {
        /// <summary>
        /// A dialog is being started through a call to `DialogContext.BeginAsync()`.
        /// </summary>
        BeginCalled,

        /// <summary>
        /// A dialog is being continued through a call to `DialogContext.ContinueAsync()`.
        /// </summary>
        ContinueCalled,

        /// <summary>
        /// A dialog ended normally through a call to `DialogContext.EndAsync()`.
        /// </summary>
        EndCalled,

        /// <summary>
        /// A dialog is ending because its being replaced through a call to `DialogContext.ReplaceAsync()`.
        /// </summary>
        ReplaceCalled,

        /// <summary>
        /// A dialog was cancelled as part of a call to `DialogContext.CancelAllAsync()`.
        /// </summary>
        CancelCalled,

        /// <summary>
        /// A step was advanced through a call to `WaterfallStepContext.NextAsync()`.
        /// </summary>
        NextCalled,
    }
}
