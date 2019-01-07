using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Contract
{
    public class DialogsInfo
    {
        [JsonProperty(PropertyName = "initialNodeId")]
        public string InitialNodeId { get; set; }

        [JsonProperty(PropertyName = "Dialogs")]
        public List<DialogInfo> Dialogs { get; set; }
    }
}
