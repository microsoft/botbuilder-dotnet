using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack File block https://api.slack.com/reference/block-kit/blocks#file
    /// </summary>
    public class FileBlock : Block
    {
        public string Type => "file";

        [JsonProperty(PropertyName = "external_id")]
        public string ExternalId { get; set; }

        public string Source { get; set; }
    }
}
