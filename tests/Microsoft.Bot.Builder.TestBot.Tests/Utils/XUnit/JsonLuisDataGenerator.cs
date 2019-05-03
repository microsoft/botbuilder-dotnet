// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.BotBuilderSamples.Tests.Utils.Luis;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Utils.XUnit
{
    public class JsonLuisDataGenerator : TheoryData<LuisTestItem>
    {
        public JsonLuisDataGenerator(string fileName, string relativePath)
        {
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            var batchTest = JsonConvert.DeserializeObject<LuisTestItem[]>(File.ReadAllText(Path.Combine(absolutePath, fileName)));
            foreach (var testUtterance in batchTest)
            {
                Add(testUtterance);
            }
        }
    }
}
