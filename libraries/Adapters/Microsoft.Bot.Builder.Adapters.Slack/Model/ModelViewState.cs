// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class ModelViewState
    {
        [JsonProperty(PropertyName = "values")]
        public Dictionary<string, Dictionary<string, ModalBlock>> Values { get; } = new Dictionary<string, Dictionary<string, ModalBlock>>();
    }
}
