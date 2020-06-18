// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class TurnContextStateCollectionTests
    {
        [Fact]
        public void TurnContextStateCollection_AddNullKey()
        {
            var ts = new TurnContextStateCollection();
            Assert.Throws<ArgumentNullException>(() => ts.Add(null, new object()));
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
            Assert.NotEqual(test, ts.Get<object>());
        }

        [Fact]
        public void TurnContextStateCollection_SetNullKey()
        {
            var ts = new TurnContextStateCollection();
            Assert.Throws<ArgumentNullException>(() => ts.Set(null, new object()));
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
            Assert.NotEqual(test, ts.Get<object>());

            ts.Set<object>("test", null);
            Assert.Null(ts.Get<object>("test"));
            Assert.Equal(test2, ts.Get<object>());
            ts.Set<object>(null);
            Assert.Null(ts.Get<object>());
        }
    }
}
