using System.Collections.Generic;
using System.IO;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public interface IResourceExplorer
    {
        IEnumerable<DirectoryInfo> Folders { get; set; }

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is deleted.
        /// </summary>
        event FileSystemEventHandler Deleted;

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is created.
        /// </summary>
        event FileSystemEventHandler Created;

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is changed.
        /// </summary>
        event FileSystemEventHandler Changed;

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is renamed.
        /// </summary>
        event RenamedEventHandler Renamed;
    }
}
