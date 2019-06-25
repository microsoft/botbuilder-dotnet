using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using Antlr4.Runtime.Atn;
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

        //public int Fibonacci(LRUCache<int, int> cache, int n)
        //{
        //
        //}
            
 

        [TestMethod]
        public void TestDPMemorySmall()
        {
            var cache = new LRUCache<int, int>(2);
            cache.Set(0, 1);
            cache.Set(1, 1);
            for (int i = 2; i <= 1000; i++){
                cache.TryGet(i - 2,  out var prev2);
                cache.TryGet(i - 1, out var prev1);
                cache.Set(i, prev1 + prev2);
            }

            Assert.IsFalse(cache.TryGet(998, out var result));

            Assert.IsTrue(cache.TryGet(999, out result));
            Assert.AreEqual(result, 1556111435);

            Assert.IsTrue(cache.TryGet(1000, out result));
            Assert.AreEqual(result, 1318412525);
        }

        [TestMethod]
        public void TestDPMemoryLarge()
        {
            var cache = new LRUCache<int, int>(500);
            cache.Set(0, 1);
            cache.Set(1, 1);
            for (int i = 2; i <= 1000; i++)
            {
                cache.TryGet(i - 2, out var prev2);
                cache.TryGet(i - 1, out var prev1);
                cache.Set(i, prev1 + prev2);
            }

            Assert.IsFalse(cache.TryGet(1, out var result));

            Assert.IsTrue(cache.TryGet(999, out result));
            Assert.AreEqual(result, 1556111435);

            Assert.IsTrue(cache.TryGet(1000, out result));
            Assert.AreEqual(result, 1318412525);
        }
    }
}
