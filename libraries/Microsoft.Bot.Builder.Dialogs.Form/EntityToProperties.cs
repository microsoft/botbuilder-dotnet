// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    // Select which slot entity belongs to
    public class EntityToProperties
    {
        public List<PropertyOp> Properties { get; set; } = new List<PropertyOp>();

        public EntityInfo Entity { get; set; }

        public override string ToString()
            => $"Slot {Entity} = [{Properties}]";
    }
}
