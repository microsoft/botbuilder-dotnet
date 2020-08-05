// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class Capabilities
    {
        public bool SupportsConfigurationDoneRequest { get; set; }

        public bool SupportsSetVariable { get; set; }

        public bool SupportsEvaluateForHovers { get; set; }

        public bool SupportsFunctionBreakpoints { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public ExceptionBreakpointFilter[] ExceptionBreakpointFilters { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public bool SupportTerminateDebuggee { get; set; }

        public bool SupportsTerminateRequest { get; set; }
    }
}
