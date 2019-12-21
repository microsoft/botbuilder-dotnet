// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Expressions.Memory
{
    public class StackedMemory : Stack<IMemory>, IMemory
    {
        public static StackedMemory Wrap(IMemory memory)
        {
            if (memory is StackedMemory sm)
            {
                return sm;
            }
            else
            {
                var stackedMemory = new StackedMemory();
                stackedMemory.Push(memory);
                return stackedMemory;
            }
        }

        public bool TryGetValue(string path, out object value)
        {
            value = null;
            if (this.Count == 0)
            {
                return true;
            }

            var it = this.GetEnumerator();
            while (it.MoveNext())
            {
                var memory = it.Current;

                if (memory.TryGetValue(path, out var result) && result != null)
                {
                    value = result;
                    return true;
                }
            }

            return true;
        }

        public void SetValue(string path, object value)
        {
            throw new Exception($"Can't set value to {path}, stacked memory is read-only");
        }

        public string Version()
        {
            return "0"; // Read-only
        }
    }
}
