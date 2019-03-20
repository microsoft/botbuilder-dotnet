using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Expressions.Tests
{
    [TestClass]
    public class ExpressionTree
    {
        [TestMethod]
        public void ExprTree()
        {
            var t1 = ExpressionEngine.Parse("{a} > 3");
            Console.WriteLine(t1);
        } 
    }
}
