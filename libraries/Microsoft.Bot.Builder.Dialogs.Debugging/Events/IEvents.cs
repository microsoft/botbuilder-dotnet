// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Events
{
    internal interface IEvents
    {
#pragma warning disable CA1819 // Properties should not return arrays (change it without breaking binary compat)
        ExceptionBreakpointFilter[] Filters
#pragma warning restore CA1819 // Properties should not return arrays
        {
            get;
        }

        bool this[string filter]
        {
            get;
            set;
        }

        void Reset(IEnumerable<string> filters);
    }
}
