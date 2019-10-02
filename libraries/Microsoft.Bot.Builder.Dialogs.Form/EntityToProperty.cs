// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    // Simple mapping
    public class EntityToProperty
    {
        public PropertyOp Change { get; set; }

        public EntityInfo Entity { get; set; }

        public override string ToString()
            => $"{Change} = {Entity}";
    }
}
