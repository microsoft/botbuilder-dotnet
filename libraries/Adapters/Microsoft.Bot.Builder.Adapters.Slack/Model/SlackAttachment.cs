// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SlackAPI;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    public class SlackAttachment
    {
        [JsonProperty(PropertyName = "callback_id")]
        public string CallbackId { get; set; }

        public string Footer { get; set; }

        public List<AttachmentAction> Actions { get; } = new List<AttachmentAction>();

        [JsonProperty(PropertyName = "mrkdwn_in")]
        public List<string> MrkDwnIn { get; } = new List<string>();

        [JsonProperty(PropertyName = "thumb_url")]
        public Uri ThumbUrl { get; set; }

        [JsonProperty(PropertyName = "image_url")]
        public Uri ImageUrl { get; set; }

        public List<IBlock> Blocks { get; } = new List<IBlock>();

        public List<Field> Fields { get; } = new List<Field>();

        [JsonProperty(PropertyName = "footer_icon")]
        public string FooterIcon { get; set; }

        public string Text { get; set; }

        public string Title { get; set; }

        [JsonProperty(PropertyName = "author_icon")]
        public string AuthorIcon { get; set; }

        [JsonProperty(PropertyName = "author_link")]
        public string AuthorLink { get; set; }

        [JsonProperty(PropertyName = "author_name")]
        public string AuthorName { get; set; }

        public string Pretext { get; set; }

        public string Color { get; set; }

        public string Fallback { get; set; }

        [JsonProperty(PropertyName = "title_link")]
        public string TitleLink { get; set; }
    }
}
