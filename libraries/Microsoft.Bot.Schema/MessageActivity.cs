// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A message in a conversation
    /// </summary>
    public class MessageActivity : ActivityWithValue
    {
        public MessageActivity() : base(ActivityTypes.Message)
        {
        }

        protected MessageActivity(string type) : base(type)
        {
        }

        /// <summary>
        /// Gets or sets the language code of the Text field
        /// </summary>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets content for the message
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets SSML Speak for TTS audio response
        /// </summary>
        [JsonProperty(PropertyName = "speak")]
        public string Speak { get; set; }

        /// <summary>
        /// Gets or sets input hint to the channel on what the bot is
        /// expecting. Possible values include: 'acceptingInput',
        /// 'ignoringInput', 'expectingInput'
        /// </summary>
        [JsonProperty(PropertyName = "inputHint")]
        public string InputHint { get; set; }

        /// <summary>
        /// Gets or sets text to display if the channel cannot render cards
        /// </summary>
        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets format of text fields Default:markdown. Possible
        /// values include: 'markdown', 'plain', 'xml'
        /// </summary>
        [JsonProperty(PropertyName = "textFormat")]
        public string TextFormat { get; set; }

        /// <summary>
        /// Gets or sets hint for how to deal with multiple attachments.
        /// Default:list. Possible values include: 'list', 'carousel'
        /// </summary>
        [JsonProperty(PropertyName = "attachmentLayout")]
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets attachments
        /// </summary>
        [JsonProperty(PropertyName = "attachments")]
        public IList<Attachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets suggestedActions are used to provide
        /// keyboard/quickreply like behavior in many clients
        /// </summary>
        [JsonProperty(PropertyName = "suggestedActions")]
        public SuggestedActions SuggestedActions { get; set; }
    }

    public static class MessageActivityExtensions
    {
        /// <summary>
        /// Checks whether this message activity has content.
        /// </summary>
        /// <returns>True, if this message activity has any content to send; otherwise, false.</returns>
        /// <remarks>This method is defined on the <see cref="Activity"/> class, but is only intended
        /// for use on an activity of <see cref="Activity.Type"/> <see cref="ActivityTypes.Message"/>.</remarks>
        public static bool HasContent(this MessageActivity messageActivity)
        {
            if (!string.IsNullOrWhiteSpace(messageActivity.Text))
                return true;

            if (!string.IsNullOrWhiteSpace(messageActivity.Summary))
                return true;

            if (messageActivity.Attachments?.Any() == true)
                return true;

            if (messageActivity.ChannelData != null)
                return true;

            return false;
        }

        /// <summary>
        /// Resolves the mentions from the entities of this (message) activity.
        /// </summary>
        /// <returns>The array of mentions; or an empty array, if none are found.</returns>
        public static IEnumerable<Mention> GetMentions(this MessageActivity messageActivity)
        {
            if(messageActivity.Entities == null)
            {
                return Enumerable.Empty<Mention>();
            }

            return messageActivity.Entities
                .Where(e => e.Type.Equals("mention", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Properties.ToObject<Mention>());
        }
    }
}
