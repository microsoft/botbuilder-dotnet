using System.Collections;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Expressions.Tests
{
    [TestClass]
    public class ExpressionEngineTests
    {
        public static object[] Test(string input, object value, HashSet<string> paths = null) => new object[] { input, value, paths };

        public static HashSet<string> one = new HashSet<string> { "one" };
        public static HashSet<string> oneTwo = new HashSet<string> {"one", "two" };

        public static IEnumerable<object[]> Data => new[]
       {
            # region Operators test
            
            Test("1 + 2", 3),
            Test("1 - 2", -1),
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
            Test("one > 0.5 || two < 1.5", true, oneTwo),
            # endregion

            # region  String functions test
            Test("concat(hello,world)","helloworld"),
            Test("concat('hello','world')","helloworld"),
            Test("concat(\"hello\",\"world\")","helloworld"),
            Test("length('hello')",5),
            Test("length(\"hello\")",5),
            Test("length(concat(hello,world))",10),
            Test("replace('hello', 'l', 'k')","hekko"),
            Test("replace('hello', 'L', 'k')","hello"),
            Test("replaceIgnoreCase('hello', 'L', 'k')","hekko"),
            Test("split('hello','e')",new string[]{ "h","llo"}),
            Test("substring('hello', 0, 5)", "hello"),
            Test("substring('hello', 0, 3)", "hel"),
            Test("toLower('UpCase')", "upcase"),
            Test("toUpper('lowercase')", "LOWERCASE"),
            Test("toLower(toUpper('lowercase'))", "lowercase"),
            Test("trim(' hello ')", "hello"),
            Test("trim(' hello')", "hello"),
            Test("trim('hello')", "hello"),
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
            Test("equals(hello == 'world', bool(0))", true),//false, false
            Test("if(!exists(one), 'r1', 'r2')", "r2"),//false
            Test("if(!!exists(one), 'r1', 'r2')", "r1"),//true
            Test("if(bool(0), 'r1', 'r2')", "r2"),//false
            Test("if(bool('true'), 'r1', 'r2')", "r1"),//true
            Test("exists(one)", true),
            Test("exists(xxx)", false),
            Test("exists(one.xxx)", false),
            Test("not(one != null)", false),
            Test("not(not(one != null))", true),
            Test("not(false)", true),
            Test("not(one == 1.0)", false, new HashSet<string> {"one" }),
            Test("not(not(one == 1.0))", true, new HashSet<string> {"one" }),
            Test("not(false)", true),
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
            Test("bool(0)", false),
            Test("bool('false')", false),
            Test("bool('true')", true),
            Test("createArray('h', 'e', 'l', 'l', 'o')", new List<object>{"h", "e", "l", "l", "o" }),
            Test("createArray(1, bool('false'), string(bool(1)), float('10'))", new List<object>{1, false, "true", 10.0f }),
            # endregion

            # region  Math functions test
            Test("add(1, 2, 3)", 6),
            Test("add(1, 2)", 3),
            Test("add(1.0, 2.0)", 3.0),
            Test("add(mul(1, 2), 3)", 5),
            Test("max(mul(1, 2), 5) ", 5),
            Test("max(4, 5) ", 5),
            Test("min(mul(1, 2), 5) ", 2),
            Test("min(4, 5) ", 4),
            Test("min(1.0, two) + max(one, 2.0)", 3.0, oneTwo),
           
            Test("sub(2, 1)", 1),
            Test("sub(2.0, 0.5)", 1.5),
            Test("mul(2, 5)", 10),
            Test("div(mul(2, 5), 2)", 5),
            Test("div(5, 2)", 2),
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
            Test("subtractFromTime(timestamp, 1, 'Day')", "2018-03-14T13:00:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Minute')", "2018-03-15T12:59:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Second')", "2018-03-15T12:59:59.0000000Z"),
            Test("dateReadBack(timestamp, addDays(timestamp, 1))", "Tomorrow"),
            Test("dateReadBack(addDays(timestamp, 1),timestamp))", "Yesterday"),
            Test("getTimeOfDay('2018-03-15T00:00:00Z')", "midnight"),
            Test("getTimeOfDay('2018-03-15T08:00:00Z')", "morning"),
            Test("getTimeOfDay('2018-03-15T12:00:00Z')", "noon"),
            Test("getTimeOfDay('2018-03-15T13:00:00Z')", "afternoon"),
            Test("getTimeOfDay('2018-03-15T18:00:00Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T22:00:00Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T23:00:00Z')", "night"),
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
            Test("first(nestedItems).x", 1, new HashSet<string> { "nestedItems"}),
            Test("join(items,',')", "zero,one,two"),
            Test("join(createArray('a', 'b', 'c'), '.')", "a.b.c"),
            Test("join(foreach(items, item, item), ',')", "zero,one,two"),
            Test("join(foreach(nestedItems, i, i.x + first(nestedItems).x), ',')", "2,3,4", new HashSet<string>{ "nestedItems"}),
            Test("join(foreach(items, item, concat(item, string(count(items)))), ',')", "zero3,one3,two3", new HashSet<string>{ "items"}),
            Test("last(items)", "two"),
            Test("last('hello')", "o"),
            Test("last(createArray(0, 1, 2))", 2),
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
            Test("property(bag, concat('na','me'))","mybag"),
            Test("items[2]", "two", new HashSet<string> { "items[2]" }),
            Test("bag.list[bag.index - 2]", "blue", new HashSet<string> {"bag.list", "bag.index" }),
            Test("bag['name']","mybag"),
            Test("bag[substring(concat('na','me','more'), 0, length('name'))]","mybag"),
            Test("items[1+1]","two"),
            # endregion
        };

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Evaluate(string input, object expected, HashSet<string> expectedRefs)
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
                    list = new[] { "red", "blue" },
                    index = 3,
                    name = "mybag"
                },
                items = new string[] { "zero", "one", "two" },
                nestedItems = new []
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
        

        public static IEnumerable<object[]> JsonData => new[]
        {
            //Test("exist(one)", true),
            Test("items[0] == 'item1'", true),
            // Test("'item1' == items[0]", false), // false because string.CompareTo(JValue) will get exception
        };

        [DataTestMethod]
        [DynamicData(nameof(JsonData))]
        public void EvaluateJSON(string input, object expected, HashSet<string> expectedRefs)
        {
            var scope = JsonConvert.DeserializeObject(@"{
                            'one': 1,
                            'two': 2,
                            'hello': 'hello',
            
                            'items': ['item1', 'item2', 'item3']
                        }");

            var parsed = new ExpressionEngine().Parse(input);
            var (actual, error) = parsed.TryEvaluate(scope);
            AssertObjectEquals(expected, actual);
        }

        private void AssertObjectEquals(object expected, object actual)
        {
            // Compare two lists
            if (expected is IList expectedList
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
