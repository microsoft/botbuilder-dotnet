using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SlackAPI;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackAttachment
    {
        [JsonProperty(PropertyName = "callback_id")]
        public string CallbackId { get; set; }

        public string Footer { get; set; }

        public List<AttachmentAction> Actions { get; set; } = new List<AttachmentAction>();

        [JsonProperty(PropertyName = "mrkdwn_in")]
        public List<string> MrkDwnIn { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "thumb_url")]
        public string ThumbUrl { get; set; }

        [JsonProperty(PropertyName = "image_url")]
        public string ImageUrl { get; set; }

        public List<IBlock> Blocks { get; set; } = new List<IBlock>();

        public List<Field> Fields { get; set; } = new List<Field>();

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
