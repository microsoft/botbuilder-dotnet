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
    internal class CustomizedMemory : IMemory
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

        public void SetValue(string path, object value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string path, out object value)
        {
            value = null;
            if (this.LocalMemory != null)
            {
                if (this.LocalMemory.TryGetValue(path, out var result))
                {
                    value = result;
                    return true;
                }
            }

            if (this.GlobalMemory != null)
            {
                this.GlobalMemory.TryGetValue(path, out var result);
                value = result;
            }

            return true;
        }

        public string Version()
        {
            return "0";
        }
    }
}
