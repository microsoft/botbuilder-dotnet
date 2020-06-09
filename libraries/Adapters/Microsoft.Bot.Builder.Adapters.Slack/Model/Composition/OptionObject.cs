using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Composition
{
    /// <summary>
    /// Represents a Slack option object https://api.slack.com/reference/block-kit/composition-objects#option
    /// </summary>
    public class OptionObject
    {
        public TextObject Text { get; set; }

        public string Value { get; set; }

        public TextObject Description { get; set; }

        public string Url { get; set; }
    }
}
