// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.BotKit.Adapters.Slack
{
    /// <summary>
    /// Data related to the dialog.
    /// </summary>
    public class DialogData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogData"/> class.
        /// </summary>
        /// <param name="title">title for the Dialog.</param>
        /// <param name="callback">callback id for the Dialog.</param>
        /// <param name="submit">submit label for the Dialog.</param>
        /// <param name="elements">List of elements for the Dialog.</param>
        public DialogData(string title, string callback, string submit, List<DialogElement> elements)
        {
            this.Title = title;
            this.CallbackId = callback;
            this.SubmitLabel = submit;
            this.Elements = elements;
        }

        /// <summary>
        /// Gets or Sets the tittle of the dialog.
        /// </summary>
        /// <value>The tittle of the dialog.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the callback Id of the dialog.
        /// </summary>
        /// <value>The callback id of the dialog.</value>
        public string CallbackId { get; set; }

        /// <summary>
        /// Gets or Sets the submit label of the dialog.
        /// </summary>
        /// <value>The submit label of the dialog.</value>
        public string SubmitLabel { get; set; }

        /// <summary>
        /// Gets or Sets a list of elements contained by the dialog.
        /// </summary>
        /// <value>The elements of the dialog.</value>
        public List<DialogElement> Elements { get; set; }

        /// <summary>
        /// Gets or Sets the state of the dialog.
        /// </summary>
        /// <value>The state of the dialog.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog should notify on cancel or not.
        /// </summary>
        /// <value>The notifyOnCancel indicator of the dialog.</value>
        public bool NotifyOnCancel { get; set; }
    }
}
