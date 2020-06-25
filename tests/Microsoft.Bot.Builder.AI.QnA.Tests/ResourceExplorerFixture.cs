using System;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    public class ResourceExplorerFixture : IDisposable
    { 
        public ResourceExplorerFixture()
        {
            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = Path.GetDirectoryName(parent);
                }
            }

            ResourceExplorer = new ResourceExplorer()
                .AddFolder(parent, monitorChanges: false);
        }

        public static ResourceExplorer ResourceExplorer { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
