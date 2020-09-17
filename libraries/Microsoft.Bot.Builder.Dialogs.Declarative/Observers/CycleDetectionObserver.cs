// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Observers
{
    /// <summary>
    /// <see cref="IConverterObserver"/> dedicated to find cycles and properly aid in the type-loading
    /// of cyclical graphs.
    /// </summary>
    /// <remarks>
    /// Cycles are detected by analyzing the call stack. On the 2 pass algorithm, the first pass loads and 
    /// caches until a cycle is found in the stack. The second pass connects the loaded pieces together.
    /// </remarks>
    internal class CycleDetectionObserver : IJsonLoadObserver
    {
        private readonly bool allowCycle;
        private readonly Dictionary<int, object> cache = new Dictionary<int, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CycleDetectionObserver"/> class.
        /// </summary>
        /// <param name="allowCycle">If allowCycle is set to false, throw an exception when detecting cycle.</param>
        public CycleDetectionObserver(bool allowCycle = true)
        {
            this.allowCycle = allowCycle;
        }

        /// <summary>
        /// Gets or sets the current pass of the algorithm.
        /// </summary>
        /// <value>
        /// The current pass of the algorithm.
        /// </value>
        public CycleDetectionPasses CycleDetectionPass { get; set; } = CycleDetectionPasses.PassOne;

        /// <summary>
        /// Notifies <see cref="IConverterObserver"/> instances before type-loading a <see cref="JToken"/>.
        /// </summary>
        /// <typeparam name="T">Type of the concrete object to be built.</typeparam>
        /// <param name="context">Source context for the current token.</param>
        /// <param name="range">Source range for the current token.</param>
        /// <param name="token">Token to be used to build the object.</param>
        /// <param name="result">Output parameter for observer to provide its result to the converter.</param>
        /// <returns>True if the observer provides a result and False if not.</returns>
        public bool OnBeforeLoadToken<T>(SourceContext context, SourceRange range, JToken token, out T result)
            where T : class
        {
            // The deep hash code provides a pseudo-unique id for a given token.
            // The token is characterized for the source range and the type expected for that source range.
            var hashCode = Hash<T>(range);

            // Now analyze the stack to find cycles.
            // If the same source range appears twice in the stack, we have a cycle.
            var isCycle = context.CallStack.Count(s => s.Equals(range)) > 1;

            if (isCycle && !allowCycle)
            {
                throw new InvalidOperationException($"Cycle detected for range: {range}");
            }

            if (isCycle && CycleDetectionPass == CycleDetectionPasses.PassOne)
            {
                // If we found a cycle, stop iterating and set the value to null in pass 1.
                result = null;
                return true;
            }

            if (CycleDetectionPass == CycleDetectionPasses.PassTwo)
            {
                // If we already visited this item in pass 2 means we found a loop.
                // Since in pass 1 we should have filled the cache with the missing objects,
                // now we just bring the items from pass 1 to stitch the full object together
                if (isCycle)
                {
                    // We found a loop and we have the final value in the cache. Return that value.
                    if (cache.ContainsKey(hashCode))
                    {
                        result = cache[hashCode] as T;
                    }

                    // Even if the value was no in the cache, we set as null since we don't want
                    // to have an infinite loop in pass 2 either.
                    else
                    {
                        result = null;
                    }

                    return true;
                }
            }

            // When no cycle was detected, we return false to avoid interfering with regular loading.
            result = null;
            return false;
        }

        /// <summary>
        /// Notifies <see cref="IConverterObserver"/> instances after type-loading a <see cref="JToken"/> into the 
        /// provided instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the concrete object that was built.</typeparam>
        /// <param name="context">Source context for the current token.</param>
        /// <param name="range">Source range for the current token.</param>
        /// <param name="token">Token used to build the object.</param>
        /// <param name="loaded">Object that was built using the token.</param>
        /// <param name="result">Output parameter for observer to provide its result to the converter.</param>
        /// <returns>True if the observer provides a result and False if not.</returns>
        public bool OnAfterLoadToken<T>(SourceContext context, SourceRange range, JToken token, T loaded, out T result)
            where T : class
        {
            // The deep hash code provides a pseudo-unique id for a given token
            var hashCode = Hash<T>(range);

            // In pass 1, after we build an object, we add it to the cache.
            if (CycleDetectionPass == CycleDetectionPasses.PassOne)
            {
                cache[hashCode] = loaded;
                result = null;
                return false;
            }

            // In pass 2, no action or interference is required.
            else
            {
                result = null;
                return false;
            }
        }

        private static int Hash<T>(SourceRange range)
        {
            // The same json may resolve to two types. The cache key should include
            // type information.
            return CombineHashCodes(
                new[]
                {
                    range.GetHashCode(),
                    typeof(T).GetHashCode()
                });
        }

        private static int CombineHashCodes(IEnumerable<int> hashCodes)
        {
            // System.String.GetHashCode(): http://referencesource.microsoft.com/#mscorlib/system/string.cs,0a17bbac4851d0d4
            // System.Web.Util.StringUtil.GetStringHashCode(System.String): http://referencesource.microsoft.com/#System.Web/Util/StringUtil.cs,c97063570b4e791a

            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            int i = 0;
            foreach (var hashCode in hashCodes)
            {
                if (i % 2 == 0)
                {
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hashCode;
                }
                else
                {
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hashCode;
                }

                ++i;
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}
