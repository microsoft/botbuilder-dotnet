// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Expressions.Memory
{
    /// <summary>
    /// Compose multiple IMemory into one IMemory, designed for 'foreach\select\..', scenarios
    /// where a global/local seperation is required.
    /// </summary>
    internal class ComposedMemory : IMemory
    {
        private Dictionary<string, IMemory> memoryMap;

        public ComposedMemory(Dictionary<string, IMemory> memoryMap)
        {
            this.memoryMap = memoryMap;
        }

        public (object value, string error) GetValue(string path)
        {
            var prefix = path.Split('.')[0];
            if (memoryMap.TryGetValue(prefix, out var scope))
            {
                return scope.GetValue(path.Substring(prefix.Length + 1)); // +1 to swallow the "."
            }

            return (null, $"path not exists at {path}");
        }

        public (object value, string error) SetValue(string path, object value)
        {
            throw new NotImplementedException();
        }

        public string Version()
        {
            // Read-only
            return "0";
        }
    }
}
