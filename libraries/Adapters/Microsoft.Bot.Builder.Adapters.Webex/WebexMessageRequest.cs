// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// Represents the payload received when a Webex Message is sent to the bot.
    /// </summary>
    public class WebexMessageRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebexMessageRequest"/> class.
        /// Represents the request needed to create a message with attachments.
        /// </summary>
        public WebexMessageRequest()
        {
        }

        /// <summary>
        /// Gets or sets the room ID of the message.
        /// </summary>
        /// <value>
        /// The room ID of the message.
        /// </value>
        [JsonProperty(PropertyName = "roomId")]
        public string RoomId { get; set; }

        /// <summary>
        /// Gets or sets the person ID of the recipient when sending a private 1:1 message.
        /// </summary>
        /// <value>
        /// The person ID of the recipient when sending a private 1:1 message.
        /// </value>
        [JsonProperty(PropertyName = "toPersonId")]
        public string ToPersonId { get; set; }

        /// <summary>
        /// Gets or sets the email address of the recipient when sending a private 1:1 message.
        /// </summary>
        /// <value>
        /// The email address of the recipient when sending a private 1:1 message.
        /// </value>
        [JsonProperty(PropertyName = "toPersonEmail")]
        public string ToPersonEmail { get; set; }

        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        /// <value>
        /// The text of the message.
        /// </value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the message in Markdown format.
        /// </summary>
        /// <value>
        /// The message, in Markdown format.
        /// </value>
        [JsonProperty(PropertyName = "markdown")]
        public string Markdown { get; set; }

        /// <summary>
        /// Gets the URI to a binary file to be posted into the room. Only one file is allowed per message.
        /// </summary>
        /// <value>
        /// The URI to a binary file to be posted into the room.
        /// </value>
        [JsonProperty(PropertyName = "files")]
        public IList<Uri> Files { get; } = new List<Uri>();

        /// <summary>
        /// Gets or sets the content attachments to attach to the message.
        /// </summary>
        /// <value>
        /// The content attachments to attach to the message.
        /// </value>
        [JsonProperty(PropertyName = "attachments")]
        public object Attachments { get; set; }

        /// <summary>
        /// Checks if Files property should be serialized or not.
        /// </summary>
        /// <returns>True if there are files in the array to be serialized, false if there aren't.</returns>
        public bool ShouldSerializeFiles()
        {
            return Files.Count > 0;
        }
    }
}
