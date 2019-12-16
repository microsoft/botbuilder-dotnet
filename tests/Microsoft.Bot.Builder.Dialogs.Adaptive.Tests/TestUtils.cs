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
using Microsoft.Bot.Builder.MockLuis;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class TestUtils
    {
        public static string RootFolder { get; set; } = GetProjectPath();

        public static ResourceExplorer ResourceExplorer { get; set; }

        public static IEnumerable<object[]> GetTestScripts(string relativeFolder)
        {
            string testFolder = Path.GetFullPath(Path.Combine(RootFolder, PathUtils.NormalizePath(relativeFolder)));
            return Directory.EnumerateFiles(testFolder, "*.test.dialog", SearchOption.AllDirectories).Select(s => new object[] { Path.GetFileName(s) }).ToArray();
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            lock (RootFolder)
            {
                if (ResourceExplorer == null)
                {
                    ResourceExplorer = new ResourceExplorer().AddFolder(RootFolder);
                }
            }
        }

        public static async Task RunTestScript(string resourceId = null, [CallerMemberName] string testName = null, IConfiguration configuration = null)
        {
            TestScript script;
            
            // TODO: For now, serialize type loading because config is static and is used by LUIS type loader.
            lock (RootFolder)
            {
                if (configuration == null || configuration != TypeFactory.Configuration)
                {
                    DeclarativeTypeLoader.Reset();
                    TypeFactory.Configuration = configuration ?? new ConfigurationBuilder().AddInMemoryCollection().Build();
                    DeclarativeTypeLoader.AddComponent(new DialogComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new QnAMakerComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new MockLuisComponentRegistration());
                }

                script = TestUtils.ResourceExplorer.LoadType<TestScript>(resourceId ?? $"{testName}.test.dialog");
            }

            script.Description = script.Description ?? resourceId;
            await script.ExecuteAsync(testName: testName, configuration: configuration, resourceExplorer: ResourceExplorer).ConfigureAwait(false);
        }

        private static string GetProjectPath()
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

            return parent;
        }
    }
}
