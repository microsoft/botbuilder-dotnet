// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal abstract class Message
    {
        public int Seq { get; set; }

        public string Type { get; set; }

        [JsonExtensionData]
        public JObject Rest { get; } = new JObject();
    }
}
