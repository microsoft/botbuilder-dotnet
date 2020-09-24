// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
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
