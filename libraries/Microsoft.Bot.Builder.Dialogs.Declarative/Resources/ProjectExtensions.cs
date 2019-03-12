using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public static class ProjectExtensions
    {
        /// <summary>
        /// Add a folder as a resource source
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static BotResourceManager AddFolderResources(this BotResourceManager manager, string folder)
        {
            folder = Path.GetFullPath(folder);
            if (!manager.Providers.Where(s => Path.Equals(s.Id, folder)).Any())
            {
                manager.Providers.Add(new FolderResourceProvider(folder, monitorChanges: manager.MonitorChanges));
            }

            return manager;
        }

        /// <summary>
        /// Add a .csproj as resource (adding the project, referenced projects and referenced packages)
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        public static BotResourceManager AddProjectResources(this BotResourceManager manager, string projectFile)
        {
            if (!File.Exists(projectFile))
            {
                projectFile = Directory.EnumerateFiles(projectFile, "*.*proj").FirstOrDefault();
                if (projectFile == null)
                {
                    throw new ArgumentNullException(nameof(projectFile));
                }
            }
            string projectFolder = Path.GetDirectoryName(projectFile);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(projectFile);

            // add folder for the project
            manager.AddFolderResources(projectFolder);

            // add project references
            foreach (XmlNode node in xmlDoc.SelectNodes("//ProjectReference"))
            {
                var path = Path.Combine(projectFolder, node.Attributes["Include"].Value);
                path = Path.GetFullPath(path);
                path = Path.GetDirectoryName(path);
                manager.AddFolderResources(path);
            }

            // add nuget package references
            foreach (XmlNode node in xmlDoc.SelectNodes("//PackageReference"))
            {
                manager.Providers.Add(new NugetResourceProvider(node.Attributes["Include"]?.Value, node.Attributes["Version"]?.Value));
            }

            return manager;
        }
    }
}
