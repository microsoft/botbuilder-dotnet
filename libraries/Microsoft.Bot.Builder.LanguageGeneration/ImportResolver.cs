﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Delegate for resolving resource id of imported lg file.
    /// </summary>
    /// <param name="resourceId">Resource id to resolve.</param>
    /// <returns>Resolved resource content and unique id.</returns>
    public delegate (string content, string id) ImportResolverDelegate(string resourceId);

    public class ImportResolver
    {
        public static ImportResolverDelegate FilePathResolver(string filePath)
        {
            return (id) =>
            {
                // import paths are in resource files which can be executed on multiple OS environments
                // Call GetOsPath() to map / & \ in importPath -> OSPath
                var importPath = GetOsPath(id);
                if (!Path.IsPathRooted(importPath))
                {
                    // get full path for importPath relative to path which is doing the import.
                    importPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath), id));
                }

                return (File.ReadAllText(importPath), importPath);
            };
        }

        public static ImportResolverDelegate FileResolver()
        {
            return (id) =>
            {
                id = Path.GetFullPath(id);
                return (File.ReadAllText(id), id);
            };
        }

        /// <summary>
        /// Normalize authored path to os path.
        /// </summary>
        /// <remarks>
        /// path is from authored content which doesn't know what OS it is running on.
        /// This method treats / and \ both as seperators regardless of OS, for windows that means / -> \ and for linux/mac \ -> /.
        /// This allows author to use ../foo.lg or ..\foo.lg as equivelents for importing.
        /// </remarks>
        /// <param name="ambigiousPath">authoredPath.</param>
        /// <returns>path expressed as OS path.</returns>
        private static string GetOsPath(string ambigiousPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // map linux/mac sep -> windows
                return ambigiousPath.Replace("/", "\\");
            }
            else
            {
                // map windows sep -> linux/mac
                return ambigiousPath.Replace("\\", "/");
            }
        }
    }
}
