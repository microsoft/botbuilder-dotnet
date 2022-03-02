// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Messaging extension result.
    /// </summary>
    public partial class MessagingExtensionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionResult"/> class.
        /// </summary>
        public MessagingExtensionResult()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionResult"/> class.
        /// </summary>
        /// <param name="attachmentLayout">Hint for how to deal with multiple
        /// attachments. Possible values include: 'list', 'grid'.</param>
        /// <param name="type">The type of the result. Possible values include:
        /// 'result', 'auth', 'config', 'message', 'botMessagePreview'.</param>
        /// <param name="attachments">(Only when type is result)
        /// Attachments.</param>
        /// <param name="suggestedActions">The message extension suggested actions.</param>
        /// <param name="text">(Only when type is message) Text.</param>
        /// <param name="activityPreview">(Only when type is botMessagePreview) Message activity to preview.</param>
        public MessagingExtensionResult(string attachmentLayout = default, string type = default, IList<MessagingExtensionAttachment> attachments = default, MessagingExtensionSuggestedAction suggestedActions = default, string text = default, Activity activityPreview = default)
        {
            AttachmentLayout = attachmentLayout;
            Type = type;
            Attachments = attachments;
            SuggestedActions = suggestedActions;
            Text = text;
            ActivityPreview = activityPreview;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets hint for how to deal with multiple attachments.
        /// Possible values include: 'list', 'grid'.
        /// </summary>
        /// <value>The hint for how to deal with multiple attachments.</value>
        [JsonPropertyName("attachmentLayout")]
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets the type of the result. Possible values include:
        /// 'result', 'auth', 'config', 'message', 'botMessagePreview'.
        /// </summary>
        /// <value>The type of the result.</value>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets (Only when type is result) Attachments.
        /// </summary>
        /// <value>The attachments.</value>
        [JsonPropertyName("attachments")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<MessagingExtensionAttachment> Attachments { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the suggested actions.
        /// </summary>
        /// <value>The suggested actions.</value>
        [JsonPropertyName("suggestedActions")]
        public MessagingExtensionSuggestedAction SuggestedActions { get; set; }

        /// <summary>
        /// Gets or sets (Only when type is message) Text.
        /// </summary>
        /// <value>The message text.</value>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets (Only when type is botMessagePreview) Message activity
        /// to preview.
        /// </summary>
        /// <value>The message activity to preview.</value>
        [JsonPropertyName("activityPreview")]
        public Activity ActivityPreview { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
