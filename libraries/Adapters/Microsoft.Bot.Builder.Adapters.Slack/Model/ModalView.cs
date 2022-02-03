// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class ModalView
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "state")]
        public ModelViewState State { get; set; }

        [JsonProperty(PropertyName = "callback_id")]
        public string ModalIndentifier { get; set; }

        [JsonProperty(PropertyName = "private_metadata")]
        public string PrivateMetadata { get; set; }
    }
}
