using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.AdaptiveExpressions.Core.AOT.Tests
{
    public class LRUCacheTest
    {
        [Fact]
        public void TestBasic()
        {
            var cache = new LRUCache<int, string>(2);
            
            Assert.False(cache.TryGet(1, out var result));

            cache.Set(1, "num1");

            Assert.True(cache.TryGet(1, out result));
            Assert.Equal("num1", result);
        }

        [Fact]
        public void TestDiacardPolicy()
        {
            var cache = new LRUCache<int, string>(2);
            cache.Set(1, "num1");
            cache.Set(2, "num2");
            cache.Set(3, "num3");

            // should be {2,'num2'} and {3, 'num3'}
            Assert.False(cache.TryGet(1, out var result));

            Assert.True(cache.TryGet(2, out result));
            Assert.Equal("num2", result);

            Assert.True(cache.TryGet(3, out result));
            Assert.Equal("num3", result);
        }

        [Fact]
        /*
         * The average time of this test is about 2ms. 
         */
        public void TestDPMemorySmall()
        {
            var cache = new LRUCache<int, int>(2);
            cache.Set(0, 1);
            cache.Set(1, 1);
            const int fib9999 = 1242044891;
            const int fib100000 = 2132534333;
            const int maxIdx = 10000;
            for (var i = 2; i <= maxIdx; i++)
            {
                cache.TryGet(i - 2,  out var prev2);
                cache.TryGet(i - 1, out var prev1);
                cache.Set(i, prev1 + prev2);
            }

            Assert.False(cache.TryGet(9998, out var result));

            Assert.True(cache.TryGet(maxIdx - 1, out result));
            Assert.Equal(fib9999, result);

            Assert.True(cache.TryGet(maxIdx, out result));
            Assert.Equal(fib100000, result);
        }

        [Fact]
        /*
         * The average time of this test is about 3ms. 
         */
        public void TestDPMemoryLarge()
        {
            var cache = new LRUCache<int, int>(500);
            cache.Set(0, 1);
            cache.Set(1, 1);
            const int fib9999 = 1242044891;
            const int fib100000 = 2132534333;
            const int maxIdx = 10000; 
            for (var i = 2; i <= 10000; i++)
            {
                cache.TryGet(i - 2, out var prev2);
                cache.TryGet(i - 1, out var prev1);
                cache.Set(i, prev1 + prev2);
            }

            Assert.False(cache.TryGet(1, out var result));

            Assert.True(cache.TryGet(maxIdx - 1, out result));
            Assert.Equal(fib9999, result);

            Assert.True(cache.TryGet(maxIdx, out result));
            Assert.Equal(fib100000, result);
        }

        [Fact]
        /*
         * The average time of this test is about 13ms(without the loop of Assert statements). 
         */
        public async Task TestMultiThreadingAsync()
        {
            var cache = new LRUCache<int, int>(10);
            var tasks = new List<Task>();
            const int numOfThreads = 10;
            const int numOfOps = 1000;
            for (var i = 0; i < numOfThreads; i++)
            {
                tasks.Add(Task.Run(() => StoreElement(cache, numOfOps, i)));
            }

            await Task.WhenAll(tasks);

            for (var i = numOfOps - numOfThreads; i < numOfOps; i++)
            {
               Assert.True(cache.TryGet(i, out var result));
            }
        }

        private void StoreElement(LRUCache<int, int> cache, int numOfOps, int idx)
        {
            for (var i = 0; i < numOfOps; i++)
            {
                var key = i;
                var value = i;
                cache.Set(key, value);
            }
        }
    }
}
