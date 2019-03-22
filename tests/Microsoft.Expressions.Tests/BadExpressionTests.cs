using System; 
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Expressions.Tests
{
    [TestClass]
    public class BadExpressionTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        public static object[] Test(string input) => new object[] { input };

        public static IEnumerable<object[]> InvalidExpressions => new[]
        {
            Test("a+"),
            Test("a+b*"),
            Test("fun(a, b, c"),
            Test("func(A,b,b,)")
        };




        [DataTestMethod]
        [DynamicData(nameof(InvalidExpressions))]
        public void Parse(string exp)
        {
            try
            {
                ExpressionEngine.Parse(exp);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ExpressionParsingException));
                TestContext.WriteLine(e.Message);
            }
        }


        public static IEnumerable<object[]> BadExpressions => new[]
        {
            Test("one[0]"),  // one is not list
            Test("add(one, 2)"), // double + int
            Test("add(hello, 2)"), // string + int
            Test("add()"), // arg count doesn't match
            Test("func()"), // no such func
            Test("add(five, six)"), // no such variables
        };

        [DataTestMethod]
        [DynamicData(nameof(BadExpressions))]
        public void Evaluate(string exp)
        {
            var scope = new
            {
                one = 1.0,
                two = 2.0,
                hello = "hello",
                world = "world",
                bag = new
                {
                    three = 3.0,
                    set = new
                    {
                        four = 4.0,
                    },
                    index = 3,
                    list = new[] { "red", "blue" }
                },
                items = new string[] { "zero", "one", "two" }
            };

            try
            {
                ExpressionEngine.Evaluate(exp, scope);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ExpressionEvaluationException));
                TestContext.WriteLine(e.Message);
            }
        }

    }
}
