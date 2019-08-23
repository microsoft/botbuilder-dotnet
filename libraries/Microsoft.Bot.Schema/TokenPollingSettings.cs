using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Helps provide polling for token details.
    /// </summary>
    public class TokenPollingSettings
    {
        /// <summary>
        /// Gets or sets polling timeout time in milliseconds. This is equivalent to login flow timeout.
        /// </summary>
        [JsonProperty("timeout")]
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets time Interval in milliseconds between token polling requests.
        /// </summary>
        [JsonProperty("interval")]
        public int Interval { get; set; }
    }
}
