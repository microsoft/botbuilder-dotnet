// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A message in a conversation
    /// </summary>
    public class MessageActivity : ActivityWithValue
    {
        private IList<Attachment> _attachments;

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
        public IList<Attachment> Attachments
        {
            get
            {
                // Use lazy instantiation to avoid overhead when not used
                if (_attachments == null)
                {
                    _attachments = new List<Attachment>();
                }

                return _attachments;
            }

            set
            {
                _attachments = value;
            }
        }

        /// <summary>
        /// Gets or sets suggestedActions are used to provide
        /// keyboard/quickreply like behavior in many clients
        /// </summary>
        [JsonProperty(PropertyName = "suggestedActions")]
        public SuggestedActions SuggestedActions { get; set; }

        /// <summary>
        /// Gets or sets dateTime to expire the activity as ISO 8601 encoded
        /// datetime
        /// </summary>
        [JsonProperty(PropertyName = "expiration")]
        public System.DateTimeOffset? Expiration { get; set; }
    }
}
