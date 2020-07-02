// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// Represents an Attachment Action - Users create attachment actions by interacting with
    /// message attachments such as clicking on a submit button in a card.
    /// https://developer.webex.com/docs/api/v1/attachment-actions.
    /// </summary>
    public class AttachmentActionData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the action.
        /// </summary>
        /// <value>
        /// The unique identifier for the action.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of action performed.
        /// </summary>
        /// <value>
        /// The type of action performed.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the parent message the attachment action was performed on.
        /// </summary>
        /// <value>
        /// The parent message the attachment action was performed on.
        /// </value>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person who performed the action.
        /// </summary>
        /// <value>
        /// The ID of the person who performed the action.
        /// </value>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the date and time the action was created.
        /// </summary>
        /// <value>
        /// The date and time the action was created.
        /// </value>
        public string Created { get; set; }

        /// <summary>
        /// Gets the action's inputs.
        /// </summary>
        /// <value>
        /// The action's inputs.
        /// </value>
        public Dictionary<string, string> Inputs { get; } = new Dictionary<string, string>();
    }
}
