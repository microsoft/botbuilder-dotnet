// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// A source map is a dictionary object -> sourceCode 
    /// </summary>
    public interface ISourceMap
    {
        /// <summary>
        /// Add an object and SourceRange information describing where the object was defined
        /// </summary>
        /// <param name="item">item to record</param>
        /// <param name="range">sourceBlock range for the object</param>
        void Add(object item, SourceRange range);

        /// <summary>
        /// Look up the SourceRange information for an object 
        /// </summary>
        /// <param name="item">object to look up</param>
        /// <param name="range">place to return the SourceRange for the object</param>
        /// <returns>true if found</returns>
        bool TryGetValue(object item, out SourceRange range);
    }
}
