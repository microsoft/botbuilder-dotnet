// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1601 // Partial elements should be documented

namespace Microsoft.Bot.AdaptiveExpressions.Core.TriggerTrees.Tests
{
    public partial class Generator
    {
        public class WeightedChoice<T>
        {
            public double Weight { get; set; } = 0.0;

            public T Choice { get; set; } = default(T);
        }
    }
}
