// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.BotBuilderSamples.Tests.Utils.Luis;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Tests.Utils
{
    /// <summary>
    /// A set of helper methods that wrap ludown and LUIS calls
    /// </summary>
    public static class LuisCommandRunner
    {
        public static void LuToLuisJson(string sourceLuFile, string sourcePath)
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                var targetPath = EnsureTargetPath(sourcePath);
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

        public static BatchTestItem[] LuToBatchTest(string sourceLuFile, string sourcePath)
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                var targetPath = EnsureTargetPath(sourcePath);
                var sourceFile = Path.Combine(sourcePath, sourceLuFile);
                ps.AddScript($"ludown parse toluis --write_luis_batch_tests --in {sourceFile} -o {targetPath}");
                var r = ps.InvokeAsync().Result;
                if (r.Count > 0)
                {
                    // TODO: improve exception management
                    throw new InvalidOperationException();
                }

                var batchFilePrefix = Path.GetFileNameWithoutExtension(sourceFile);
                var batchTest = JsonConvert.DeserializeObject<BatchTestItem[]>(File.ReadAllText($"{targetPath}\\{batchFilePrefix}_LUISBatchTest.json"));
                return batchTest;
            }
        }

        private static string EnsureTargetPath(string sourcePath)
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
