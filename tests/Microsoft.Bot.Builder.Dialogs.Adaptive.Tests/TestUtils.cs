using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class TestUtils
    {
        private static string rootFolder = PathUtils.NormalizePath(@"..\..\..");

        public static ResourceExplorer ResourceExplorer { get; set; }

        public static IEnumerable<object[]> GetTestScripts(string relativeFolder)
        {
            string testFolder = Path.GetFullPath(Path.Combine(rootFolder, PathUtils.NormalizePath(relativeFolder)));
            return Directory.EnumerateFiles(testFolder, "*.test.dialog", SearchOption.AllDirectories).Select(s => new object[] { Path.GetFileName(s) }).ToArray();
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            lock (rootFolder)
            {
                if (ResourceExplorer == null)
                {
                    ResourceExplorer = new ResourceExplorer().AddFolder(rootFolder);
                    TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
                    DeclarativeTypeLoader.AddComponent(new DialogComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new QnAMakerComponentRegistration());
                }
            }
        }

        public static async Task RunTestScript(string resourceId, [CallerMemberName] string testName = null, IConfiguration configuration = null)
        {
            var script = TestUtils.ResourceExplorer.LoadType<TestScript>(resourceId);
            script.Description = script.Description ?? resourceId;
            await script.ExecuteAsync(testName: testName, configuration: configuration).ConfigureAwait(false);
        }
    }
}
