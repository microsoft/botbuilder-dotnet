// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Expressions.Memory
{
    public class StackedMemory : MemoryBase
    {
        private readonly Stack<IMemory> memoryList;

        public StackedMemory(params IMemory[] memory)
        {
            memoryList = new Stack<IMemory>(memory);
        }

        public static StackedMemory Wrap(IMemory memory)
        {
            if (memory is StackedMemory sm)
            {
                return sm;
            }
            else
            {
                return new StackedMemory(memory);
            }
        }

        public void Push(IMemory memory)
        {
            memoryList.Push(memory);
        }

        public IMemory Pop()
        {
            return memoryList.Pop();
        }

        public override object GetValue(string path)
        {
            if (memoryList.Count == 0)
            {
                throw new Exception("Invalid memory status, memory stack is empty");
            }

            var it = memoryList.GetEnumerator();
            while (it.MoveNext())
            {
                var memory = it.Current;
                if (memory.TryGetValue(path, out var value) && value != null)
                {
                    return value;
                }
            }

            return null;
        }

        public override object SetValue(string path, object value)
        {
            throw new Exception($"Can't set value to {path}, stacked memory is read-only");
        }
    }
}
