// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Dialogs
{
    /// <summary>
    /// Defines Dialog Options for QnAMakerDialog.
    /// </summary>
    public class QnAMakerDialogOptions
    {
        /// <summary>
        /// Gets or sets the options for the QnAMaker service.
        /// </summary>
        /// <value>
        /// The options for the QnAMaker service.
        /// </value>
        public QnAMakerOptions QnAMakerOptions { get; set; }

        /// <summary>
        /// Gets or sets the response options for the QnAMakerDialog.
        /// </summary>
        /// <value>
        /// The response options for the QnAMakerDialog.
        /// </value>
        public QnADialogResponseOptions ResponseOptions { get; set; }
    }
}
