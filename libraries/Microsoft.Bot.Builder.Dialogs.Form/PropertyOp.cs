// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    // Slot and operation
    public class PropertyOp
    {
        public string Property { get; set; }

        public string Operation { get; set; }

        public override string ToString()
            => $"{Operation}({Property})";
    }
}
