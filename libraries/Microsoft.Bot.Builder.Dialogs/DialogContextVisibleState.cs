using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines the shape of the state object returned by calling DialogContext.State.ToJson().
    /// </summary>
    [Obsolete("This class is no longer used and is deprecated. Use DialogContext.State.GetMemorySnapshot() to get all visible memory scope objects.", error: false)]
    public class DialogContextVisibleState
    {
        [JsonProperty(PropertyName = "user")]
        public IDictionary<string, object> User { get; set; }

        [JsonProperty(PropertyName = "conversation")]
        public IDictionary<string, object> Conversation { get; set; }

        [JsonProperty(PropertyName = "dialog")]
        public IDictionary<string, object> Dialog { get; set; }
    }
}
