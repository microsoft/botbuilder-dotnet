// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime;
using AdaptiveExpressions.Memory;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// A customized memory designed for LG evaluation, in which
    /// we want to make sure the global memory (the first memory passed in) can be
    /// accessible at any sub evaluation process. 
    /// </summary>
    internal class CustomizedMemory : IMemory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomizedMemory"/> class.
        /// </summary>
        /// <param name="scope">Scope.</param>
        public CustomizedMemory(object scope)
        {
            this.GlobalMemory = scope == null ? null : MemoryFactory.Create(scope);
            this.LocalMemory = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomizedMemory"/> class.
        /// </summary>
        /// <param name="globalMemory">Global memory.</param>
        /// <param name="localMemory">Local memory.</param>
        public CustomizedMemory(IMemory globalMemory, IMemory localMemory = null)
        {
            this.GlobalMemory = globalMemory;
            this.LocalMemory = localMemory;
        }

        /// <summary>
        /// Gets or sets global memory.
        /// </summary>
        /// <value>
        /// Global memory.
        /// </value>
        public IMemory GlobalMemory { get; set; }

        /// <summary>
        /// Gets or sets local memory.
        /// </summary>
        /// <value>
        /// Local memory.
        /// </value>
        public IMemory LocalMemory { get; set; }

        public void SetValue(string path, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Try to get the value from a given path. Firstly, get result from global memory,
        /// if global memory does not contain, get from local memory.
        /// </summary>
        /// <param name="path">Memory path.</param>
        /// <param name="value">Resolved value.</param>
        /// <returns>True if the memory contains an element with the specified key; otherwise, false.</returns>
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
