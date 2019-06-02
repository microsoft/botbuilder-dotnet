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

        private static string getOsPath(string path) => Path.Combine(path.TrimEnd('\\').Split('\\'));

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(testDialogFile);
        }

        [TestMethod]
        public async Task TestFolderSource()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, getOsPath(@"..\..\..")));
            using(var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(path));

                await AssertResourceType(path, explorer, "dialog");
                var resources = explorer.GetResources("foo").ToArray();
                Assert.AreEqual(0, resources.Length);
            }
        }

        [TestMethod]
        public async Task TestFolderSource_Shallow()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, getOsPath(@"..\..\..")));
            using(var explorer = new ResourceExplorer())
            {
                explorer.AddFolder(path, includeSubFolders: false);

                var resources = explorer.GetResources("dialog").ToArray();
                Assert.AreEqual(0, resources.Length, "shallow folder shouldn't list the dialog resources");

                resources = explorer.GetResources("cs").ToArray();
                Assert.IsTrue(resources.Length > 0, "shallow folder should list the root files");
            }
        }

        [TestMethod]
        public async Task TestFolderSource_NewFiresChanged()
        {
            File.Delete(testDialogFile);

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, getOsPath(@"..\..\..")));
            using(var explorer = new ResourceExplorer())
            {
                explorer.AddFolder(path, monitorChanges: true);

                TaskCompletionSource<bool> changeFired = new TaskCompletionSource<bool>();

                explorer.Changed += (resources) =>
                {
                    if (resources.Any(resource => resource.Id == "foo.dialog"))
                    {
                        changeFired.SetResult(true);
                    }
                };

                // new file
                File.WriteAllText(testDialogFile, "{}");
                await changeFired.Task.ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task TestFolderSource_WriteFiresChanged()
        {
            File.Delete(testDialogFile);
            File.WriteAllText(testDialogFile, "{}");

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, getOsPath(@"..\..\..")));
            using(var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(path, monitorChanges: true));

                TaskCompletionSource<bool> changeFired = new TaskCompletionSource<bool>();

                explorer.Changed += (resources) =>
                {
                    if (resources.Any(resource => resource.Id == "foo.dialog"))
                    {
                        changeFired.SetResult(true);
                    }
                };

                // changed file
                File.WriteAllText(testDialogFile, "{'foo':123 }");
                await changeFired.Task.ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task TestFolderSource_DeleteFiresChanged()
        {
            File.Delete(testDialogFile);
            File.WriteAllText(testDialogFile, "{}");

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, getOsPath(@"..\..\..")));
            using(var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(path, monitorChanges:true));

                TaskCompletionSource<bool> changeFired = new TaskCompletionSource<bool>();

                explorer.Changed += (resources) =>
                {
                    if (resources.Any(resource => resource.Id == "foo.dialog"))
                    {
                        changeFired.SetResult(true);
                    }
                };
                // changed file
                File.Delete(testDialogFile);
                await changeFired.Task.ConfigureAwait(false);
            }
        }

        private static async Task AssertResourceType(string path, ResourceExplorer explorer, string resourceType)
        {
            var resources = explorer.GetResources(resourceType).ToArray();
            Assert.AreEqual(1, resources.Length);
            Assert.AreEqual($".{resourceType}", Path.GetExtension(resources[0].Id));
            Assert.AreEqual("test", Path.GetFileNameWithoutExtension(resources[0].Id));
        }
    }
}
