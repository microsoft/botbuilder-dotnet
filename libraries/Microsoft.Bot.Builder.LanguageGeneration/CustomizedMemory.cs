// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

        public (object value, string error) GetValue(string path)
        {
            object value = null;
            var error = string.Empty;

            if (this.LocalMemory != null)
            {
                (value, error) = this.LocalMemory.GetValue(path);
                if (error == null && value != null)
                {
                    return (value, error);
                }
            }

            if (this.GlobalMemory != null)
            {
                return this.GlobalMemory.GetValue(path);
            }

            return (value, error);
        }

        public (object value, string error) SetValue(string path, object value)
        {
            return (null, "LG memory are readonly");
        }

        public string Version()
        {
            // Read-only
            return "0";
        }
    }
}
