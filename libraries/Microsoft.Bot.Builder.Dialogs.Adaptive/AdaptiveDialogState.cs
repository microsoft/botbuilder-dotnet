// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class AdaptiveDialogState
    {
        public AdaptiveDialogState()
        {
        }

        [JsonProperty(PropertyName = "options")]
        public dynamic Options { get; set; }

        [JsonProperty(PropertyName = "actions")]
        public List<ActionState> Actions { get; set; } = new List<ActionState>();

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }
    }
}
