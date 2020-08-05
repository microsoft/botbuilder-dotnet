// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    internal interface IBreakpoints
    {
        bool IsBreakPoint(object item);

        object ItemFor(Breakpoint breakpoint);

        void Clear();

        IReadOnlyList<Breakpoint> SetBreakpoints(Source source, IReadOnlyList<SourceBreakpoint> sourceBreakpoints);

        IReadOnlyList<Breakpoint> SetBreakpoints(IReadOnlyList<FunctionBreakpoint> functionBreakpoints);

        IReadOnlyList<Breakpoint> ApplyUpdates();
    }
}
