// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugging;
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

                var resources = explorer.GetResources(".dialog").ToArray();

                Assert.AreEqual(4, resources.Length);
                Assert.AreEqual($".dialog", Path.GetExtension(resources[0].Id));

                resources = explorer.GetResources("foo").ToArray();
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
        public void TestResourceDialogIdAssignment()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));
                var dlg1 = explorer.LoadType<Dialog>("test.dialog") as AdaptiveDialog;
                Assert.AreEqual("test.dialog", dlg1.Id, "resource .id should be used as default dialog.id if none assigned");

                Assert.AreEqual("1234567890", dlg1.Triggers[0].Actions[0].Id);
                Assert.AreEqual("test3.dialog", dlg1.Triggers[0].Actions[1].Id);

                var dlg2 = explorer.LoadType<Dialog>("test2.dialog");
                Assert.AreEqual("1234567890", dlg2.Id, "id in the .dialog file should be honored");
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

        [TestMethod]
        public async Task ResourceExplorer_ReadTokenRange_AssignId()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            var sourceContext = new ResourceSourceContext();
            var resourcesFolder = "resources";
            var resourceId = "test.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);

                // Read token range using resource explorer
                var (jToken, range) = await explorer.ReadTokenRangeAsync(resource, sourceContext).ConfigureAwait(false);

                // Verify correct range
                var expectedRange = new SourceRange()
                {
                    StartPoint = new SourcePoint(0, 0),
                    EndPoint = new SourcePoint(13, 1),
                    Path = Path.Join(Path.Join(path, resourcesFolder), resourceId)
                };

                Assert.AreEqual(expectedRange, range);

                // Verify ID was added
                Assert.AreEqual(resourceId, sourceContext.DefaultIdMap[jToken]);
            }
        }

        [TestMethod]
        public async Task ResourceExplorer_ReadTokenRangeAdvance_AssignId()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            var sourceContext = new ResourceSourceContext();
            var resourcesFolder = "resources";
            var resourceId = "test.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);

                // Read token range using resource explorer
                var (jToken, range) = await explorer.ReadTokenRangeAsync(resource, sourceContext, advanceJsonReader: true).ConfigureAwait(false);

                // Verify correct range
                var expectedRange = new SourceRange()
                {
                    StartPoint = new SourcePoint(1, 1),
                    EndPoint = new SourcePoint(13, 1),
                    Path = Path.Join(Path.Join(path, resourcesFolder), resourceId)
                };

                Assert.AreEqual(expectedRange, range);

                // Verify ID was added
                Assert.AreEqual(resourceId, sourceContext.DefaultIdMap[jToken]);
            }
        }

        [TestMethod]
        public async Task ResourceExplorer_LoadType_VerifyTokenRangeAndIdAssigned()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            var resourcesFolder = "resources";
            var resourceId = "test.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);
                var dialog = await explorer.LoadTypeAsync<Dialog>(resource).ConfigureAwait(false);

                // Verify correct range
                var expectedRange = new SourceRange()
                {
                    StartPoint = new SourcePoint(1, 1),
                    EndPoint = new SourcePoint(13, 1),
                    Path = Path.Join(Path.Join(path, resourcesFolder), resourceId)
                };

                Assert.AreEqual(expectedRange, dialog.Source);

                // Verify that the correct id was assigned
                Assert.AreEqual(resourceId, dialog.Id);
            }
        }

        [TestMethod]
        public async Task ResourceExplorer_LoadType_VerifyTokenRangeAndIdHonored()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            var resourcesFolder = "resources";
            var resourceId = "testWithId.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);
                var dialog = await explorer.LoadTypeAsync<Dialog>(resource).ConfigureAwait(false);

                // Verify correct range
                var expectedRange = new SourceRange()
                {
                    StartPoint = new SourcePoint(1, 1),
                    EndPoint = new SourcePoint(14, 1),
                    Path = Path.Join(Path.Join(path, resourcesFolder), resourceId)
                };

                Assert.AreEqual(expectedRange, dialog.Source);

                // Verify that the correct id was set
                Assert.AreEqual("explicit-id", dialog.Id);
            }
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
