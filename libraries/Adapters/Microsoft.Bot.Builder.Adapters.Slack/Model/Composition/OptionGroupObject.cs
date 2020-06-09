using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Composition
{
    /// <summary>
    /// Represents a Slack Option Group object https://api.slack.com/reference/block-kit/composition-objects#option_group
    /// </summary>
    public class OptionGroupObject
    {
        public TextObject Label { get; set; }

        public List<OptionObject> Options { get; set; }

        public bool ExcludeExternalSharedChannels { get; set; }
    }
}
