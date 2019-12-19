// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Expressions.Memory;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// A customized memory designed for LG evaluation, in which
    /// we want to make sure the global memory (the first memory passed in) can be
    /// accessible at any sub evaluation process. 
    /// </summary>
    internal class CustomizedMemory : MemoryBase
    {
        public CustomizedMemory(object scope)
        {
            this.GlobalMemory = scope == null ? null : SimpleObjectMemory.Wrap(scope);
            this.LocalMemory = null;
        }

        public CustomizedMemory(IMemory globalMemory, IMemory localMemory = null)
        {
            this.GlobalMemory = globalMemory;
            this.LocalMemory = localMemory;
        }

        public IMemory GlobalMemory { get; set; }

        public IMemory LocalMemory { get; set; }

        public override bool ContainsPath(string path)
        {
            return (this.LocalMemory != null && this.LocalMemory.TryGetValue(path, out _))
                || (this.GlobalMemory != null && this.GlobalMemory.TryGetValue(path, out _));
        }

        public override object GetValue(string path)
        {
            if (this.LocalMemory != null)
            {
                if (this.LocalMemory.TryGetValue(path, out var value))
                {
                    return value;
                }
            }

            if (this.GlobalMemory != null)
            {
                return this.GlobalMemory.GetValue(path);
            }

            return null;
        }

        public override object SetValue(string path, object value)
        {
            throw new Exception("LG memory are readonly");
        }
    }
}
