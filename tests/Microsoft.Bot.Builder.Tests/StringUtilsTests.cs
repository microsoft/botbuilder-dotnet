// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class StringUtilsTests
    {
        private string _test1 = "asdlfkjasdflkjasdlfkjasldkfjasdf";
        private string _test2 = "alskjdf lksjfd laksjdf lksjdfasdlfkjasdflkjasdlfkjasldkfjasdf";

        [Fact]
        public void TestHash()
        {
            var hash1 = StringUtils.Hash(_test1);
            var hash2 = StringUtils.Hash(_test1);
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEqual(_test1, hash1);
            Assert.Equal(hash1, hash2);

            hash1 = StringUtils.Hash(_test1);
            hash2 = StringUtils.Hash(_test2);
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEqual(_test1, hash1);
            Assert.NotEqual(_test2, hash2);
            Assert.NotEqual(hash1, hash2);

            hash1 = StringUtils.Hash(_test1.ToUpper());
            hash2 = StringUtils.Hash(_test1);
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void TestEllipsis()
        {
            Assert.Equal("...", StringUtils.Ellipsis(_test1, 0));
            Assert.Equal($"{_test1.Substring(0, 5)}...", StringUtils.Ellipsis(_test1, 5));
            Assert.Equal(_test1, StringUtils.Ellipsis(_test1, 1000));
        }

        [Fact]
        public void TestEllipsisHash()
        {
            Assert.Equal($"...{StringUtils.Hash(_test1)}", StringUtils.EllipsisHash(_test1, 0));
            Assert.Equal($"{_test1.Substring(0, 5)}...{StringUtils.Hash(_test1)}", StringUtils.EllipsisHash(_test1, 5));
            Assert.Equal(_test1, StringUtils.EllipsisHash(_test1, 1000));
        }

        [Fact]
        public void TestEllipsisStringBuilder()
        {
            Assert.Equal("...", StringUtils.Ellipsis(new StringBuilder(_test1), 0).ToString());

            Assert.Equal($"{_test1.Substring(0, 5)}...", StringUtils.Ellipsis(new StringBuilder(_test1), 5).ToString());
            Assert.Equal(_test1, StringUtils.Ellipsis(new StringBuilder(_test1), 1000).ToString());
        }

        [Fact]
        public void TestEllipsisHashStringBuilder()
        {
            Assert.Equal($"...{StringUtils.Hash(_test1)}", StringUtils.EllipsisHash(new StringBuilder(_test1), 0).ToString());
            Assert.Equal($"{_test1.Substring(0, 5)}...{StringUtils.Hash(_test1)}", StringUtils.EllipsisHash(new StringBuilder(_test1), 5).ToString());
            Assert.Equal(_test1, StringUtils.EllipsisHash(new StringBuilder(_test1), 1000).ToString());
        }
    }
}
