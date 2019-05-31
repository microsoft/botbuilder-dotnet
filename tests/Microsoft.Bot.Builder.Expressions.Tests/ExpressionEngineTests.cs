using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Expressions.Tests
{
    [TestClass]
    public class ExpressionEngineTests
    {
        public static object[] Test(string input, object value, HashSet<string> paths = null) => new object[] { input, value, paths };

        public static HashSet<string> one = new HashSet<string> { "one" };
        public static HashSet<string> oneTwo = new HashSet<string> { "one", "two" };

        private readonly object scope = new
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
            user = new
            {
                lists = new
                {
                    todo = new []
                    {
                        "todo1",
                        "todo2",
                        "todo3",
                    }
                },
                listType = "todo",
            },
            turn = new
            {
                entities = new
                {
                    city = "Seattle",
                    ordinal = new []
                    {
                        "1",
                        "2",
                        "3"
                    }
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

        public static IEnumerable<object[]> Data => new[]
       {
            # region Operators test
            Test("1 + 2", 3),
            Test("- 1 + 2", 1),
            Test("+ 1 + 2", 3),
            Test("1 - 2", -1),
            Test("1 - (-2)", 3),
            Test("1.0 + 2.0", 3.0),
            Test("1 * 2 + 3", 5),
            Test("1 + 2 * 3", 7),
            Test("4 / 2", 2),
            Test("1 + 3 / 2", 2),
            Test("(1 + 3) / 2", 2),
            Test("1 * (2 + 3)", 5),
            Test("(1 + 2) * 3", 9),
            Test("(one + two) * bag.three", 9.0, new HashSet<string> {"one", "two", "bag.three" }),
            Test("(one + two) * bag.set.four", 12.0, new HashSet<string> {"one", "two", "bag.set.four" } ),
            Test("2^2", 4.0),
            Test("3^2^2", 81.0),
            Test("one > 0.5 && two < 2.5", true),
            Test("one > 0.5 || two < 1.5", true),
            Test("5 % 2", 1),
            Test("!(one == 1.0)", false),
            Test("!!(one == 1.0)", true),
            Test("!exists(xione) || !!exists(two)", true),
            Test("(1 + 2) == (4 - 1)", true),
            Test("!!exists(one) == !!exists(one)", true),
            Test("!(one == 1.0)", false, new HashSet<string> {"one" }),
            Test("!!(one == 1.0)", true, new HashSet<string> {"one" }),
            Test("!(one == 1.0) || !!(two == 2.0)", true, oneTwo),
            Test("!true", false),
            Test("!!true", true),
            Test("!(one == 1.0) || !!(two == 2.0)", true),
            Test("hello == 'hello'", true),
            Test("hello == 'world'", false),
            Test("(1 + 2) != (4 - 1)", false),
            Test("!!exists(one) != !!exists(one)", false),
            Test("hello != 'hello'", false),
            Test("hello != 'world'", true),
            Test("hello != \"hello\"", false),
            Test("hello != \"world\"", true),
            Test("(1 + 2) >= (4 - 1)", true),
            Test("(2 + 2) >= (4 - 1)", true),
            Test("float(5.5) >= float(4 - 1)", true),
            Test("(1 + 2) <= (4 - 1)", true),
            Test("(2 + 2) <= (4 - 1)", false),
            Test("float(5.5) <= float(4 - 1)", false),
            Test("'string'&'builder'","stringbuilder"),
            Test("\"string\"&\"builder\"","stringbuilder"),
            Test("one > 0.5 && two < 2.5", true, oneTwo),
            Test("notThere > 4", false), 
            Test("float(5.5) && float(0.0)", true),
            Test("hello && \"hello\"", true),
            Test("items || ((2 + 2) <= (4 - 1))", true), // true || false
            Test("0 || false", true), // true || false
            Test("!(hello)", false), // false
            Test("!(10)", false),
            Test("!(0)", false),
            Test("one > 0.5 || two < 1.5", true, oneTwo),
            Test("one / 0 || two", true),
            Test("0/3", 0),
            # endregion

            # region  String functions test
            Test("concat(hello,world)","helloworld"),
            Test("concat('hello','world')","helloworld"),
            Test("concat(\"hello\",\"world\")","helloworld"),
            Test("length('hello')",5),
            Test("length(\"hello\")",5),
            Test("length(concat(hello,world))",10),
            Test("count('hello')",5),
            Test("count(\"hello\")",5),
            Test("count(concat(hello,world))",10),
            Test("replace('hello', 'l', 'k')","hekko"),
            Test("replace('hello', 'L', 'k')","hello"),
            Test("replaceIgnoreCase('hello', 'L', 'k')","hekko"),
            Test("split('hello','e')",new string[]{ "h","llo"}),
            Test("substring('hello', 0, 5)", "hello"),
            Test("substring('hello', 0, 3)", "hel"),
            Test("substring('hello', 3)", "lo"),
            Test("substring('hello', 0, bag.index)", "hel"),
            Test("toLower('UpCase')", "upcase"),
            Test("toUpper('lowercase')", "LOWERCASE"),
            Test("toLower(toUpper('lowercase'))", "lowercase"),
            Test("trim(' hello ')", "hello"),
            Test("trim(' hello')", "hello"),
            Test("trim('hello')", "hello"),
            Test("endsWith('hello','o')", true),
            Test("endsWith('hello','a')", false),
            Test("endsWith(hello,'o')", true),
            Test("endsWith(hello,'a')", false),
            Test("startsWith('hello','h')", true),
            Test("startsWith('hello','a')", false),
            Test("countWord(hello)", 1),
            Test("countWord(concat(hello, ' ', world))", 2),
            Test("addOrdinal(11)", "11th"),
            Test("addOrdinal(11 + 1)", "12th"),
            Test("addOrdinal(11 + 2)", "13th"),
            Test("addOrdinal(11 + 10)", "21st"),
            Test("addOrdinal(11 + 11)", "22nd"),
            Test("addOrdinal(11 + 12)", "23rd"),
            Test("addOrdinal(11 + 13)", "24th"),
            Test("addOrdinal(-1)", "-1"),//original string value
            
            # endregion

            # region  Logical comparison functions test
            Test("and(1 == 1, 1 < 2, 1 > 2)", false),
            Test("and(!true, !!true)", false),//false && true
            Test("and(!!true, !!true)", true),//true && true
            Test("and(hello != 'world', bool('true'))", true),//true && true
            Test("and(hello == 'world', bool('true'))", false),//false && true
            Test("or(!exists(one), !!exists(one))", true),//false && true
            Test("or(!exists(one), !exists(one))", false),//false && false
            Test("greater(one, two)", false, oneTwo),
            Test("greater(one , 0.5) && less(two , 2.5)", true),// true && true
            Test("greater(one , 0.5) || less(two , 1.5)", true),//true || false
            Test("greater(5, 2)", true),
            Test("greater(2, 2)", false),
            Test("greater(one, two)", false),
            Test("greaterOrEquals((1 + 2) , (4 - 1))", true),
            Test("greaterOrEquals((2 + 2) , (4 - 1))", true),
            Test("greaterOrEquals(float(5.5) , float(4 - 1))", true),
            Test("greaterOrEquals(one, one)", true),
            Test("greaterOrEquals(one, two)", false),
            Test("greaterOrEquals(one, one)", true, one),
            Test("greaterOrEquals(one, two)", false, oneTwo),
            Test("less(5, 2)", false),
            Test("less(2, 2)", false),
            Test("less(one, two)", true),
            Test("less(one, two)", true, oneTwo),
            Test("lessOrEquals(one, one)", true, new HashSet<string>{"one" }),
            Test("lessOrEquals(one, two)", true, oneTwo),
            Test("lessOrEquals(one, one)", true),
            Test("lessOrEquals(one, two)", true),
            Test("lessOrEquals((1 + 2) , (4 - 1))", true),
            Test("lessOrEquals((2 + 2) , (4 - 1))", false),
            Test("lessOrEquals(float(5.5) , float(4 - 1))", false),
            Test("lessOrEquals(one, one)", true),
            Test("lessOrEquals(one, two)", true),
            Test("equals(hello, 'hello')", true),
            Test("equals(bag.index, 3)", true),
            Test("equals(bag.index, 2)", false),
            Test("equals(hello == 'world', bool('true'))", false),//false, true
            Test("equals(hello == 'world', bool(0))", false),//false, true
            Test("if(!exists(one), 'r1', 'r2')", "r2"),//false
            Test("if(!!exists(one), 'r1', 'r2')", "r1"),//true
            Test("if(0, 'r1', 'r2')", "r1"),//true
            Test("if(bool('true'), 'r1', 'r2')", "r1"),//true
            Test("if(istrue, 'r1', 'r2')", "r1"),//true
            Test("exists(one)", true),
            Test("exists(xxx)", false),
            Test("exists(one.xxx)", false),
            Test("not(one != null)", false),
            Test("not(not(one != null))", true),
            Test("not(false)", true),
            Test("not(one == 1.0)", false, new HashSet<string> {"one" }),
            Test("not(not(one == 1.0))", true, new HashSet<string> {"one" }),
            Test("not(false)", true),
            Test("and(one > 0.5, two < 2.5)", true, oneTwo),
            Test("and(float(5.5), float(0.0))", true),
            Test("and(hello, \"hello\")", true),
            Test("or(items, (2 + 2) <= (4 - 1))", true), // true || false
            Test("or(0, false)", true), // true || false
            Test("not(hello)", false), // false
            Test("not(10)", false),
            Test("not(0)", false),
            Test("if(hello, 'r1', 'r2')", "r1"),
            Test("if(null, 'r1', 'r2')", "r2"),
            Test("if(hello * 5, 'r1', 'r2')", "r2"),
            Test("if(10, 'r1', 'r2')", "r1"),
            # endregion

            # region  Conversion functions test
            Test("float('10.333')", 10.333f),
            Test("float('10')", 10.0f),
            Test("int('10')", 10),
            Test("string('str')", "str"),
            Test("string(one)", "1.0"),
            Test("string(bool(1))", "true"),
            Test("string(bag.set)", "{\"four\":4.0}"),
            Test("bool(1)", true),
            Test("bool(0)", true),
            Test("bool(null)", false),
            Test("bool(hello * 5)", false),
            Test("bool('false')", true),
            Test("bool('hi')", true),
            Test("createArray('h', 'e', 'l', 'l', 'o')", new List<object>{"h", "e", "l", "l", "o" }),
            Test("createArray(1, bool(0), string(bool(1)), float('10'))", new List<object>{1, true, "true", 10.0f }),
            # endregion

            # region  Math functions test
            Test("add(1, 2, 3)", 6),
            Test("add(1, 2)", 3),
            Test("add(1.0, 2.0)", 3.0),
            Test("add(mul(1, 2), 3)", 5),
            Test("max(mul(1, 2), 5) ", 5),
            Test("max(5) ", 5),
            Test("max(4, 5) ", 5),
            Test("min(mul(1, 2), 5) ", 2),
            Test("min(4, 5) ", 4),
            Test("min(4) ", 4),
            Test("min(1.0, two) + max(one, 2.0)", 3.0, oneTwo),
            Test("sub(2, 1)", 1),
            Test("sub(2, 1, 1)", 0),
            Test("sub(2.0, 0.5)", 1.5),
            Test("mul(2, 5)", 10),
            Test("mul(2, 5, 2)", 20),
            Test("div(mul(2, 5), 2)", 5),
            Test("div(5, 2)", 2),
            Test("div(5, 2 ,2)", 1),
            Test("exp(2,2)", 4.0),
            Test("mod(5,2)", 1),
            Test("rand(1, 2)", 1),
            Test("rand(2, 3)", 2),
            # endregion

            # region  Date and time function test
            //init dateTime: 2018-03-15T13:00:00Z
            Test("addDays(timestamp, 1)", "2018-03-16T13:00:00.0000000Z"),
            Test("addDays(timestamp, 1,'MM-dd-yy')", "03-16-18"),
            Test("addHours(timestamp, 1)", "2018-03-15T14:00:00.0000000Z"),
            Test("addHours(timestamp, 1,'MM-dd-yy hh-mm')", "03-15-18 02-00"),
            Test("addMinutes(timestamp, 1)", "2018-03-15T13:01:00.0000000Z"),
            Test("addMinutes(timestamp, 1, 'MM-dd-yy hh-mm')", "03-15-18 01-01"),
            Test("addSeconds(timestamp, 1)", "2018-03-15T13:00:01.0000000Z"),
            Test("addSeconds(timestamp, 1, 'MM-dd-yy hh-mm-ss')", "03-15-18 01-00-01"),
            Test("dayOfMonth(timestamp)", 15),
            Test("dayOfWeek(timestamp)", 4),//Thursday
            Test("dayOfYear(timestamp)", 74),
            Test("month(timestamp)", 3),
            Test("date(timestamp)", "3/15/2018"),//Default. TODO
            Test("year(timestamp)", 2018),
            Test("formatDateTime(timestamp)", "2018-03-15T13:00:00.0000000Z"),
            Test("formatDateTime(timestamp, 'MM-dd-yy')", "03-15-18"),
            Test("subtractFromTime(timestamp, 1, 'Year')", "2017-03-15T13:00:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Month')", "2018-02-15T13:00:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Week')", "2018-03-08T13:00:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Day')", "2018-03-14T13:00:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Hour')", "2018-03-15T12:00:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Minute')", "2018-03-15T12:59:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Second')", "2018-03-15T12:59:59.0000000Z"),
            Test("dateReadBack(timestamp, addDays(timestamp, 1))", "tomorrow"),
            Test("dateReadBack(addDays(timestamp, 1),timestamp))", "yesterday"),
            Test("getTimeOfDay('2018-03-15T00:00:00Z')", "midnight"),
            Test("getTimeOfDay('2018-03-15T08:00:00Z')", "morning"),
            Test("getTimeOfDay('2018-03-15T12:00:00Z')", "noon"),
            Test("getTimeOfDay('2018-03-15T13:00:00Z')", "afternoon"),
            Test("getTimeOfDay('2018-03-15T18:00:00Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T22:00:00Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T23:00:00Z')", "night"),
            Test("getPastTime(1,'Year','MM-dd-yy')", DateTime.Now.AddYears(-1).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Month','MM-dd-yy')", DateTime.Now.AddMonths(-1).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Week','MM-dd-yy')", DateTime.Now.AddDays(-7).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Day','MM-dd-yy')", DateTime.Now.AddDays(-1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Year','MM-dd-yy')", DateTime.Now.AddYears(1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Month','MM-dd-yy')", DateTime.Now.AddMonths(1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Week','MM-dd-yy')", DateTime.Now.AddDays(7).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Day','MM-dd-yy')", DateTime.Now.AddDays(1).ToString("MM-dd-yy")),
           
            # endregion

            # region  collection functions test
            Test("sum(createArray(1, 2))", 3),
            Test("sum(createArray(one, two, 3))", 6.0),
            Test("average(createArray(1, 2))", 1.5),
            Test("average(createArray(one, two, 3))", 2.0),
            Test("contains('hello world', 'hello')", true),
            Test("contains('hello world', 'hellow')", false),
            Test("contains(items, 'zero')", true),
            Test("contains(items, 'hi')", false),
            Test("contains(bag, 'three')", true),
            Test("contains(bag, 'xxx')", false),
            Test("count(split(hello,'e'))",2),
            Test("count(createArray('h', 'e', 'l', 'l', 'o'))",5),
            Test("empty('')", true),
            Test("empty('a')", false),
            Test("empty(bag)", false),
            Test("empty(items)", false),
            Test("first(items)", "zero"),
            Test("first('hello')", "h"),
            Test("first(createArray(0, 1, 2))", 0),
            Test("first(1)", null),
            Test("first(nestedItems).x", 1, new HashSet<string> { "nestedItems"}),
            Test("join(items,',')", "zero,one,two"),
            Test("join(createArray('a', 'b', 'c'), '.')", "a.b.c"),
            Test("join(foreach(items, item, item), ',')", "zero,one,two"),
            Test("join(foreach(nestedItems, i, i.x + first(nestedItems).x), ',')", "2,3,4", new HashSet<string>{ "nestedItems"}),
            Test("join(foreach(items, item, concat(item, string(count(items)))), ',')", "zero3,one3,two3", new HashSet<string>{ "items"}),
            Test("last(items)", "two"),
            Test("last('hello')", "o"),
            Test("last(createArray(0, 1, 2))", 2),
            Test("last(1)", null),
            Test("count(union(createArray('a', 'b')))", 2),
            Test("count(union(createArray('a', 'b'), createArray('b', 'c'), createArray('b', 'd')))", 4),
            Test("count(intersection(createArray('a', 'b')))", 2),
            Test("count(intersection(createArray('a', 'b'), createArray('b', 'c'), createArray('b', 'd')))", 1),
            
            # endregion

            # region  Object manipulation and construction functions
            Test("string(addProperty(json('{\"key1\":\"value1\"}'), 'key2','value2'))", "{\"key1\":\"value1\",\"key2\":\"value2\"}"),
            Test("string(setProperty(json('{\"key1\":\"value1\"}'), 'key1','value2'))", "{\"key1\":\"value2\"}"),
            Test("string(removeProperty(json('{\"key1\":\"value1\",\"key2\":\"value2\"}'), 'key2'))", "{\"key1\":\"value1\"}"),
            # endregion

            # region  Short Hand Expression
            Test("@city == 'Bellevue'", false, new HashSet<string> {"turn.entities.city"}),
            Test("@city", "Seattle", new HashSet<string> {"turn.entities.city"}),
            Test("@city == 'Seattle'", true, new HashSet<string> {"turn.entities.city"}),
            Test("#BookFlight == 'BookFlight'", true, new HashSet<string> {"turn.intents.BookFlight"}),
            Test("exists(#BookFlight)", true, new HashSet<string> {"turn.intents.BookFlight"}),
            Test("$title", "Dialog Title", new HashSet<string> {"dialog.result.title"}),
            Test("$subTitle", "Dialog Sub Title", new HashSet<string> {"dialog.result.subTitle"}),
            # endregion

            # region  Memory access
            Test("getProperty(bag, concat('na','me'))","mybag"),
            Test("items[2]", "two", new HashSet<string> { "items[2]" }),
            Test("bag.list[bag.index - 2]", "blue", new HashSet<string> {"bag.list", "bag.index" }),
            Test("items[nestedItems[1].x]", "two", new HashSet<string> { "items","nestedItems[1].x" }),
            Test("bag['name']","mybag"),
            Test("bag[substring(concat('na','me','more'), 0, length('name'))]","mybag"),
            Test("items[1+1]","two"),
            Test("getProperty(null, 'p')", null),
            Test("(getProperty(null, 'p'))[1]", null),
            #endregion

            # region Dialog 
            Test("user.lists.todo[int(@ordinal[0]) - 1] != null", true),
            Test("user.lists.todo[int(@ordinal[0]) + 3] != null", false),
            Test("count(user.lists.todo) > int(@ordinal[0]))", true),
            Test("count(user.lists.todo) >= int(@ordinal[0]))", true),
            Test("user.lists.todo[int(@ordinal[0]) - 1]", "todo1"),
            Test("user.lists[user.listType][int(@ordinal[0]) - 1]", "todo1"),
            #endregion

        };

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Evaluate(string input, object expected, HashSet<string> expectedRefs)
        {
            var parsed = new ExpressionEngine().Parse(input);
            Assert.IsNotNull(parsed);
            var (actual, msg) = parsed.TryEvaluate(scope);
            Assert.AreEqual(null, msg);
            AssertObjectEquals(expected, actual);
            if (expectedRefs != null)
            {
                var actualRefs = parsed.References();
                Assert.IsTrue(expectedRefs.SetEquals(actualRefs), $"References do not match, expected: {string.Join(',', expectedRefs)} acutal: {string.Join(',', actualRefs)}");
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void EvaluateJson(string input, object expected, HashSet<string> expectedRefs)
        {
            var jsonScope = JToken.FromObject(scope);
            var parsed = new ExpressionEngine().Parse(input);
            Assert.IsNotNull(parsed);
            var (actual, msg) = parsed.TryEvaluate(jsonScope);
            Assert.AreEqual(null, msg);
            AssertObjectEquals(expected, actual);
            if (expectedRefs != null)
            {
                var actualRefs = parsed.References();
                Assert.IsTrue(expectedRefs.SetEquals(actualRefs), $"References do not match, expected: {string.Join(',', expectedRefs)} acutal: {string.Join(',', actualRefs)}");
            }
        }

        public static bool IsNumber(object value) =>
            value is sbyte
            || value is byte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong
            || value is float
            || value is double
            || value is decimal;

        private void AssertObjectEquals(object expected, object actual)
        {
            if (IsNumber(actual) && IsNumber(expected))
            {
                if (actual is int)
                {
                    Assert.IsTrue(expected is int);
                }
                else
                {
                    Assert.IsTrue(Convert.ToSingle(actual) == Convert.ToSingle(expected));
                }
            }
            // Compare two lists
            else if (expected is IList expectedList
                && actual is IList actualList)
            {
                Assert.AreEqual(expectedList.Count, actualList.Count);
                for (var i = 0; i < expectedList.Count; i++)
                {
                    Assert.AreEqual(expectedList[i], actualList[i]);
                }
            }
            else
            {
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
