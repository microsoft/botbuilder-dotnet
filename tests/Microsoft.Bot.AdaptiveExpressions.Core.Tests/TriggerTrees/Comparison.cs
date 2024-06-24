// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.AdaptiveExpressions.Core.TriggerTrees.Tests
{
    public class Comparison
    {
        public Comparison(string type, object value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }

        public object Value { get; set; }
    }
}
