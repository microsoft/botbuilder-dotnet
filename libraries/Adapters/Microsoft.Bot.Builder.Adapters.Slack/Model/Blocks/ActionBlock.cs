using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Action block https://api.slack.com/reference/block-kit/blocks#actions
    /// </summary>
    public class ActionBlock : Block
    {
        public string Type => "actions";

        public List<BlockElement> Elements { get; } = new List<BlockElement>();
    }
}
