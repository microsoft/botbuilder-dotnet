// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Tests.Framework.Luis
{
    /// <summary>
    /// A set of helper methods that wraps ludown and luis command calls.
    /// </summary>
    public static class LuisCommandRunner
    {
        public static void LuToLuisJson(string sourceLuFile, string sourcePath)
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                var targetPath = EnsureTargetFolder(sourcePath);
                var sourceFile = Path.Combine(sourcePath, sourceLuFile);
                ps.AddScript($"ludown parse toluis --in {sourceFile} -o {targetPath}");
                var r = ps.InvokeAsync().Result;
                if (r.Count > 0)
                {
                    // TODO: improve exception management
                    throw new InvalidOperationException();
                }
            }
        }

        public static LuisTestItem[] LuToBatchTest(string sourceLuFile, string sourcePath)
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                var targetFolder = EnsureTargetFolder(sourcePath);
                var sourceFile = Path.Combine(sourcePath, sourceLuFile);
                ps.AddScript($"ludown parse toluis --write_luis_batch_tests --in {sourceFile} -o {targetFolder}");
                var r = ps.InvokeAsync().Result;
                if (r.Count > 0)
                {
                    // TODO: improve exception management
                    throw new InvalidOperationException();
                }

                var batchFilePrefix = Path.GetFileNameWithoutExtension(sourceFile);
                var batchTest = JsonConvert.DeserializeObject<LuisTestItem[]>(File.ReadAllText($"{targetFolder}\\{batchFilePrefix}_LUISBatchTest.json"));
                return batchTest;
            }
        }

        private static string EnsureTargetFolder(string sourcePath)
        {
            var targetPath = Path.Combine(sourcePath, "Temp");
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            return targetPath;
        }
    }
}
