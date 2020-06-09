using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Input block https://api.slack.com/reference/block-kit/blocks#input
    /// </summary>
    public class InputBlock : Block
    {
        public string Type => "input";

        public TextObject Label { get; set; }

        public BlockElement Element { get; set; }

        public TextObject Hint { get; set; }

        public bool Optional { get; set; }
    }
}
