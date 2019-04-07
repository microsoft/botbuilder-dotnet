// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public delegate void ResourceChangedEventHandler(string[] paths);

    public interface IResourceExplorer
    {
        void AddFolder(string path, bool monitorFiles = true);

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<DirectoryInfo> Folders { get; set; }

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is changed.
        /// </summary>
        event ResourceChangedEventHandler Changed;
    }
}
