using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Divider block https://api.slack.com/reference/block-kit/blocks#divider
    /// </summary>
    public class DividerBlock : Block
    {
        public string Type => "divider";
    }
}
