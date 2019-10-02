// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    // Select from multiple entities for singleton
    public class EntitiesToProperty
    {
        public List<EntityInfo> Entities { get; set; } = new List<EntityInfo>();

        public PropertyOp Property { get; set; }

        public override string ToString()
            => $"Singleton {Property} = [{Entities}]";
    }
}
