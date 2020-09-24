// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    internal sealed class Row
    {
        public Row(Source source, SourceBreakpoint sourceBreakpoint)
        {
            Source = source;
            SourceBreakpoint = sourceBreakpoint;
        }

        public Row(FunctionBreakpoint functionBreakpoint)
        {
            FunctionBreakpoint = functionBreakpoint;
        }

        public Source Source { get; }

        public SourceBreakpoint SourceBreakpoint { get; }

        public FunctionBreakpoint FunctionBreakpoint { get; }

        public Breakpoint Breakpoint { get; } = new Breakpoint();

        public object Item { get; set; }
    }
}
