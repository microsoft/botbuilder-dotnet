// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// NullSourceMap is used to disable tracking of source code Ranges.
    /// </summary>
    public class NullSourceMap : ISourceMap
    {
        /// <summary>
        /// Initializes a read-only new instance of the <see cref="NullSourceMap"/>.
        /// </summary>
        public static readonly NullSourceMap Instance = new NullSourceMap();

        /// <summary>
        /// Add an object and SourceRange information describing where the object was defined.
        /// </summary>
        /// <param name="item">Object item to record.</param>
        /// <param name="range"><see cref="SourceRange"/> for the object.</param>
        /// <remarks>For a <see cref="NullSourceMap"/> it does nothing.</remarks>
        public void Add(object item, SourceRange range)
        {
        }

        /// <summary>
        /// Look up the <see cref="SourceRange"/> information for an object.
        /// </summary>
        /// <param name="item">Object to look up.</param>
        /// <param name="range">Place to return the <see cref="SourceRange"/> for the object.</param>
        /// <returns><c>true</c> if found.</returns>
        /// <remarks>For a <see cref="NullSourceMap"/> always returns <c>false</c>.</remarks>
        public bool TryGetValue(object item, out SourceRange range)
        {
            range = null;
            return false;
        }
    }
}
