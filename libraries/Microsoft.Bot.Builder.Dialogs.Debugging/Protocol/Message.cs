// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal abstract class Message : HasRest
    {
        public int Seq { get; set; }

        public string Type { get; set; }
    }
}
