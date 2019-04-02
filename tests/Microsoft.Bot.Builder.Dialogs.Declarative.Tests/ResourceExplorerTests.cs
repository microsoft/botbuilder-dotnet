// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    [TestClass]
    public class ResourceTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
        private string testDialogFile = Path.Combine(Environment.CurrentDirectory, "foo.dialog");

        public TestContext TestContext { get; set; }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(testDialogFile);
        }

        [TestMethod]
        public async Task TestFolderSource()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\.."));
            var explorer = new ResourceExplorer();
            explorer.AddFolder(path);

            await AssertResourceType(path, explorer, "dialog");
            var resources = explorer.GetResources("foo").ToArray();
            Assert.AreEqual(0, resources.Length);
        }

        [TestMethod]
        public async Task TestFolderSource_Watcher()
        {
            File.Delete(testDialogFile);
            TaskCompletionSource<bool> changeFired = new TaskCompletionSource<bool>();

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\.."));
            var explorer = new ResourceExplorer();
            explorer.AddFolder(path);

            explorer.Changed += (src, resource) =>
            {
                if (Path.GetFileName(resource.Name) == "foo.dialog")
                {
                    changeFired.SetResult(true);
                    changeFired = new TaskCompletionSource<bool>();
                }
            };

            // make sure we are loaded and listening...
            var resources = explorer.GetResources("dialog").ToArray();
            Assert.AreEqual(1, resources.Length);
            Assert.AreEqual(".dialog", resources[0].Extension);

            // new file
            File.WriteAllText(testDialogFile, "{}");
            await changeFired.Task.ConfigureAwait(false);

            // changed file
            File.WriteAllText(testDialogFile, "{}");
            await changeFired.Task.ConfigureAwait(false);

            // delete file
            File.Delete(testDialogFile);
            await changeFired.Task.ConfigureAwait(false);
        }

        private static async Task AssertResourceType(string path, ResourceExplorer explorer, string resourceType)
        {
            var resources = explorer.GetResources(resourceType).ToArray();
            Assert.AreEqual(1, resources.Length);
            Assert.AreEqual($".{resourceType}", resources[0].Extension);
            Assert.AreEqual("test", Path.GetFileNameWithoutExtension(resources[0].Name));
            Assert.AreEqual(Path.Combine(path, $@"resources\test.{resourceType}"), resources[0].FullName);
        }


        //[TestMethod]
        //public async Task TestNugetSource()
        //{
        //    // don't have a nuget source yet!?
        //}

        //[TestMethod]
        //public async Task TestResourceManager_AddProject()
        //{
        //    string projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $@"..\..\..\Microsoft.Bot.Builder.Dialogs.Composition.Tests.csproj"));
        //    string projRefPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $@"..\..\..\..\..\libraries\Microsoft.Bot.Builder.Dialogs"));
        //    System.Diagnostics.Debug.Print(projRefPath);
        //    ResourceExplorer manager = new ResourceExplorer();
        //    manager.AddProjectResources(projPath);
        //    var x = manager.Providers.Select(s => { System.Diagnostics.Debug.WriteLine(s.Id); return s.Id; }).ToArray();
        //    Assert.IsTrue(manager.Providers.Any(s => Path.Equals(s.Id, Path.GetDirectoryName(projPath))), "project folder not added");
        //    Assert.IsTrue(manager.Providers.Any(s => Path.Equals(s.Id, projRefPath)), "project reference not added");
        //    // TODO we need a nuget source
        //    // Assert.IsTrue(manager.Sources.Any(s => s.Id.EndsWith("nuget package with resources.csproj")), "nuget reference not added");

        //    var dialogs = await manager.GetResources("dialog");
        //    Assert.IsTrue(dialogs.Any(dialog => dialog.Name == "test" && dialog.ResourceType == "dialog"), "should have found the dialog file in current project");

        //    var schemas = await manager.GetResources("schema");
        //    Assert.IsTrue(schemas.Any(schema => schema.Name == "Microsoft.IntegerPrompt" && schema.ResourceType == "schema"), "should have found schema file from project reference");
        //}

    }
}
