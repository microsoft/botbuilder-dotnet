// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

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
        /// <param name="activityPreview">(Only when type is botMessagePreview) Message activity to preview</param>
        public MessagingExtensionResult(string attachmentLayout = default(string), string type = default(string), IList<MessagingExtensionAttachment> attachments = default(IList<MessagingExtensionAttachment>), MessagingExtensionSuggestedAction suggestedActions = default(MessagingExtensionSuggestedAction), string text = default(string), Activity activityPreview = default(Activity))
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
        [JsonProperty(PropertyName = "attachmentLayout")]
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets the type of the result. Possible values include:
        /// 'result', 'auth', 'config', 'message', 'botMessagePreview'.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets (Only when type is result) Attachments.
        /// </summary>
        [JsonProperty(PropertyName = "attachments")]
        public IList<MessagingExtensionAttachment> Attachments { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "suggestedActions")]
        public MessagingExtensionSuggestedAction SuggestedActions { get; set; }

        /// <summary>
        /// Gets or sets (Only when type is message) Text.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets (Only when type is botMessagePreview) Message activity
        /// to preview.
        /// </summary>
        [JsonProperty(PropertyName = "activityPreview")]
        public Activity ActivityPreview { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
