using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Request
{
    /// <summary>
    /// Represents an incoming request from the Event API https://api.slack.com/events-api#receiving_events.
    /// </summary>
    public class EventRequest
    {
        public string Token { get; set; }

        [JsonProperty(PropertyName = "team_id")]
        public string TeamId { get; set; }

        [JsonProperty(PropertyName = "api_app_id")]
        public string ApiAppId { get; set; }

        public EventType Event { get; set; }

        public string Type { get; set; }

        [JsonProperty(PropertyName = "authed_users")]
        public List<string> AuthedUsers { get; } = new List<string>();

        [JsonProperty(PropertyName = "event_id")]
        public string EventId { get; set; }

        [JsonProperty(PropertyName = "event_time")]
        public string EventTime { get; set; }
    }
}
