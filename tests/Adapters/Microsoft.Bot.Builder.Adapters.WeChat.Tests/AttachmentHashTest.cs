// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
{
    public class AttachmentHashTest
    {
        [Fact]
        public void Md5HashTest()
        {
            var md5Hash = new AttachmentHash();
            var testString = "test string to get hash";
            var stringHashed = md5Hash.ComputeHash(testString);
            var bytesHashed = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(testString));
            Assert.Equal("E16B52D76AD74BB8D4B507515CD9ADB8", stringHashed);
            Assert.Equal(bytesHashed, stringHashed);
        }
    }
}
