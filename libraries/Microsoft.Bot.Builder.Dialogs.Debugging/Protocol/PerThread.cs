// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal abstract class PerThread
    {
        public ulong ThreadId { get; set; }
    }
}
