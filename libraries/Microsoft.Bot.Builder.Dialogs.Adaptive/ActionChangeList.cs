// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    [DebuggerDisplay("{ChangeType}:{Desire}")]
    public class ActionChangeList
    {
        [JsonProperty(PropertyName = "changeType")]
        public ActionChangeType ChangeType { get; set; } = ActionChangeType.InsertActions;

        [JsonProperty(PropertyName = "actions")]
        public List<ActionState> Actions { get; set; } = new List<ActionState>();

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets turn state associated with the plan change list (it will be applied to turn state when plan is applied).
        /// </summary>
        /// <value>
        /// Turn state associated with the plan change list (it will be applied to turn state when plan is applied).
        /// </value>
        [JsonProperty(PropertyName = "turn")]
        public Dictionary<string, object> Turn { get; set; }
    }
}
