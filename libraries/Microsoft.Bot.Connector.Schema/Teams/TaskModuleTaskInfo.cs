// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Metadata for a Task Module.
    /// </summary>
    public partial class TaskModuleTaskInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleTaskInfo"/> class.
        /// </summary>
        public TaskModuleTaskInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleTaskInfo"/> class.
        /// </summary>
        /// <param name="title">Appears below the app name and to the right of the app icon.</param>
        /// <param name="height">This can be a number, representing the task module's height in pixels, or a string, one of: small, medium, large.</param>
        /// <param name="width">This can be a number, representing the task module's width in pixels, or a string, one of: small, medium, large.</param>
        /// <param name="url">The URL of what is loaded as an iframe inside the task module. One of url or card is required.</param>
        /// <param name="card">The JSON for the Adaptive card to appear in the task module.</param>
        /// <param name="fallbackUrl">If a client does not support the task module feature, this URL is opened in a browser tab.</param>
        /// <param name="completionBotId">Specifies a bot App ID to send the result of the user's interaction with the task module to.
        /// If specified, the bot will receive a task/submit invoke event with a JSON object in the event payload.</param>
        public TaskModuleTaskInfo(string title = default, object height = default, object width = default, string url = default, Attachment card = default, string fallbackUrl = default, string completionBotId = default(string))
        {
            Title = title;
            Height = height;
            Width = width;
            Url = url;
            Card = card;
            FallbackUrl = fallbackUrl;
            CompletionBotId = completionBotId;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the title that appears below the app name and to the right of the app
        /// icon.
        /// </summary>
        /// <value>The title.</value>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets this can be a number, representing the task module's
        /// height in pixels, or a string, one of: small, medium, large.
        /// </summary>
        /// <value>The task module's height.</value>
        [JsonPropertyName("height")]
        public object Height { get; set; }

        /// <summary>
        /// Gets or sets this can be a number, representing the task module's
        /// width in pixels, or a string, one of: small, medium, large.
        /// </summary>
        /// <value>The task module's width.</value>
        [JsonPropertyName("width")]
        public object Width { get; set; }

        /// <summary>
        /// Gets or sets the URL of what is loaded as an iframe inside the task
        /// module. One of url or card is required.
        /// </summary>
        /// <value>The URL of what is loaded as an iframe inside the task module.</value>
        [JsonPropertyName("url")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets the JSON for the Adaptive card to appear in the task
        /// module.
        /// </summary>
        /// <value>The JSON for the Adaptive card to appear in the task module.</value>
        [JsonPropertyName("card")]
        public Attachment Card { get; set; }

        /// <summary>
        /// Gets or sets if a client does not support the task module feature,
        /// this URL is opened in a browser tab.
        /// </summary>
        /// <value>The fallback URL to open in a browser tab if the client does not support the task module feature.</value>
        [JsonPropertyName("fallbackUrl")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string FallbackUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets Specifies a bot App ID to send the result of the user's
        /// interaction with the task module to. If specified, the bot will receive
        /// a task/submit invoke event with a JSON object in the event payload.
        /// </summary>
        /// <value>The completion bot ID.</value>
        [JsonPropertyName("completionBotId")]
        public string CompletionBotId { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
