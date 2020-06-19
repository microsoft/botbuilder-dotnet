// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Observers
{
    /// <summary>
    /// Passes for the 2-pass cycle detection algorithm implemented in <see cref="CycleDetectionObserver"/>.
    /// This algorithm is used to detect cycles in Json type loading by the different converters registered
    /// by declarative components.
    /// </summary>
    internal enum CycleDetectionPasses
    {
        /// <summary>
        /// First pass of the 2-pass cycle detection algorithm. 
        /// </summary>
        /// <remarks>
        /// The first pass builds all objects and caches them, but stops processing branches whenever it
        /// finds a cycle.
        /// </remarks>
        PassOne,

        /// <summary>
        /// Second pass of the 2-pass cycle detection algorithm.
        /// </summary>
        /// <remarks>
        /// The second pass takes all items from the cache, so there is no real serialization but 
        /// mostly stitching objects together from the cache.
        /// </remarks>
        PassTwo
    }
}
