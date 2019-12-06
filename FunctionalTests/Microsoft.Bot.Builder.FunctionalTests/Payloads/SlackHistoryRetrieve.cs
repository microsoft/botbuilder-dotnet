// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Adapters.Slack;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.FunctionalTests.Payloads
{
    public class SlackHistoryRetrieve
    {
        [JsonProperty(PropertyName = "ok")]
        public string Ok { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public List<SlackMessage> Messages { get; } = new List<SlackMessage>();
    }
}
