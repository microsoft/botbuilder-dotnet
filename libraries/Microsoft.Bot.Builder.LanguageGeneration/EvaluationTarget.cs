// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    internal class EvaluationTarget
    {
        private readonly Dictionary<string, (string, object)> expCache = new Dictionary<string, (string, object)>();

        public EvaluationTarget(string templateName, object scope)
        {
            TemplateName = templateName;
            Scope = scope;
        }

        public string TemplateName { get; set; }

        public object Scope { get; set; }

        /// <summary>
        /// Get the cached result of an expression. 
        /// If a result is cached under a differnt version of memory than the current one
        /// it will be considered as invalid cache.
        /// </summary>
        /// <param name="exp">the exp as key.</param>
        /// <returns>cached value.</returns>
        public object ExpressionCacheGet(string exp)
        {
            if (expCache.TryGetValue(exp, out var result))
            {
                var curVersion = MemoryVersion();
                if (result.Item1.Equals(curVersion))
                {
                    return result.Item2;
                }
            }

            return null;
        }

        /// <summary>
        /// Set the expression cache, auto-attach the current memory version.
        /// </summary>
        /// <param name="exp">the expression as string.</param>
        /// <param name="result">the result.</param>
        public void ExpressionCacheSet(string exp, object result)
        {
            expCache[exp] = (MemoryVersion(), result);
        }

        private string MemoryVersion()
        {
            return Scope == null ? string.Empty : ((CustomizedMemory)Scope).Version();
        }
    }
}
