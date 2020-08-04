// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    public class Breakpoint : Range
    {
        public bool Verified { get; set; }

        public string Message { get; set; }
    }
}
