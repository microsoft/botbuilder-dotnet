// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class Request : Message
    {
        public string Command { get; set; }

        public override string ToString() => Command;
    }
}
