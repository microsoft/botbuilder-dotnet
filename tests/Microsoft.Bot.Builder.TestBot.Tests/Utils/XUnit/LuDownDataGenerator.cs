// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.BotBuilderSamples.Tests.Utils.Luis;

namespace Microsoft.BotBuilderSamples.Tests.Utils.XUnit
{
    /// <summary>
    /// A helper class to generate theory data from a lu down file.
    /// </summary>
    public class LuDownDataGenerator : IEnumerable<object[]>
    {
        private readonly List<object[]> _data;

        public LuDownDataGenerator(string fileName, string relativePath)
        {
            _data = new List<object[]>();
            var absolutePath = string.IsNullOrEmpty(relativePath) ? @"C:\Projects\Repos\botbuilder-dotnet\tests\Microsoft.Bot.Builder.TestBot.Tests\bin\Debug\netcoreapp2.1\CognitiveModels\Data\" : Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            var batchTest = LuisCommandRunner.LuToBatchTest(fileName, absolutePath);
            foreach (var testUtterance in batchTest)
            {
                _data.Add(new object[] { new TestDataObject(testUtterance) });
            }
        }

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
