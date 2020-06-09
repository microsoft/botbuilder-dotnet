using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Request
{
    /// <summary>
    /// Represents a Slack Event Type object https://api.slack.com/events-api#receiving_events.
    /// </summary>
    public class EventType
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        public string Ts { get; set; }

        public string Item { get; set; }

        [JsonProperty(PropertyName = "event_ts")]
        public string EventTs { get; internal set; }

        public string Channel { get; internal set; }

        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelId { get; internal set; }

        [JsonProperty(PropertyName = "bot_id")]
        public string BotId { get; internal set; }

        [JsonProperty(PropertyName = "thread_ts")]
        public string ThreadTs { get; internal set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, JToken> Properties { get; set; }
    }
}
