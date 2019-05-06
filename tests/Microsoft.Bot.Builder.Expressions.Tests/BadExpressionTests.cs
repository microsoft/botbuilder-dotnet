using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            # region General test
            Test("length(func())"), // no such function in children
            Test("func()"), // no such func
            Test("a.func()"), // no such function
            Test("(1.foreach)()"),// error func
            Test("('str'.foreach)()"),// error func
            # endregion

            # region Operators test
            Test("'1' + 2"), // params should be number
            Test("'1' * 2"), // params should be number
            Test("'1' - 2"), // params should be number
            Test("'1' / 2"), // params should be number
            Test("'1' % 2"), // params should be number
            Test("'1' ^ 2"), // params should be number
            Test("'string'&one"), // $ can only accept string parameter
            Test("1/0"), // $ can not divide 0
            # endregion
            
            # region String functions test
            Test("concat(one, hello)"), // concat can only accept string parameter
            Test("length(one, 1)"), // length can only have one param
            Test("length(concat(one, hello))"), // children func error
            Test("replace(hello)"), // replace need three parameters
            Test("replace(one, 'l', 'k')"), // replace only accept string parameter
            Test("replace('hi', 1, 'k')"), // replace only accept string parameter
            Test("replace('hi', 'l', 1)"), // replace only accept string parameter
            Test("replaceIgnoreCase(hello)"), // replaceIgnoreCase need three parameters
            Test("replaceIgnoreCase(one, 'l', 'k')"), // replaceIgnoreCase only accept string parameter
            Test("replaceIgnoreCase('hi', 1, 'k')"), // replaceIgnoreCase only accept string parameter
            Test("replaceIgnoreCase('hi', 'l', 1)"), // replaceIgnoreCase only accept string parameter
            Test("split(hello)"), // split need two parameters
            Test("split(one, 'l')"), // split only accept string parameter
            Test("split(hello, 1)"), // split only accept string parameter
            Test("substring(hello, 0.5)"), // the second parameter of substring must be integer
            Test("substring(one, 0)"), // the first parameter of substring must be string
            Test("substring(hello, 10)"), // the start index is out of the range of the string length
            Test("substring(hello, 0, hello)"), // length is not integer
            Test("substring(hello, 0, 10)"), // the length of substring is out of the range of the original string
            Test("substring(hello, 0, 'hello')"), // length is not integer
            Test("toLower(one)"), // the parameter of toLower must be string
            Test("toLower('hi', 1)"), // should have 1 param
            Test("toUpper(one)"), // the parameter of toUpper must be string
            Test("toUpper('hi', 1)"), // should have 1 param
            Test("trim(one)"), // the parameter of trim must be string
            Test("trim('hi', 1)"), // should have 1 param
            Test("endsWith(hello, one)"),// should have string params
            Test("endsWith(one, hello)"),// should have string params
            Test("endsWith(hello)"),// should have two params
            Test("startsWith(hello, one)"),// should have string params
            Test("startsWith(one, hello)"),// should have string params
            Test("startsWith(hello)"),// should have two params
            Test("countWord(hello, 1)"),// should have one param
            Test("countWord(one)"),// should have string param
            Test("countWord(one)"),// should have string param
            Test("addOrdinal(one)"),// should have Integer param
            Test("addOrdinal(one, two)"),// should have one param
            # endregion

            # region Logical comparison functions test
            Test("greater(one, hello)"), // string and integer are not comparable
            Test("greater(one)"), // greater need two parameters
            Test("greaterOrEquals(one, hello)"), // string and integer are not comparable
            Test("greaterOrEquals(one)"), // function need two parameters
            Test("less(false, true)"), // string or number parameters are needed
            Test("less(one, hello)"), // string and integer are not comparable
            Test("less(one)"), // function need two parameters
            Test("lessOrEquals(one, hello)"), // string and integer are not comparable
            Test("lessOrEquals(one)"), // function need two parameters
            Test("equals(one)"), // equals must accept two parameters
            Test("exists(1, 2)"), // function need one parameter
            //Test("if(!exists(one), one, hello)"), // the second and third parameters of if must the same type
            Test("not(false, one)"), // function need one parameter
            # endregion

            # region Conversion functions test
            Test("float(hello)"), // param shoud be float format string
            Test("float(hello, 1)"), // shold have 1 param
            Test("int(hello)"), // param shoud be int format string
            Test("int(1, 1)"), // shold have 1 param
            Test("string(hello, 1)"), // shold have 1 param
            Test("bool(false, 1)"), // shold have 1 param
            # endregion

            # region Math functions test
            Test("max(hello, one)"), // param should be number
            Test("max()"), // function need 1 or more than 1 parameters
            Test("min(hello, one)"), // param should be number
            Test("min()"), // function need 1 or more than 1 parameters
            Test("add(hello, 2)"), // param should be number
            Test("add()"), // arg count doesn't match
            Test("add(five, six)"), // no such variables
            Test("add(one)"), // add function need 2 or more than two parameters
            Test("sub(hello, 2)"), // param should be number
            Test("sub()"), // arg count doesn't match
            Test("sub(five, six)"), // no such variables
            Test("sub(one)"), // sub function need 2 or more than two parameters
            Test("mul(hello, one)"), // param should be number
            Test("mul(one)"), // mul function need 2 or more than two parameters
            Test("div(one, 0)"), // one cannot be divided by zero
            Test("div(one)"), // // div function need 2 or more than two parameters
            Test("div(hello, one)"), // string hello cannot be divided
            Test("exp(2, hello)"), // exp cannot accept parameter of string
            Test("mod(1, 0)"), // mod cannot accept zero as the second parameter
            Test("mod(5.5, 2)"), //  param should be integer
            Test("mod(5, 2.1)"), //  param should be integer
            Test("mod(5, 2.1 ,3)"), //  need two params
            Test("rand(5, 6.1)"), //  param should be integer
            Test("rand(5)"), //  need two params
            Test("rand(7, 6)"), //  minvalue cannot be greater than maxValue
            Test("sum(items)"), //  should have number parameters
            #endregion
            
            #region Date and time function test
            Test("addDays('errortime', 1)"),// error datetime format
            Test("addDays(timestamp, 'hi')"),// second param should be integer
            Test("addDays(timestamp)"),// should have 2 or 3 params
            Test("addDays(timestamp, 1,'yyyy', 2)"),// should have 2 or 3 params
            Test("addHours('errortime', 1)"),// error datetime format
            Test("addHours(timestamp, 'hi')"),// second param should be integer
            Test("addHours(timestamp)"),// should have 2 or 3 params
            Test("addHours(timestamp, 1,'yyyy', 2)"),// should have 2 or 3 params
            Test("addMinutes('errortime', 1)"),// error datetime format
            Test("addMinutes(timestamp, 'hi')"),// second param should be integer
            Test("addMinutes(timestamp)"),// should have 2 or 3 params
            Test("addMinutes(timestamp, 1,'yyyy', 2)"),// should have 2 or 3 params
            Test("addSeconds('errortime', 1)"),// error datetime format
            Test("addSeconds(timestamp, 'hi')"),// second param should be integer
            Test("addSeconds(timestamp)"),// should have 2 or 3 params
            Test("addSeconds(timestamp, 1,'yyyy', 2)"),// should have 2 or 3 params
            Test("dayOfMonth('errortime')"), // error datetime format
            Test("dayOfMonth(timestamp, 1)"), //should have 1 param
            Test("dayOfWeek('errortime')"), // error datetime format
            Test("dayOfWeek(timestamp, 1)"), //should have 1 param
            Test("dayOfYear('errortime')"), // error datetime format
            Test("dayOfYear(timestamp, 1)"), //should have 1 param
            Test("month('errortime')"), // error datetime format
            Test("month(timestamp, 1)"), //should have 1 param
            Test("date('errortime')"), // error datetime format
            Test("date(timestamp, 1)"), //should have 1 param
            Test("year('errortime')"), // error datetime format
            Test("year(timestamp, 1)"), // should have 1 param
            Test("formatDateTime('errortime')"), // error datetime format
            Test("formatDateTime(timestamp, 'yyyy', 1)"), // should have 2 or 3 params
            Test("subtractFromTime('errortime', 'yyyy', 1)"), // error datetime format
            Test("subtractFromTime(timestamp, 1, 'W')"),// error time unit
            Test("subtractFromTime(timestamp, timestamp, 'W')"),// error parameters format
            Test("subtractFromTime(timestamp, 'yyyy', '1')"), // third param should be integer
            Test("subtractFromTime(timestamp, 'yyyy')"), // should have 3 or 4 params
            Test("dateReadBack('errortime', 'errortime')"), // error datetime format
            Test("dateReadBack(timestamp)"), // shold have two params
            Test("getTimeOfDay('errortime')"), // error datetime format
            Test("getTimeOfDay(timestamp, timestamp)"), // should have 1 param
            Test("getPastTime(1, 'W')"),// error time unit
            Test("getPastTime(timestamp, 'W')"),// error parameters format
            Test("getPastTime('yyyy', '1')"),// second param should be integer
            Test("getPastTime('yyyy')"),// should have 2 or 3 params
            Test("getFeatureTime(1, 'W')"),// error time unit
            Test("getFeatureTime(timestamp, 'W')"),// error parameters format
            Test("getFeatureTime('yyyy', '1')"),// second param should be integer
            Test("getFeatureTime('yyyy')"),// should have 2 or 3 params
            # endregion

            # region collection functions test
            Test("sum(items, 'hello')"),//should have 1 parameter
            Test("sum('hello')"),//first param should be list
            Test("average(items, 'hello')"),//should have 1 parameter
            Test("average('hello')"),//first param should be list
            Test("average(hello)"),//first param should be list
            Test("contains('hello world', 'hello', 'new')"),//should have 2 parameter
            Test("count(items, 1)"), //should have 1 parameter
            Test("count(1)"), //first param should be list or string
            Test("empty(1,2)"), //should have two params
            Test("first(items,2)"), //should have 1 param
            Test("last(items,2)"), //should have 1 param
            Test("join(items, 'p1', 'p2','p3')"),//builtin function should have 3 params, 
                                                    //method extension should have 2-3 params
            Test("join(hello, 'hi')"),// first param must list
            Test("join(items, 1)"),// second param must string 
            Test("foreach(hello, item, item)"),// first arg is not list
            Test("foreach(items, item)"),//should have three parameters
            Test("foreach(items, item, item2, item3)"),//should have three parameters
            Test("foreach(items, add(1), item)"),// Second paramter of foreach is not an identifier
            Test("foreach(items, 1, item)"),// Second paramter error
            Test("foreach(items, x, sum(x))"),// third paramter error
            Test("union(one, two)"),// should have collection param
            Test("intersection(one, two)"),// should have collection param
            # endregion

            # region Object manipulation and construction functions test
            Test("json(1,2)"), //should have 1 parameter
            Test("json(1)"),//should be string parameter
            Test("json('{\"key1\":value1\"}')"), // invalid json format string 
            Test("addProperty(json('{\"key1\":\"value1\"}'), 'key2','value2','key3')"), //should have 3 parameter
            Test("addProperty(json('{\"key1\":\"value1\"}'), 1,'value2')"), // second param should be string
            Test("setProperty(json('{\"key1\":\"value1\"}'), 'key2','value2','key3')"), //should have 3 parameter
            Test("setProperty(json('{\"key1\":\"value1\"}'), 1,'value2')"), // second param should be string
            Test("removeProperty(json('{\"key1\":\"value1\",\"key2\":\"value2\"}'), 1))"),// second param should be string
            Test("removeProperty(json('{\"key1\":\"value1\",\"key2\":\"value2\"}'), '1', '2'))"),// should have 2 parameter
           # endregion

            # region Memory access test
            Test("getProperty(bag, 1)"),// second param should be string
            Test("Accessor(1)"),// first param should be string
            Test("Accessor(bag, 1)"),// second should be object
            Test("one[0]"),  // one is not list
            Test("items[3]"), // index out of range
            Test("items[one+0.5]"), // index is not integer
            # endregion
        };

        [DataTestMethod]
        [DynamicData(nameof(BadExpressions))]
        public void Evaluate(string exp)
        {
            var isFail = false;
            object scope = new
            {
                one = 1.0,
                two = 2.0,
                hello = "hello",
                world = "world",
                istrue = true,
                bag = new
                {
                    three = 3.0,
                    set = new
                    {
                        four = 4.0,
                    },
                    list = new[] { "red", "blue" },
                    index = 3,
                    name = "mybag"
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
                },
            };

            try
            {
                var (value, error) = new ExpressionEngine().Parse(exp).TryEvaluate(scope);
                if (error == null)
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
