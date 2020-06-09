// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlackAPI;

namespace Microsoft.Bot.Builder.Adapters.Slack
{

    public class BlockActionsPayload
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "trigger_id")]
        public string TriggerId { get; set; }

        [JsonProperty(PropertyName = "response_url")]
        public Uri ResponseUrl { get; set; }

        public SlackUser User { get; set; }

        public MessageEvent Message { get; set; }

        // TODO: ADD VIEW

        public List<SlackAction> Actions { get; } = new List<SlackAction>();

        [JsonProperty(PropertyName = "actions.block_id")]
        public string ActionsBlockId { get; set; }

        [JsonProperty(PropertyName = "actions.action_id")]
        public string ActionsActionId { get; set; }

        [JsonProperty(PropertyName = "actions.value")]
        public string ActionsValue { get; set; }

        public string Token { get; set; }

        public SlackChannel Channel { get; set; }

        [JsonProperty(PropertyName = "thread_ts")]
        public string ThreadTs { get; set; }

        public JObject Team { get; } = new JObject();

        [JsonProperty(PropertyName = "action_ts")]
        public string ActionTs { get; set; }

        public JObject Submission { get; } = new JObject();

        [JsonProperty(PropertyName = "callback_id")]
        public string CallbackId { get; set; }

        public string State { get; set; }
    }
}
