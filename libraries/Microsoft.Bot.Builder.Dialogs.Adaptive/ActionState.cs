// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    [DebuggerDisplay("{DialogId}")]
    public class ActionState : DialogState
    {
        public ActionState()
        {
        }

        public ActionState(string dialogId = null, object options = null)
        {
            DialogId = dialogId;
            Options = options;
        }

        [JsonProperty(PropertyName = "dialogId")]
        public string DialogId { get; set; }

        [JsonProperty(PropertyName = "options")]
        public object Options { get; set; }
    }
}
