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

    }
}
