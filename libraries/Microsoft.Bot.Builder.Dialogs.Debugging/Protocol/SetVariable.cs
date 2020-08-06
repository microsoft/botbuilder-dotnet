// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class SetVariable
    {
        public ulong VariablesReference { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
