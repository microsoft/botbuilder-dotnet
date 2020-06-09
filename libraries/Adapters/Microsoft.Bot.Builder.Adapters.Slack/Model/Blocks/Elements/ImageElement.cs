using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Image element https://api.slack.com/reference/block-kit/block-elements#image
    /// </summary>
    public class ImageElement : BlockElement
    {
        public string Type => "image";

        [JsonProperty(PropertyName = "image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "alt_text")]
        public string AltText { get; set; }
    }
}
