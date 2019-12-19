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

        public (object value, string error) GetValue(string path)
        {
            if (this.Count == 0)
            {
                throw new Exception("Invalid memory status, memory stack is empty");
            }

            var it = this.GetEnumerator();
            while (it.MoveNext())
            {
                var memory = it.Current;
                (var value, var error) = memory.GetValue(path);
                if (error == null && value != null)
                {
                    return (value, error);
                }
            }

            return (null, null);
        }

        public (object value, string error) SetValue(string path, object value)
        {
            throw new Exception($"Can't set value to {path}, stacked memory is read-only");
        }

        public string Version()
        {
            return "0"; // Read-only
        }
    }
}
