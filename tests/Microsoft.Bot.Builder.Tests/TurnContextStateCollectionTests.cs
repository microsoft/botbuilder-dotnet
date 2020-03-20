// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class TurnContextStateCollectionTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TurnContextStateCollection_AddNullKey()
        {
            var ts = new TurnContextStateCollection();
            ts.Add(null, new object());
            Assert.Fail("Should Fail due to null key");
        }

        [TestMethod]
        public void TurnContextStateCollection_AddRemove()
        {
            var ts = new TurnContextStateCollection();
            var test = new object();
            var test2 = new object();

            ts.Add("test", test);
            ts.Add(test2);
            Assert.AreEqual(test, ts.Get<object>("test"));
            Assert.AreEqual(test2, ts.Get<object>());
            Assert.AreNotEqual(test, ts.Get<object>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TurnContextStateCollection_SetNullKey()
        {
            var ts = new TurnContextStateCollection();
            ts.Set(null, new object());
            Assert.Fail("Should Fail due to null key");
        }
        
        [TestMethod]
        public void TurnContextStateCollection_Set()
        {
            var ts = new TurnContextStateCollection();
            var test = new object();
            var test2 = new object();

            ts.Set("test", test);
            ts.Set(test2);
            Assert.AreEqual(test, ts.Get<object>("test"));
            Assert.AreEqual(test2, ts.Get<object>());
            Assert.AreNotEqual(test, ts.Get<object>());

            ts.Set<object>("test", null);
            Assert.IsNull(ts.Get<object>("test"));
            Assert.AreEqual(test2, ts.Get<object>());
            ts.Set<object>(null);
            Assert.IsNull(ts.Get<object>());
        }

        [TestMethod]
        public void TurnContextStateCollection_PopPushKey()
        {
            var ts = new TurnContextStateCollection();
            var test1 = new object();
            var test2 = new object();
            var test3 = new object();

            var key = "test";
            
            Assert.IsNull(ts.Pop<object>(key), "pop with no pushes is null");

            ts.Push(key, test1);
            ts.Push(key, test2);
            ts.Push(key, test3);

            // test3 should be current object
            Assert.AreNotEqual(test1, ts.Get<object>(key), "test3 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(key), "test3 should be current");
            Assert.AreEqual(test3, ts.Get<object>(key), "test3 should be current");

            Assert.AreEqual(test2, ts.Pop<object>(key), "pop should return test2");

            Assert.AreNotEqual(test1, ts.Get<object>(key), "test2 should be current");
            Assert.AreEqual(test2, ts.Get<object>(key), "test2 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(key), "test2 should be current");

            Assert.AreEqual(test1, ts.Pop<object>(key), "pop should return test1");

            Assert.AreEqual(test1, ts.Get<object>(key), "test1 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(key), "test1 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(key), "test1 should be current");

            Assert.AreEqual(null, ts.Pop<object>(key), "pop should return null");

            Assert.IsNull(ts.Get<object>(key), "null should be current");
            
            Assert.AreEqual(null, ts.Pop<object>(key), "pop with nothing should be null");
            Assert.IsNull(ts.Get<object>(key), "null should be current");
        }

        [TestMethod]
        public void TurnContextStateCollection_PopPush()
        {
            var ts = new TurnContextStateCollection();
            var test1 = new object();
            var test2 = new object();
            var test3 = new object();

            Assert.IsNull(ts.Pop<object>(), "pop with no pushes is null");

            ts.Push(test1);
            ts.Push(test2);
            ts.Push(test3);

            // test3 should be current object
            Assert.AreNotEqual(test1, ts.Get<object>(), "test3 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(), "test3 should be current");
            Assert.AreEqual(test3, ts.Get<object>(), "test3 should be current");

            Assert.AreEqual(test2, ts.Pop<object>(), "pop should return test2");

            Assert.AreNotEqual(test1, ts.Get<object>(), "test2 should be current");
            Assert.AreEqual(test2, ts.Get<object>(), "test2 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(), "test2 should be current");

            Assert.AreEqual(test1, ts.Pop<object>(), "pop should return test1");

            Assert.AreEqual(test1, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(), "test1 should be current");

            Assert.AreEqual(null, ts.Pop<object>(), "pop should return null");

            Assert.IsNull(ts.Get<object>(), "null should be current");

            Assert.AreEqual(null, ts.Pop<object>(), "pop with nothing should be null");
            Assert.IsNull(ts.Get<object>(), "null should be current");
        }

        [TestMethod]
        public void TurnContextStateCollection_PopPushSetFirst()
        {
            var ts = new TurnContextStateCollection();
            var test1 = new object();
            var test2 = new object();
            var test3 = new object();

            Assert.IsNull(ts.Pop<object>(), "pop with no pushes is null");

            ts.Set(test1);
            Assert.AreEqual(test1, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(), "test1 should  be current");
            
            ts.Push(test2);
            ts.Push(test3);

            // test3 should be current object
            Assert.AreNotEqual(test1, ts.Get<object>(), "test3 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(), "test3 should be current");
            Assert.AreEqual(test3, ts.Get<object>(), "test3 should be current");

            Assert.AreEqual(test2, ts.Pop<object>(), "pop should return test2");

            Assert.AreNotEqual(test1, ts.Get<object>(), "test2 should be current");
            Assert.AreEqual(test2, ts.Get<object>(), "test2 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(), "test2 should be current");

            Assert.AreEqual(test1, ts.Pop<object>(), "pop should return test1");

            Assert.AreEqual(test1, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(), "test1 should be current");

            Assert.AreEqual(test1, ts.Pop<object>(), "pop should return test1");

            Assert.AreEqual(test1, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(), "test1 should  be current");

            Assert.AreEqual(test1, ts.Pop<object>(), "pop should return test1");

            Assert.AreEqual(test1, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test2, ts.Get<object>(), "test1 should be current");
            Assert.AreNotEqual(test3, ts.Get<object>(), "test1 should  be current");
        }
    }
}
