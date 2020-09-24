// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Defines the action state change list.
    /// </summary>
    [DebuggerDisplay("{ChangeType}:{Desire}")]
    public class ActionChangeList
    {
        /// <summary>
        /// Gets or sets the action change type value.
        /// </summary>
        /// <value>
        /// Action change type value.
        /// </value>
        [JsonProperty(PropertyName = "changeType")]
        public ActionChangeType ChangeType { get; set; } = ActionChangeType.InsertActions;

        /// <summary>
        /// Gets or sets the <see cref="ActionState"/> list.
        /// </summary>
        /// <value>
        /// <see cref="ActionState"/> list.
        /// </value>
        [JsonProperty(PropertyName = "actions")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<ActionState> Actions { get; set; } = new List<ActionState>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the tag list.
        /// </summary>
        /// <value>
        /// Tag list.
        /// </value>
        [JsonProperty(PropertyName = "tags")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<string> Tags { get; set; } = new List<string>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets turn state associated with the plan change list (it will be applied to turn state when plan is applied).
        /// </summary>
        /// <value>
        /// Turn state associated with the plan change list (it will be applied to turn state when plan is applied).
        /// </value>
        [JsonProperty(PropertyName = "turn")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public Dictionary<string, object> Turn { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
