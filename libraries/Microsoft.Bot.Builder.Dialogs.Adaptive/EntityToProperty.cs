// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class EntityToProperty
    {
        [JsonIgnore]
        public PropertySchema Schema { get; set; }

        public string Property { get; set; }

        public string Operation { get; set; }

        public EntityInfo Entity { get; set; }

        public bool Expected { get; set; }

        public override string ToString()
            => (Expected ? "+" : string.Empty) + $"{Property} = {Operation}({Entity})";
    }
}
