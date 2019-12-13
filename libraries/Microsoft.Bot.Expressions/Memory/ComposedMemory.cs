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
        public ComposedMemory(IMemory state, Dictionary<string, object> local)
        {
            if (state is ComposedMemory existingMemory)
            {
                var existingLocal = existingMemory.LocalMemory;
                foreach (var pair in local)
                {
                    existingLocal.SetValue(pair.Key, pair.Value);
                }

                LocalMemory = existingLocal;
                GlobalMemory = existingMemory.GlobalMemory;
            }
            else
            {
                LocalMemory = new SimpleObjectMemory(local);
                GlobalMemory = state;
            }
        }

        public SimpleObjectMemory LocalMemory { get; set; }

        public IMemory GlobalMemory { get; set; }

        public (object value, string error) GetValue(string path)
        {
            if (path.StartsWith(BuiltInFunctions.LocalVariablePrefix))
            {
                return LocalMemory.GetValue(path);
            }
            else
            {
                return GlobalMemory.GetValue(path);
            }
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
