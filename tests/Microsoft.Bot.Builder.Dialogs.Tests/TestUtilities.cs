// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class TestUtilities
    {
        private static string rootFolder = PathUtils.NormalizePath(@"..\..\..");

        public static IEnumerable<object[]> GetTestScripts(string relativeFolder)
        {
            string testFolder = Path.GetFullPath(Path.Combine(rootFolder, PathUtils.NormalizePath(relativeFolder)));
            return Directory.EnumerateFiles(testFolder, "*.test.dialog", SearchOption.AllDirectories).Select(s => new object[] { Path.GetFileName(s) }).ToArray();
        }

        public static async Task RunTestScript(string resourceId = null, [CallerMemberName] string testName = null, IConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            }

            var resourceExplorer = new ResourceExplorer().AddFolder(rootFolder, monitorChanges: false);
            var script = resourceExplorer.LoadType<TestScript>(resourceId ?? $"{testName}.test.dialog");
            script.Configuration = configuration;
            script.Description = script.Description ?? resourceId;
            await script.ExecuteAsync(testName: testName, resourceExplorer: resourceExplorer).ConfigureAwait(false);
        }
    }
}
