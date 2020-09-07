using System;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class ResourceExplorerFixture : IDisposable
    {
        private string _projectPath;

        public ResourceExplorerFixture()
        {
            ResourceExplorer = new ResourceExplorer();
            _projectPath = TestUtils.GetProjectPath();
        }

        public ResourceExplorer ResourceExplorer { get; private set; }

        public ResourceExplorerFixture AddFolder(string resourceFolder)
        {
            var folderPath = Path.Combine(_projectPath, "Tests", resourceFolder);
            
            if (!ResourceExplorer.ResourceProviders.Any(e => e.Id == folderPath))
            {
                ResourceExplorer.AddFolder(folderPath, monitorChanges: false);
            }

            return this;
        }

        public void Dispose()
        {
            ResourceExplorer.Dispose();
            _projectPath = null;
        }
    }
}
