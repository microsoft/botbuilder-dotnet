using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Tests
{
    [TestClass]
    public class ResourceTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
        private string testCogFile = Path.Combine(Environment.CurrentDirectory, "foo.cog");

        public TestContext TestContext { get; set; }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(testCogFile);
        }

        [TestMethod]
        public async Task TestFolderSource()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\.."));
            FolderResourceProvider source = new FolderResourceProvider(path);

            await AssertResourceType(path, source, "cog");
            await AssertResourceType(path, source, "schema");
            await AssertResourceType(path, source, "lg");
            await AssertResourceType(path, source, "lu");

            // get resource value as text or binary
            var cogs = await source.GetResources("cog");
            Assert.AreEqual("{}", await cogs[0].GetTextAsync());

            // unknown resource type
            var resources = await source.GetResources("foo");
            Assert.AreEqual(0, resources.Length);
        }

        [TestMethod]
        public async Task TestFolderSource_Changed()
        {
            TaskCompletionSource<bool> changeFired = new TaskCompletionSource<bool>();
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\.."));
            FolderResourceProvider source = new FolderResourceProvider(path);
            source.Changed += (src, resource) =>
            {
                Assert.AreEqual(source, src);
                Assert.AreEqual("foo", resource.Name);
                Assert.AreEqual("cog", resource.ResourceType);
                changeFired.SetResult(true);
            };

            // make sure we are loaded and listening...
            var resources = await source.GetResources("cog");
            Assert.AreEqual(1, resources.Length);
            Assert.AreEqual("cog", resources[0].ResourceType);
            File.WriteAllText(testCogFile, "{}");
            await changeFired.Task;
        }

        private static async Task AssertResourceType(string path, FolderResourceProvider source, string resourceType)
        {
            var resources = await source.GetResources(resourceType);
            Assert.AreEqual(1, resources.Length);
            Assert.AreEqual(resourceType, resources[0].ResourceType);
            Assert.AreEqual("test", resources[0].Name);
            Assert.AreEqual(Path.Combine(path, $@"resources\test.{resourceType}"), resources[0].Id);
        }


        [TestMethod]
        public async Task TestNugetSource()
        {
            // don't have a nuget source yet!?
        }

        [TestMethod]
        public async Task TestResourceManager_AddProject()
        {
            string projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $@"..\..\..\Microsoft.Bot.Builder.Dialogs.Composition.Tests.csproj"));
            string projRefPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $@"..\..\..\..\..\libraries\Microsoft.Bot.Builder.Dialogs"));
            System.Diagnostics.Debug.Print(projRefPath);
            BotResourceManager manager = new BotResourceManager();
            manager.AddProjectResources(projPath);
            var x = manager.Providers.Select(s => { System.Diagnostics.Debug.WriteLine(s.Id); return s.Id; }).ToArray();
            Assert.IsTrue(manager.Providers.Any(s => Path.Equals(s.Id, Path.GetDirectoryName(projPath))), "project folder not added");
            Assert.IsTrue(manager.Providers.Any(s => Path.Equals(s.Id, projRefPath)), "project reference not added");
            // TODO we need a nuget source
            // Assert.IsTrue(manager.Sources.Any(s => s.Id.EndsWith("nuget package with resources.csproj")), "nuget reference not added");

            var cogs = await manager.GetResources("cog");
            Assert.IsTrue(cogs.Any(cog => cog.Name == "test" && cog.ResourceType == "cog"), "should have found the cog file in current project");

            var schemas = await manager.GetResources("schema");
            Assert.IsTrue(schemas.Any(schema => schema.Name == "Microsoft.IntegerPrompt" && schema.ResourceType == "schema"), "should have found schema file from project reference");
        }

    }
}
