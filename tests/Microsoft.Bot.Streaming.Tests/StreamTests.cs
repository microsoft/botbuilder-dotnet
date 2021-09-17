// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class StreamTests
    {
        [Fact]

        public void StreamExtensions_ReadAsUtf8String()
        {
            const string stringInput = "test";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(stringInput));

            var stringUtf8 = stream.ReadAsUtf8String();

            Assert.Equal(stringInput, stringUtf8);
        }

        [Fact]

        public async Task StreamExtensions_ReadAsUtf8StringAsync()
        {
            const string stringInput = "test";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(stringInput));

            var stringUtf8 = await stream.ReadAsUtf8StringAsync();

            Assert.Equal(stringInput, stringUtf8);
        }
    }
}
