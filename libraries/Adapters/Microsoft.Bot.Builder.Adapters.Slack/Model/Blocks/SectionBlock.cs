using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Section block https://api.slack.com/reference/block-kit/blocks#section
    /// </summary>
    public class SectionBlock
    {
        public string Type => "section";

        public string Text { get; set; }

        public List<TextObject> Fields { get; set; }

        public BlockElement Accessory { get; set; }
    }
}
