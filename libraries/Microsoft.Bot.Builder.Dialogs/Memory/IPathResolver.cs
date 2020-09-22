// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Defines Path Resolver interface for transforming paths.
    /// </summary>
    public interface IPathResolver
    {
        /// <summary>
        /// Transform the path.
        /// </summary>
        /// <param name="path">path to inspect.</param>
        /// <returns>transformed path.</returns>
        string TransformPath(string path);
    }
}
