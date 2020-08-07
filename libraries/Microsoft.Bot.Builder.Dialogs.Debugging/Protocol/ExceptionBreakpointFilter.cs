// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class ExceptionBreakpointFilter
    {
        public string Filter { get; set; }

        public string Label { get; set; }

        public bool Default { get; set; }
    }
}
