// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A utilities class of file path operations.
    /// </summary>
    public static class PathUtils
    {
        /// <summary>
        /// Normalizes authored path to OS-compatible path.
        /// </summary>
        /// <remarks>
        /// Path is from authored content which doesn't know what OS it is running on.
        /// This method treats / and \ both as separators regardless of OS, for Windows that means
        /// changing all `/` characters to `/`, and for Linux/Mac `\` to `/`.
        /// This allows author to use ../foo.lg or ..\foo.lg as equivalents for importing.
        /// </remarks>
        /// <param name="ambiguousPath">authoredPath.</param>
        /// <returns>path expressed as OS path.</returns>
        public static string NormalizePath(string ambiguousPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Map Linux/Mac separator to Windows.
                return ambiguousPath.Replace("/", "\\");
            }
            else
            {
                // Map Windows separator to Linux/Mac.
                return ambiguousPath.Replace("\\", "/");
            }
        }
    }
}
