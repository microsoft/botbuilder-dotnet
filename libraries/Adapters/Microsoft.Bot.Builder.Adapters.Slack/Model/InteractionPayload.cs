﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlackAPI;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    /// <summary>
    /// Union class to represent various user interaction payloads https://api.slack.com/interactivity/handling#payloads.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class InteractionPayload
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "channel")]
        public Channel Channel { get; set; }

        [JsonProperty(PropertyName = "thread_ts")]
        public string ThreadTs { get; set; }

        [JsonProperty(PropertyName = "team")]
        public Team Team { get; set; }

        [JsonProperty(PropertyName = "message")]
        public MessageEvent Message { get; set; }

        [JsonProperty(PropertyName = "user")]
        public User User { get; set; }

        [JsonProperty(PropertyName = "actions")]
        public List<SlackAction> Actions { get; } = new List<SlackAction>();

        [JsonProperty(PropertyName = "trigger_id")]
        public string TriggerId { get; set; }

        [JsonProperty(PropertyName = "action_ts")]
        public string ActionTs { get; set; }

        [JsonProperty(PropertyName = "submission")]
        public JObject Submission { get; } = new JObject();

        [JsonProperty(PropertyName = "callback_id")]
        public string CallbackId { get; set; }

        [JsonProperty(PropertyName = "state")]
        public object State { get; set; }

        [JsonProperty(PropertyName = "response_url")]
        public Uri ResponseUrl { get; set; }

        [JsonProperty(PropertyName = "view")]
        public ModalView View { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, JToken> AdditionalProperties { get; } = new Dictionary<string, JToken>();
    }
}
