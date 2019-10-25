// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Defines Dialog Options for QnAMakerDialog.
    /// </summary>
    public class QnAMakerDialogOptions
    {
        public QnAMakerOptions Options { get; set; }

        public QnADialogResponseOptions ResponseOptions { get; set; }
    }
}
