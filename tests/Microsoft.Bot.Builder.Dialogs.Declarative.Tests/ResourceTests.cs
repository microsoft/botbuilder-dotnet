// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Tests.TestComponents;
using Xunit;
using Xunit.Sdk;
using static Microsoft.Bot.Builder.Dialogs.Declarative.Tests.ResourceExplorerOptionsTests;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    public class ResourceTests : IClassFixture<DeleteTestResourceFixture>
    {
        public static IEnumerable<object[]> ResourceExplorerRegistrationTestData()
        {
            var testProviders = new ResourceProvider[] { new TestResourceProvider() };
            var testDeclarativeTypes = new DeclarativeType[] { new DeclarativeType<ResourceExplorerOptionsTests>("test") };
            var testConverterFactories = new JsonConverterFactory[] { new JsonConverterFactory<TestDeclarativeConverter>() };
            var testLegacyComponentTypes = new IComponentDeclarativeTypes[] { new TestDeclarativeComponentRegistration() };

            // params: ResourceExplorerOptions options
            
            // Initial declarative types only
            yield return new object[] { new ResourceExplorerOptions(null, testDeclarativeTypes, testConverterFactories) { DeclarativeTypes = null }, null };

            // Initial IComponentDeclarativeTypes only
            yield return new object[] { new ResourceExplorerOptions(null, null, null) { DeclarativeTypes = testLegacyComponentTypes }, null };

            // Initial declarative types and IComponentDeclarativeTypes
            yield return new object[] { new ResourceExplorerOptions(null, testDeclarativeTypes, testConverterFactories) { DeclarativeTypes = testLegacyComponentTypes }, null };
            
            // Legacy registration only
            yield return new object[] { new ResourceExplorerOptions(null, null, null) { DeclarativeTypes = null }, new TestDeclarativeComponentRegistration() };

            // Legacy bridged registration only
            yield return new object[] { new ResourceExplorerOptions(null, null, null) { DeclarativeTypes = null }, new LegacyTestComponentRegistration() };

            // All at once, should to union of all registrations
            yield return new object[] { new ResourceExplorerOptions(null, testDeclarativeTypes, testConverterFactories) { DeclarativeTypes = testLegacyComponentTypes }, new LegacyTestComponentRegistration() };
        }

        [Theory]
        [MemberData(nameof(ResourceExplorerRegistrationTestData))]
        public void TestResourceExplorerRegistration(ResourceExplorerOptions options, ComponentRegistration legacyRegistration)
        {
            // Arrange
            // Build resourceExplorer
            using (var explorer = new ResourceExplorer(options))
            {
                // Clear component registration
                if (legacyRegistration != null)
                {
                    ComponentRegistration.Add(legacyRegistration);
                }

                // Test
                var declarativeType = explorer.LoadType<TestDeclarativeType>(new MemoryResource());

                // Assert
                Assert.NotNull(declarativeType);
                Assert.Equal("fromConverter", declarativeType.Data);
            }
        }

        [Fact]
        public void TestFolderSource()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                var resources = explorer.GetResources(".dialog").ToArray();

                Assert.Equal(4, resources.Length);
                Assert.Equal($".dialog", Path.GetExtension(resources[0].Id));

                resources = explorer.GetResources("foo").ToArray();
                Assert.Empty(resources);
            }
        }

        [Fact]
        public void TestMissingResourceThrows()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));
                try
                {
                    explorer.GetResource("bogus.dialog");
                    throw new XunitException("should have thrown exception");
                }
                catch (ArgumentException err)
                {
                    Assert.Contains("bogus", err.Message);
                    Assert.Equal("bogus.dialog", err.ParamName);
                }
                catch (Exception err2)
                {
                    throw new XunitException($"Unknown exception {err2.GetType().Name} thrown");
                }
            }
        }

        [Fact]
        public void TestResourceDialogIdAssignment()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));
                var dlg1 = explorer.LoadType<Dialog>("test.dialog") as AdaptiveDialog;
                Assert.Equal("test.dialog", dlg1.Id);

                Assert.Equal("1234567890", dlg1.Triggers[0].Actions[0].Id);
                Assert.Equal("test3.dialog", dlg1.Triggers[0].Actions[1].Id);

                var dlg2 = explorer.LoadType<Dialog>("test2.dialog");
                Assert.Equal("1234567890", dlg2.Id);
            }
        }

        [Fact]
        public void TestFolderSource_Shallow()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddFolder(path, false);

                var resources = explorer.GetResources("dialog").ToArray();
                Assert.Empty(resources);

                resources = explorer.GetResources("schema").ToArray();
                Assert.True(resources.Length > 0, "shallow folder should list the root files");
            }
        }

        [Fact]
        public async Task TestFolderSource_NewFiresChanged()
        {
            const string testId = "NewFiresChanged.dialog";
            var testDialogFile = Path.Combine(Environment.CurrentDirectory, testId);

            File.Delete(testDialogFile);

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddFolder(path, monitorChanges: true);

                AssertResourceNull(explorer, testId);

                var changeFired = new TaskCompletionSource<bool>();

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

        [Fact]
        public async Task TestFolderSource_WriteFiresChanged()
        {
            const string testId = "WriteFiresChanged.dialog";
            var testDialogFile = Path.Combine(Environment.CurrentDirectory, testId);

            File.Delete(testDialogFile);
            var contents = "{}";
            File.WriteAllText(testDialogFile, contents);

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path, true));

                AssertResourceFound(explorer, testId);

                await AssertResourceContents(explorer, testId, contents);

                var changeFired = new TaskCompletionSource<bool>();

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

        [Fact]
        public async Task TestFolderSource_DeleteFiresChanged()
        {
            const string testId = "DeleteFiresChanged.dialog";
            var testDialogFile = Path.Combine(Environment.CurrentDirectory, testId);

            File.Delete(testDialogFile);
            File.WriteAllText(testDialogFile, "{}");

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path, true));

                AssertResourceFound(explorer, testId);

                var changeFired = new TaskCompletionSource<bool>();

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

        [Fact]
        public async Task ResourceExplorer_ReadTokenRange_AssignId()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            var sourceContext = new ResourceSourceContext();
            const string resourcesFolder = "resources";
            const string resourceId = "test.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);

                // Read token range using resource explorer
                var (jToken, range) = await explorer.ReadTokenRangeAsync(resource, sourceContext).ConfigureAwait(false);

                // Verify correct range
                var expectedRange = new SourceRange
                {
                    StartPoint = new SourcePoint(0, 0),
                    EndPoint = new SourcePoint(14, 1),
                    Path = Path.Join(Path.Join(path, resourcesFolder), resourceId)
                };

                Assert.Equal(expectedRange, range);

                // Verify ID was added
                Assert.Equal(resourceId, sourceContext.DefaultIdMap[jToken]);
            }
        }

        [Fact]
        public async Task ResourceExplorer_ReadTokenRangeAdvance_AssignId()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            var sourceContext = new ResourceSourceContext();
            const string resourcesFolder = "resources";
            const string resourceId = "test.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);

                // Read token range using resource explorer
                var (jToken, range) = await explorer.ReadTokenRangeAsync(resource, sourceContext, true).ConfigureAwait(false);

                // Verify correct range
                var expectedRange = new SourceRange
                {
                    StartPoint = new SourcePoint(1, 1),
                    EndPoint = new SourcePoint(14, 1),
                    Path = Path.Join(Path.Join(path, resourcesFolder), resourceId)
                };

                Assert.Equal(expectedRange, range);

                // Verify ID was added
                Assert.Equal(resourceId, sourceContext.DefaultIdMap[jToken]);
            }
        }

        [Fact]
        public async Task ResourceExplorer_LoadType_VerifyTokenRangeAndIdAssigned()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            const string resourcesFolder = "resources";
            const string resourceId = "test.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);
                var dialog = await explorer.LoadTypeAsync<Dialog>(resource).ConfigureAwait(false);

                // Verify correct range
                var expectedRange = new SourceRange
                {
                    StartPoint = new SourcePoint(1, 1),
                    EndPoint = new SourcePoint(14, 1),
                    Path = Path.Join(Path.Join(path, resourcesFolder), resourceId)
                };

                Assert.Equal(expectedRange, dialog.Source);

                // Verify that the correct id was assigned
                Assert.Equal(resourceId, dialog.Id);
            }
        }

        [Fact]
        public async Task ResourceExplorer_LoadType_VerifyTokenRangeAndIdHonored()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            const string resourcesFolder = "resources";
            const string resourceId = "testWithId.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);
                var dialog = await explorer.LoadTypeAsync<Dialog>(resource).ConfigureAwait(false);

                // Verify correct range
                var expectedRange = new SourceRange
                {
                    StartPoint = new SourcePoint(1, 1),
                    EndPoint = new SourcePoint(14, 1),
                    Path = Path.Join(Path.Join(path, resourcesFolder), resourceId)
                };

                Assert.Equal(expectedRange, dialog.Source);

                // Verify that the correct id was set
                Assert.Equal("explicit-id", dialog.Id);
            }
        }

        [Fact]
        public async Task ResourceExplorer_LoadType_VerifyAdaptiveDialogIdAssigned()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            const string resourceId = "test.dialog";

            using (var explorer = new ResourceExplorer())
            {
                explorer.AddResourceProvider(new FolderResourceProvider(explorer, path));

                // Load file using resource explorer
                var resource = explorer.GetResource(resourceId);
                var dialog = await explorer.LoadTypeAsync<AdaptiveDialog>(resource).ConfigureAwait(false);

                // Verify that the correct id was assigned
                Assert.Equal(resourceId, dialog.Id);
            }
        }

        private static void AssertResourceFound(ResourceExplorer explorer, string id)
        {
            var dialog = explorer.GetResource(id);
            Assert.NotNull(dialog);
            var dialogs = explorer.GetResources("dialog");
            Assert.True(dialogs.Any(d => d.Id == id), $"getResources({id}) should return resource");
        }

        private static void AssertResourceNull(ResourceExplorer explorer, string id)
        {
            try
            {
                explorer.GetResource(id);
                throw new XunitException($"GetResource({id}) should throw");
            }
            catch (ArgumentException err)
            {
                Assert.Equal(err.ParamName, id);
            }

            var dialogs = explorer.GetResources("dialog");
            Assert.False(dialogs.Any(d => d.Id == id), $"getResources({id}) should not return resource");
        }

        private async Task AssertResourceContents(ResourceExplorer explorer, string id, string contents)
        {
            var resource = explorer.GetResource(id);

            var text = await resource.ReadTextAsync();
            Assert.Equal(contents, text);
            resource = explorer.GetResources("dialog").Single(d => d.Id == id);

            text = await resource.ReadTextAsync();
            Assert.Equal(contents, text);
        }

        private class MemoryResource : Resource
        {
            public override Task<Stream> OpenStreamAsync()
            {
                byte[] byteArray = Encoding.UTF8.GetBytes("{}");
                return Task.FromResult<Stream>(new MemoryStream(byteArray));
            }
        }
    }
}
