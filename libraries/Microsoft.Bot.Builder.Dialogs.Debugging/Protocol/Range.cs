// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class Range : Reference
    {
        public string Item { get; set; }

        public string More { get; set; }

        public JToken Designer { get; set; }

        public Source Source { get; set; }

        public int? Line { get; set; }

        public int? Column { get; set; }

        public int? EndLine { get; set; }

        public int? EndColumn { get; set; }
    }
}
