// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class StringUtilsTests
    {
        private string test1 = "asdlfkjasdflkjasdlfkjasldkfjasdf";
        private string test2 = "alskjdf lksjfd laksjdf lksjdfasdlfkjasdflkjasdlfkjasldkfjasdf";

        [Fact]
        public void TestHash()
        {
            var hash1 = StringUtils.Hash(test1);
            var hash2 = StringUtils.Hash(test1);
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEqual(test1, hash1);
            Assert.Equal(hash1, hash2);

            hash1 = StringUtils.Hash(test1);
            hash2 = StringUtils.Hash(test2);
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEqual(test1, hash1);
            Assert.NotEqual(test2, hash2);
            Assert.NotEqual(hash1, hash2);

            hash1 = StringUtils.Hash(test1.ToUpper());
            hash2 = StringUtils.Hash(test1);
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void TestEllipsis()
        {
            Assert.Equal("...", StringUtils.Ellipsis(test1, 0));
            Assert.Equal($"{test1.Substring(0, 5)}...", StringUtils.Ellipsis(test1, 5));
            Assert.Equal(test1, StringUtils.Ellipsis(test1, 1000));
        }

        [Fact]
        public void TestEllipsisHash()
        {
            Assert.Equal($"...{StringUtils.Hash(test1)}", StringUtils.EllipsisHash(test1, 0));
            Assert.Equal($"{test1.Substring(0, 5)}...{StringUtils.Hash(test1)}", StringUtils.EllipsisHash(test1, 5));
            Assert.Equal(test1, StringUtils.EllipsisHash(test1, 1000));
        }

        [Fact]
        public void TestEllipsisStringBuilder()
        {
            Assert.Equal("...", StringUtils.Ellipsis(new StringBuilder(test1), 0).ToString());
           
            Assert.Equal($"{test1.Substring(0, 5)}...", StringUtils.Ellipsis(new StringBuilder(test1), 5).ToString());
            Assert.Equal(test1, StringUtils.Ellipsis(new StringBuilder(test1), 1000).ToString());
        }

        [Fact]
        public void TestEllipsisHashStringBuilder()
        {
            Assert.Equal($"...{StringUtils.Hash(test1)}", StringUtils.EllipsisHash(new StringBuilder(test1), 0).ToString());
            Assert.Equal($"{test1.Substring(0, 5)}...{StringUtils.Hash(test1)}", StringUtils.EllipsisHash(new StringBuilder(test1), 5).ToString());
            Assert.Equal(test1, StringUtils.EllipsisHash(new StringBuilder(test1), 1000).ToString());
        }
    }
}
