// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.BotBuilderSamples.Tests.Utils.Luis;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Utils.XUnit
{
    /// <summary>
    /// A helper class to generate theory data from a lu down file.
    /// </summary>
    public class LuDownDataGenerator : TheoryData<BatchTestItem>
    {
        public LuDownDataGenerator(string fileName, string relativePath)
        {
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            var currentPath = Directory.GetCurrentDirectory();
            var batchTest = LuisCommandRunner.LuToBatchTest(fileName, absolutePath);
            foreach (var testUtterance in batchTest)
            {
                Add(testUtterance);
            }
        }
    }
}
