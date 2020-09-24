// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.Builder.Parsers.LU.Parser;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Parsers.LU.Tests.Parser
{
    public class LuParserTests
    {
        [Fact]
        public void ParseLuContent()
        {
            // var luContent = "# Help"+ Environment.NewLine + "- help" + Environment.NewLine + "- I need help" + Environment.NewLine + "- please help";

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Fixtures", "ImportAllLu.txt");
            Console.WriteLine(path);

            var luContent = File.ReadAllText(path);
            luContent = luContent.Substring(0, luContent.Length - 1);
            var result = LuParser.Parse(luContent);
            LuResource expected = JsonConvert.DeserializeObject<LuResource>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Fixtures", "ImportAllLu.json")));

            var serializedResult = JsonConvert.SerializeObject(result).Replace("\\r", string.Empty);
            var serializedExpected = JsonConvert.SerializeObject(expected).Replace("\\r", string.Empty);

            Assert.Equal(serializedResult, serializedExpected);
        }
    }
}
