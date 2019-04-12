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

            //Test("add(one)"), // add function need two variables
            Test("items[3]"), // index out of range
            Test("items[one+0.5]"), // index is not integer
            Test("div(one, 0)"), // one cannot be divided by zero
            Test("div(hello, one)"), // string hello cannot be divided
            Test("and(one, hello, one < two)"), //one and hello are not bool type
            Test("greater(one, hello)"), // string and integer are not comparable
            Test("greater(one)"), // greater need two parameters
            Test("less(one, hello)"), // string and integer are not comparable
            Test("less(one)"), // less need two parameters
            Test("pow(2, hello)"), // pow cannot accept parameter of string
            Test("mod(one, 0)"), // mod cannot accept zero as the second parameter
            Test("not(hello)"), // not can only accept bool parameter
            Test("'string'&one"), // $ can only accept string parameter
            Test("concat(one, hello)"), // concat can only accept string parameter
            Test("length(one)"), // length can only accept string parameter
            Test("replace(hello)"), // replace need three parameters
            Test("replace(one, 'l', 'k')"), // replace only accept string parameter
            Test("split(hello)"), // split need two parameters
            Test("split(one, 'l')"), // split only accept string parameter
            Test("substring(hello, 0.5)"), // the second parameter of substring must be integer
            Test("substring(one, 0)"), // the first parameter of substring must be string
            Test("substring(hello, 10)"), // the start index is out of the range of the string length
            Test("substring(hello, 0, 10)"), // the length of substring is out of the range of the original string
            Test("toLower(one)"), // the parameter of toLower must be string
            Test("toUpper(one)"), // the parameter of toUpper must be string
            Test("trim(one)"), // the parameter of trim must be string
            Test("equals(one)"), // equals must accept two parameters
            Test("if(hello, 'r1', 'r2')"), // the first parameter of the if must be bool
            //Test("if(!exists(one), one, hello)"), // the second and third parameters of if must the same type
            //Test("or(hello == 'hello')"), // or function needs two parameters
            Test("or(hello, one)"), // or function only accept bool parameters
            Test("max(hello, one)"), // max can only accept two parameters with same type of integer, float or string
            Test("mul(hello, one)"), // mul can only accept two parameters of interger or float
            //Test("mul(one)"), // mul function need two parameters


            //foreach
            Test("foreach(hello, item, item)"),// first arg is not list
            Test("foreach(items, item)"),//should have three args
            Test("foreach(items, item, item2, item3)"),//should have three args
            Test("foreach(items, add(1), item)"),// Second paramter of foreach is not an identifier

            //join
            Test("join(items, 'p1', 'p2','p3')"),//builtin function should have 3 params, 
                                                    //method extension should have 2-3 params
            Test("join(hello, 'hi')"),// first args must list
            //Test("join(items, 1)"),// second args must string 
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
                    list = new[] { "red", "blue" },
                    index = 3
                },
                items = new string[] { "zero", "one", "two" },
                nestedItems = new[]
                {
                    new
                    {
                        x = 1
                    },
                    new
                    {
                        x = 2,
                    },
                    new
                    {
                        x = 3,
                    }
                },
                timestamp = "2018-03-15T13:00:00Z",
                turn = new
                {
                    entities = new
                    {
                        city = "Seattle"
                    },
                    intents = new
                    {
                        BookFlight = "BookFlight"
                    }
                },
                dialog = new
                {
                    result = new
                    {
                        title = "Dialog Title",
                        subTitle = "Dialog Sub Title"
                    }
                }
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
