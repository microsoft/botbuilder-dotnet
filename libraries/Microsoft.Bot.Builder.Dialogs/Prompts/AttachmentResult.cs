// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents recognition result for the prompt.
    /// </summary>
    public class AttachmentResult : PromptResult
    {
        public AttachmentResult()
        {
            Attachments = new List<Attachment>();
        }

        /// <summary>
        /// Gets or sets the collection of attachments recognized.
        /// </summary>
        /// <value>
        /// The collection of attachments recognized.
        /// </value>
        public List<Attachment> Attachments
        {
            get { return GetProperty<List<Attachment>>(nameof(Attachments)); }
            set { this[nameof(Attachments)] = value; }
        }
    }
}
