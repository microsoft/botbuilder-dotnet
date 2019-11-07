// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface IBreakpoints
    {
        bool IsBreakPoint(object item);

        object ItemFor(Protocol.Breakpoint breakpoint);

        IReadOnlyList<Protocol.Breakpoint> SetBreakpoints(Protocol.Source source, IReadOnlyList<Protocol.SourceBreakpoint> sourceBreakpoints);

        IReadOnlyList<Protocol.Breakpoint> SetBreakpoints(IReadOnlyList<Protocol.FunctionBreakpoint> functionBreakpoints);

        IReadOnlyList<Protocol.Breakpoint> ApplyUpdates();
    }
}
