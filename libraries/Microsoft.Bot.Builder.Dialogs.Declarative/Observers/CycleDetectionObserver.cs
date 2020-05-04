// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Observers
{
    /// <summary>
    /// <see cref="IConverterObserver"/> dedicated to find cycles and properly aid in the type-loading
    /// of cyclical graphs.
    /// </summary>
    internal class CycleDetectionObserver : IConverterObserver
    {
        private readonly Dictionary<int, object> cache = new Dictionary<int, object>();
        private readonly HashSet<int> visitedPassOne = new HashSet<int>();
        private readonly HashSet<int> visitedPassTwo = new HashSet<int>();

        /// <summary>
        /// Gets or sets the current pass of the algorithm.
        /// </summary>
        /// <value>
        /// The current pass of the algorithm.
        /// </value>
        public CycleDetectionPasses CycleDetectionPass { get; set; } = CycleDetectionPasses.PassOne;

        /// <inheritdoc/>
        public bool OnBeforeLoadToken<T>(JToken token, out T result)
            where T : class
        {
            // The deep hash code provides a pseudo-unique id for a given token
            var hashCode = Hash<T>(token);

            // Pass 1: We track the already visited objects' hash in the 'visited' collection
            // If we've already visited this hash code and we are still in pass 1, 
            // we found a loop! If we found a loop, we want to return a value and stop deserializing
            // to avoid infinite loops.
            if (visitedPassOne.Contains(hashCode) && CycleDetectionPass == CycleDetectionPasses.PassOne) 
            {
                // If we already have a hydrated object for this hash code, return it
                if (cache.ContainsKey(hashCode))
                {
                    result = cache[hashCode] as T;
                }

                // If we don't have a cached value for this hash code, we send null as the value.
                // Pass 2 will exist with the purpose of filling in these nulled values with the 
                // references discovered and cached during pass 1
                else
                {
                    result = null;
                }

                return true;
            }

            if (CycleDetectionPass == CycleDetectionPasses.PassTwo) 
            {
                // If we already visited this item in pass 2 means we found a loop.
                // Since in pass 1 we should have filled the cache with the missing objects,
                // now we just bring the items from pass 1 to stitch the full object together
                if (visitedPassTwo.Contains(hashCode))
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

                visitedPassTwo.Add(hashCode);
            }

            visitedPassOne.Add(hashCode);
            result = null;
            return false;
        }

        /// <inheritdoc/>
        public bool OnAfterLoadToken<T>(JToken token, T loaded, out T result)
            where T : class
        {
            // The deep hash code provides a pseudo-unique id for a given token
            var hashCode = Hash<T>(token);

            // In pass 1, after we build an object, we add it to the cache.
            if (CycleDetectionPass == CycleDetectionPasses.PassOne) 
            {
                cache[hashCode] = loaded;
                result = null;
                return false;
            }

            // In pass 2, we track the objects that we visited to avoid
            // infinite loops in pass 2. If we have a value in the cache,
            // we prefer the cache value.
            else
            {
                visitedPassTwo.Add(hashCode);
                
                result = null;
                return false;
            }
        }

        private static int Hash<T>(JToken jToken)
        {
            // The same json may resolve to two types. The cache key should include
            // type information.
            return CombineHashCodes(new[] { jToken.GetHashCode(), typeof(T).GetHashCode() });
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
