// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class Initialize
    {
        public string ClientID { get; set; }

        public string ClientName { get; set; }

        public string AdapterID { get; set; }

        public string PathFormat { get; set; }

        public bool LinesStartAt1 { get; set; }

        public bool ColumnsStartAt1 { get; set; }

        public bool SupportsVariableType { get; set; }

        public bool SupportsVariablePaging { get; set; }

        public bool SupportsRunInTerminalRequest { get; set; }

        public string Locale { get; set; }
    }
}
