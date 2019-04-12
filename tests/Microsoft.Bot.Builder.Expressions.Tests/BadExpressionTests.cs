using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Expressions.Tests
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
            Test("func(A,b,b,)"),
            Test("a.#title"),
            Test("\"hello'"),
        };




        [DataTestMethod]
        [DynamicData(nameof(InvalidExpressions))]
        [ExpectedException(typeof(Exception))]
        public void Parse(string exp)
        {
            try
            {
                new ExpressionEngine().Parse(exp);
            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.Message);
                throw e;
            }
        }


        public static IEnumerable<object[]> BadExpressions => new[]
        {
            Test("one[0]"),  // one is not list
            Test("add(hello, 2)"), // string + int
            Test("add()"), // arg count doesn't match
            Test("func()"), // no such func
            Test("add(five, six)"), // no such variables
        };

        [DataTestMethod]
        [DynamicData(nameof(BadExpressions))]
        public void Evaluate(string exp)
        {
            var isFail = false;
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
                items = new string[] { "zero", "one", "two" },
                
            };

            try
            {
                var (value, error) = new ExpressionEngine().Parse(exp).TryEvaluate(scope);
                if(error == null)
                {
                    isFail = true;
                }
                else
                {
                    TestContext.WriteLine(error);
                }
            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.Message);
            }

            if (isFail)
            {
                Assert.Fail("Test method did not throw expected exception");
            }
        }
    }
}
