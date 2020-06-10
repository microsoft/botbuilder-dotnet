// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    public class SlackUser
    {
        public string Id { get; set; }

        public string Username { get; set; }

        [JsonProperty(PropertyName = "team_id")]
        public string TeamId { get; set; }
    }
}
