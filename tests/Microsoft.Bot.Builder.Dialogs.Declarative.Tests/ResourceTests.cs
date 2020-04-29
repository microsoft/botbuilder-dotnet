// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    [TestClass]
    public class ResourceTests
    {
        private static JsonSerializerSettings jsonSerializerSettings =
            new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var path = Path.GetFullPath(PathUtils.NormalizePath(Path.Combine(Environment.CurrentDirectory, @"..\..")));
            foreach (var file in Directory.EnumerateFiles(path, "*.dialog", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }

        [TestMethod]
        public async Task TestFolderSource()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                await AssertResourceType(path, explorer, "dialog");
                var resources = explorer.GetResources("foo").ToArray();
                Assert.AreEqual(0, resources.Length);
            }
        }

        [TestMethod]
        public void TestMissingResourceThrows()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));
                try
                {
                    explorer.GetResource("bogus.dialog");
                    Assert.Fail($"should have thrown exception");
                }
                catch (ArgumentException err)
                {
                    Assert.IsTrue(err.Message.Contains("bogus"));
                    Assert.AreEqual("bogus.dialog", err.ParamName);
                }
                catch (Exception err2)
                {
                    Assert.Fail($"Unknown exception {err2.GetType().Name} thrown");
                }
            }
        }

        [TestMethod]
        public void TestFolderSource_Shallow()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddFolder(path, includeSubFolders: false);

                var resources = explorer.GetResources("dialog").ToArray();
                Assert.AreEqual(0, resources.Length, "shallow folder shouldn't list the dialog resources");

                resources = explorer.GetResources("schema").ToArray();
                Assert.IsTrue(resources.Length > 0, "shallow folder should list the root files");
            }
        }

        [TestMethod]
        public async Task TestFolderSource_NewFiresChanged()
        {
            string testId = "NewFiresChanged.dialog";
            string testDialogFile = Path.Combine(Environment.CurrentDirectory, testId);

            File.Delete(testDialogFile);

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddFolder(path, monitorChanges: true);

                AssertResourceNull(explorer, testId);

                TaskCompletionSource<bool> changeFired = new TaskCompletionSource<bool>();

                explorer.Changed += (e, resources) =>
                {
                    if (resources.Any(resource => resource.Id == testId))
                    {
                        changeFired.SetResult(true);
                    }
                };

                // new file
                File.WriteAllText(testDialogFile, "{}");

                await Task.WhenAny(changeFired.Task, Task.Delay(5000)).ConfigureAwait(false);

                AssertResourceFound(explorer, testId);
            }
        }

        [TestMethod]
        public async Task TestFolderSource_WriteFiresChanged()
        {
            string testId = "WriteFiresChanged.dialog";
            string testDialogFile = Path.Combine(Environment.CurrentDirectory, testId);

            File.Delete(testDialogFile);
            string contents = "{}";
            File.WriteAllText(testDialogFile, contents);

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path, monitorChanges: true));

                AssertResourceFound(explorer, testId);

                await AssertResourceContents(explorer, testId, contents);

                TaskCompletionSource<bool> changeFired = new TaskCompletionSource<bool>();

                explorer.Changed += (e, resources) =>
                {
                    if (resources.Any(res => res.Id == testId))
                    {
                        changeFired.SetResult(true);
                    }
                };

                // changed file
                contents = "{'foo':123 }";
                File.WriteAllText(testDialogFile, contents);

                await Task.WhenAny(changeFired.Task, Task.Delay(5000)).ConfigureAwait(false);

                AssertResourceFound(explorer, testId);

                await AssertResourceContents(explorer, testId, contents);
            }
        }

        [TestMethod]
        public async Task TestFolderSource_DeleteFiresChanged()
        {
            string testId = "DeleteFiresChanged.dialog";
            string testDialogFile = Path.Combine(Environment.CurrentDirectory, testId);

            File.Delete(testDialogFile);
            File.WriteAllText(testDialogFile, "{}");

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path, monitorChanges: true));

                AssertResourceFound(explorer, testId);

                TaskCompletionSource<bool> changeFired = new TaskCompletionSource<bool>();

                explorer.Changed += (e, resources) =>
                {
                    if (resources.Any(resource => resource.Id == testId))
                    {
                        changeFired.SetResult(true);
                    }
                };

                // changed file
                File.Delete(testDialogFile);

                await Task.WhenAny(changeFired.Task, Task.Delay(5000)).ConfigureAwait(false);

                AssertResourceNull(explorer, testId);
            }
        }

        private static async Task AssertResourceType(string path, ResourceExplorer explorer, string resourceType)
        {
            var resources = explorer.GetResources(resourceType).ToArray();
            Assert.AreEqual(1, resources.Length);
            Assert.AreEqual($".{resourceType}", Path.GetExtension(resources[0].Id));
            await Task.FromResult<object>(null);
        }

        private static void AssertResourceFound(ResourceExplorer explorer, string id)
        {
            var dialog = explorer.GetResource(id);
            Assert.IsNotNull(dialog, $"getResource({id}) should return resource");
            var dialogs = explorer.GetResources("dialog");
            Assert.IsTrue(dialogs.Where(d => d.Id == id).Any(), $"getResources({id}) should return resource");
        }

        private static void AssertResourceNull(ResourceExplorer explorer, string id)
        {
            try
            {
                var dialog = explorer.GetResource(id);
                Assert.Fail($"GetResource({id}) should throw");
            }
            catch (ArgumentException err)
            {
                Assert.AreEqual(err.ParamName, id, "Should throw error with resource id in it");
            }

            var dialogs = explorer.GetResources("dialog");
            Assert.IsFalse(dialogs.Where(d => d.Id == id).Any(), $"getResources({id}) should not return resource");
        }

        private async Task AssertResourceContents(ResourceExplorer explorer, string id, string contents)
        {
            var resource = explorer.GetResource(id);

            var text = await resource.ReadTextAsync();
            Assert.AreEqual(contents, text, $"getResource({id}) contents not the same ");
            resource = explorer.GetResources("dialog").Where(d => d.Id == id).Single();

            text = await resource.ReadTextAsync();
            Assert.AreEqual(contents, text, $"getResources({id}) contents not the same");
        }
    }
}
