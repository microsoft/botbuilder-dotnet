// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class StringUtilsTests
    {
        private string test1 = "asdlfkjasdflkjasdlfkjasldkfjasdf";
        private string test2 = "alskjdf lksjfd laksjdf lksjdfasdlfkjasdflkjasdlfkjasldkfjasdf";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestHash()
        {
            var hash1 = StringUtils.Hash(test1);
            var hash2 = StringUtils.Hash(test1);
            Assert.IsNotNull(hash1);
            Assert.IsNotNull(hash2);
            Assert.AreNotEqual(test1, hash1, "hash should be different");
            Assert.AreEqual(hash1, hash2, "same string should be same");

            hash1 = StringUtils.Hash(test1);
            hash2 = StringUtils.Hash(test2);
            Assert.IsNotNull(hash1);
            Assert.IsNotNull(hash2);
            Assert.AreNotEqual(test1, hash1, "hash should be different");
            Assert.AreNotEqual(test2, hash2, "hash should be different");
            Assert.AreNotEqual(hash1, hash2, "different string should be different");

            hash1 = StringUtils.Hash(test1.ToUpper());
            hash2 = StringUtils.Hash(test1);
            Assert.IsNotNull(hash1);
            Assert.IsNotNull(hash2);
            Assert.AreNotEqual(hash1, hash2, "case changes string should be different");
        }

        [TestMethod]
        public void TestEllipsis()
        {
            Assert.AreEqual("...", StringUtils.Ellipsis(test1, 0));
            Assert.AreEqual($"{test1.Substring(0, 5)}...", StringUtils.Ellipsis(test1, 5));
            Assert.AreEqual(test1, StringUtils.Ellipsis(test1, 1000));
        }

        [TestMethod]
        public void TestEllipsisHash()
        {
            Assert.AreEqual($"...{StringUtils.Hash(test1)}", StringUtils.EllipsisHash(test1, 0));
            Assert.AreEqual($"{test1.Substring(0, 5)}...{StringUtils.Hash(test1)}", StringUtils.EllipsisHash(test1, 5));
            Assert.AreEqual(test1, StringUtils.EllipsisHash(test1, 1000));
        }

        [TestMethod]
        public void TestEllipsisStringBuilder()
        {
            Assert.AreEqual("...", StringUtils.Ellipsis(new StringBuilder(test1), 0).ToString());
           
            Assert.AreEqual($"{test1.Substring(0, 5)}...", StringUtils.Ellipsis(new StringBuilder(test1), 5).ToString());
            Assert.AreEqual(test1, StringUtils.Ellipsis(new StringBuilder(test1), 1000).ToString());
        }

        [TestMethod]
        public void TestEllipsisHashStringBuilder()
        {
            Assert.AreEqual($"...{StringUtils.Hash(test1)}", StringUtils.EllipsisHash(new StringBuilder(test1), 0).ToString());
            Assert.AreEqual($"{test1.Substring(0, 5)}...{StringUtils.Hash(test1)}", StringUtils.EllipsisHash(new StringBuilder(test1), 5).ToString());
            Assert.AreEqual(test1, StringUtils.EllipsisHash(new StringBuilder(test1), 1000).ToString());
        }
    }
}
