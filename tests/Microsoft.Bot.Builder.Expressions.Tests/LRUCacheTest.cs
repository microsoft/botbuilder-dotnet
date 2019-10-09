using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Expressions.Tests
{
    [TestClass]
    public class LRUCacheTest
    {
        [TestMethod]
        public void TestBasic()
        {
            var cache = new LRUCache<int, string>(2);
            
            Assert.IsFalse(cache.TryGet(1, out var result));

            cache.Set(1, "num1");

            Assert.IsTrue(cache.TryGet(1, out result));
            Assert.AreEqual(result, "num1");
        }

        [TestMethod]
        public void TestDiacardPolicy()
        {
            var cache = new LRUCache<int, string>(2);
            cache.Set(1, "num1");
            cache.Set(2, "num2");
            cache.Set(3, "num3");

            // should be {2,'num2'} and {3, 'num3'}
            Assert.IsFalse(cache.TryGet(1, out var result));

            Assert.IsTrue(cache.TryGet(2, out result));
            Assert.AreEqual(result, "num2");

            Assert.IsTrue(cache.TryGet(3, out result));
            Assert.AreEqual(result, "num3");
        }

        [TestMethod]
        /*
         * The average time of this test is about 2ms. 
         */
        public void TestDPMemorySmall()
        {
            var cache = new LRUCache<int, int>(2);
            cache.Set(0, 1);
            cache.Set(1, 1);
            var fib9999 = 1242044891;
            var fib100000 = 2132534333;
            var maxIdx = 10000;
            for (int i = 2; i <= maxIdx; i++)
            {
                cache.TryGet(i - 2,  out var prev2);
                cache.TryGet(i - 1, out var prev1);
                cache.Set(i, prev1 + prev2);
            }

            Assert.IsFalse(cache.TryGet(9998, out var result));

            Assert.IsTrue(cache.TryGet(maxIdx - 1, out result));
            Assert.AreEqual(result, fib9999);

            Assert.IsTrue(cache.TryGet(maxIdx, out result));
            Assert.AreEqual(result, fib100000);
        }

        [TestMethod]
        /*
         * The average time of this test is about 3ms. 
         */
        public void TestDPMemoryLarge()
        {
            var cache = new LRUCache<int, int>(500);
            cache.Set(0, 1);
            cache.Set(1, 1);
            var fib9999 = 1242044891;
            var fib100000 = 2132534333;
            var maxIdx = 10000; 
            for (int i = 2; i <= 10000; i++)
            {
                cache.TryGet(i - 2, out var prev2);
                cache.TryGet(i - 1, out var prev1);
                cache.Set(i, prev1 + prev2);
            }

            Assert.IsFalse(cache.TryGet(1, out var result));

            Assert.IsTrue(cache.TryGet(maxIdx - 1, out result));
            Assert.AreEqual(result, fib9999);

            Assert.IsTrue(cache.TryGet(maxIdx, out result));
            Assert.AreEqual(result, fib100000);
        }

        [TestMethod]
        /*
         * The average time of this test is about 13ms(without the loop of Assert statements). 
         */
        public async Task TestMultiThreadingAsync()
        {
            var cache = new LRUCache<int, int>(10);
            var tasks = new List<Task>();
            var numOfThreads = 10;
            var numOfOps = 1000;
            for (var i = 0; i < numOfThreads; i++)
            {
                tasks.Add(Task.Run(() => StoreElement(cache, numOfOps, i)));
            }

            await Task.WhenAll(tasks);

            for (var i = numOfOps - numOfThreads; i < numOfOps; i++)
            {
               Assert.IsTrue(cache.TryGet(i, out var result));
            }
        }

        public void StoreElement(LRUCache<int, int> cache, int numOfOps, int idx)
        {
            for (int i = 0; i < numOfOps; i++)
            {
                var key = i;
                var value = i;
                cache.Set(key, value);
            }
        }
    }
}
