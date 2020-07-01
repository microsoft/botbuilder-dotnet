// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines the shape of the state object returned by calling DialogContext.State.ToJson().
    /// </summary>
    [Obsolete("This class is no longer used and is deprecated. Use DialogContext.State.GetMemorySnapshot() to get all visible memory scope objects.", error: false)]
    public class DialogContextVisibleState
    {
        [JsonProperty(PropertyName = "user")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IDictionary<string, object> User { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        [JsonProperty(PropertyName = "conversation")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IDictionary<string, object> Conversation { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        [JsonProperty(PropertyName = "dialog")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IDictionary<string, object> Dialog { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
