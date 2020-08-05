// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class StackTrace : PerThread
    {
        public int? StartFrame { get; set; }

        public int? Levels { get; set; }
    }
}
