using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Events
{
    /// <summary>
    /// Represents a Slack Url Verification event https://api.slack.com/events/url_verification.
    /// </summary>
    public class UrlVerificationEvent
    {
        public string Type { get; set; }

        public string Challenge { get; set; }

        public string Token { get; set; }
    }
}
