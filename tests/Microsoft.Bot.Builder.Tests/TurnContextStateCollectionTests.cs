// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class TurnContextStateCollectionTests
    {
        [Fact]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TurnContextStateCollection_AddNullKey()
        {
            var ts = new TurnContextStateCollection();
            ts.Add(null, new object());
            Assert.Fail("Should Fail due to null key");
        }

        [Fact]
        public void TurnContextStateCollection_AddRemove()
        {
            var ts = new TurnContextStateCollection();
            var test = new object();
            var test2 = new object();

            ts.Add("test", test);
            ts.Add(test2);
            Assert.Equal(test, ts.Get<object>("test"));
            Assert.Equal(test2, ts.Get<object>());
            Assert.AreNotEqual(test, ts.Get<object>());
        }

        [Fact]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TurnContextStateCollection_SetNullKey()
        {
            var ts = new TurnContextStateCollection();
            ts.Set(null, new object());
            Assert.Fail("Should Fail due to null key");
        }
        
        [Fact]
        public void TurnContextStateCollection_Set()
        {
            var ts = new TurnContextStateCollection();
            var test = new object();
            var test2 = new object();

            ts.Set("test", test);
            ts.Set(test2);
            Assert.Equal(test, ts.Get<object>("test"));
            Assert.Equal(test2, ts.Get<object>());
            Assert.AreNotEqual(test, ts.Get<object>());

            ts.Set<object>("test", null);
            Assert.Null(ts.Get<object>("test"));
            Assert.Equal(test2, ts.Get<object>());
            ts.Set<object>(null);
            Assert.Null(ts.Get<object>());
        }
    }
}
