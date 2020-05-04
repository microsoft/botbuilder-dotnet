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
        private readonly Dictionary<long, object> cache = new Dictionary<long, object>();
        private readonly HashSet<long> visitedPassOne = new HashSet<long>();
        private readonly HashSet<long> visitedPassTwo = new HashSet<long>();

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
            // JTokenEqualityComparer does a deep hash code for JToken objects
            // The deep hash code provides a pseudo-unique id for a given token
            var jTokenComparer = new JTokenEqualityComparer();
            var hashCode = jTokenComparer.GetHashCode(token);

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
            var jTokenComparer = new JTokenEqualityComparer();
            var hashCode = jTokenComparer.GetHashCode(token);

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
                result = cache[hashCode] as T;
                return true;
            }
        }
    }
}
