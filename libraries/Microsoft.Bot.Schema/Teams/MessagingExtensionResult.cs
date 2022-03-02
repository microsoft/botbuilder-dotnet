// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension result.
    /// </summary>
    public class MessagingExtensionResult
    {
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
            Attachments = attachments ?? new List<MessagingExtensionAttachment>();
            SuggestedActions = suggestedActions;
            Text = text;
            ActivityPreview = activityPreview;
        }

        /// <summary>
        /// Gets or sets hint for how to deal with multiple attachments.
        /// Possible values include: 'list', 'grid'.
        /// </summary>
        /// <value>The hint for how to deal with multiple attachments.</value>
        [JsonProperty(PropertyName = "attachmentLayout")]
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets the type of the result. Possible values include:
        /// 'result', 'auth', 'config', 'message', 'botMessagePreview'.
        /// </summary>
        /// <value>The type of the result.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets (Only when type is result) Attachments.
        /// </summary>
        /// <value>The attachments.</value>
        [JsonProperty(PropertyName = "attachments")]
        public IList<MessagingExtensionAttachment> Attachments { get; private set; } = new List<MessagingExtensionAttachment>();

        /// <summary>
        /// Gets or sets the suggested actions.
        /// </summary>
        /// <value>The suggested actions.</value>
        [JsonProperty(PropertyName = "suggestedActions")]
        public MessagingExtensionSuggestedAction SuggestedActions { get; set; }

        /// <summary>
        /// Gets or sets (Only when type is message) Text.
        /// </summary>
        /// <value>The message text.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets (Only when type is botMessagePreview) Message activity
        /// to preview.
        /// </summary>
        /// <value>The message activity to preview.</value>
        [JsonProperty(PropertyName = "activityPreview")]
        public Activity ActivityPreview { get; set; }
    }
}
