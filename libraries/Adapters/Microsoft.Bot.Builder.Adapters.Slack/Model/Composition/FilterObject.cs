using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Composition
{
    /// <summary>
    /// Represents a Slack Filter object https://api.slack.com/reference/block-kit/composition-objects#filter_conversations
    /// </summary>
    public class FilterObject
    {
        public List<string> Include { get; set; }

        [JsonProperty(PropertyName = "exclude_external_shared_channels")]
        public bool ExcludeExternalSharedChannels { get; set; }

        [JsonProperty(PropertyName = "exclude_bot_users")]
        public bool ExcludeBotUsers { get; set; }
    }
}
