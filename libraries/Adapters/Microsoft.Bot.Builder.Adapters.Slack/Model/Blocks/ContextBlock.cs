using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Context block https://api.slack.com/reference/block-kit/blocks#context
    /// </summary>
    public class ContextBlock : Block
    {
        public string Type => "context";

        public List<BlockElement> Elements { get; } = new List<BlockElement>();
    }
}
