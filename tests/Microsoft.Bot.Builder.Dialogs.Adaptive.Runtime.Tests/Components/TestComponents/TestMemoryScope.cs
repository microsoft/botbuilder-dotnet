// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Tests.Components.TestComponents
{
    /// <summary>
    /// Simple test memory scope.
    /// </summary>
    public class TestMemoryScope : MemoryScope
    {
        private const string ScopePath = "test";

        public TestMemoryScope() 
            : base(ScopePath)
        {
        }

        public Dictionary<string, string> Data { get; private set; } = new Dictionary<string, string>() { { "somedata", "somevalue" } };

        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (!dc.Context.TurnState.TryGetValue(ScopePath, out var settings))
            {
                settings = Data;
                dc.Context.TurnState[ScopePath] = settings;
            }

            return settings;
        }

        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException("You cannot set the memory for a readonly memory scope");
        }
    }
}
