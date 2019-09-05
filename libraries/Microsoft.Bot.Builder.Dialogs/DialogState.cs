// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogState
    {
        public DialogState()
            : this(null)
        {
        }

        public DialogState(List<DialogInstance> stack)
        {
            DialogStack = stack ?? new List<DialogInstance>();
        }

        [JsonProperty("dialogStack")]
        public List<DialogInstance> DialogStack { get; set; } = new List<DialogInstance>();
    }
}
