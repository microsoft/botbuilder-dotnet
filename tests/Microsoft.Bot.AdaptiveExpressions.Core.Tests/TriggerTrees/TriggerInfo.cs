// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.AdaptiveExpressions.Core.TriggerTrees.Tests
{
    public class TriggerInfo
    {
        public Expression Trigger { get; set; }

        public Dictionary<string, object> Bindings { get; set; } = new Dictionary<string, object>();
    }
}
