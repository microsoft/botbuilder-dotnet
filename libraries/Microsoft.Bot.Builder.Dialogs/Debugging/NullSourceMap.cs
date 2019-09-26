// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// NullSourceMap is used to disable tracking of source code Ranges
    /// </summary>
    public class NullSourceMap : ISourceMap
    {
        public static readonly NullSourceMap Instance = new NullSourceMap();

        public void Add(object item, SourceRange range)
        {
        }

        public bool TryGetValue(object item, out SourceRange range)
        {
            range = null;
            return false;
        }
    }
}
