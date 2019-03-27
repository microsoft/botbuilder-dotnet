using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Expressions.Tests
{
    [TestClass]
    public class ExpressionEngineTests
    {
        public static object[] Test(string input, object value, HashSet<string> paths = null) => new object[] { input, value, paths };

        public static IEnumerable<object[]> Data => new[]
        {
            Test("1 + 2", 3),
            Test("1.0 + 2.0", 3.0),
            Test("1 * 2 + 3", 5),
            Test("1 + 2 * 3", 7),
            Test("1 * (2 + 3)", 5),
            Test("(1 + 2) * 3", 9),
            Test("(one + two) * bag.three", 9.0, new HashSet<string> {"one", "two", "bag.three" }),
            Test("(one + two) * bag.set.four", 12.0, new HashSet<string> {"one", "two", "bag.set.four" } ),
            Test("(hello + ' ' + world)", "hello world", new HashSet<string> {"hello", "world" }),
            Test("items[2]", "two", new HashSet<string> { "items[2]" }),
            Test("bag.list[bag.index - 2]", "blue", new HashSet<string> {"bag.list", "bag.index" }),
            Test("bag.list[bag.index - 2] + 'more'", "bluemore", new HashSet<string> {"bag.list", "bag.index" }),
            Test("min(1.0, two) + max(one, 2.0)", 3.0, new HashSet<string>{ "two", "one" }),

            // operator as functions tests
            Test("add(1, 2)", 3),
            Test("add(1.0, 2.0)", 3.0),
            Test("add(mul(1, 2), 3)", 5),
            Test("sub(2, 1)", 1),
            Test("sub(2.0, 0.5)", 1.5),
            Test("mul(2, 5)", 10),
            Test("div(mul(2, 5), 2)", 5),
            Test("div(5, 2)", 2),
            Test("greater(5, 2)", true),
            Test("greater(2, 2)", false),
            Test("greater(one, two)", false, new HashSet<string>{"one", "two" }),
            Test("greaterOrEquals(one, one)", true, new HashSet<string>{"one" }),
            Test("greaterOrEquals(one, two)", false, new HashSet<string>{"one", "two" }),
            Test("less(5, 2)", false),
            Test("less(2, 2)", false),
            Test("less(one, two)", true, new HashSet<string>{"one", "two" }),
            Test("lessOrEquals(one, one)", true, new HashSet<string>{"one" }),
            Test("lessOrEquals(one, two)", true, new HashSet<string>{"one", "two" }),

            Test("one > 0.5 && two < 2.5", true, new HashSet<string>{"one", "two" }),
            Test("one > 0.5 || two < 1.5", true, new HashSet<string>{"one", "two" }),

            Test("!(one == 1.0)", false, new HashSet<string> {"one" }),
            Test("!!(one == 1.0)", true, new HashSet<string> {"one" }),
            Test("!(one == 1.0) || !!(two == 2.0)", true, new HashSet<string>{"one", "two" }),
            Test("not(one == 1.0)", false, new HashSet<string> {"one" }),
            Test("not(not(one == 1.0))", true, new HashSet<string> {"one" }),
            Test("not(false)", true),
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
                    index = 3,
                    list = new[] { "red", "blue" }
                },
                items = new List<string> { "zero", "one", "two" }
            };
            if (expectedRefs == null)
            {
                expectedRefs = new HashSet<string>();
            }
            var parsed = new ExpressionEngine().Parse(input);
            Assert.IsNotNull(parsed);
            var (actual, msg) = parsed.TryEvaluate(scope);
            Assert.AreEqual(null, msg);
            Assert.AreEqual(expected, actual);
            var actualRefs = parsed.References();
            Assert.IsTrue(expectedRefs.SetEquals(actualRefs), "References do not match");
        }

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void TryEvaluate(string input, object expected)
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

            object actual = null;
            var success = ExpressionEngine.TryEvaluate(input, scope, out actual);
            Assert.IsTrue(success);
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<object[]> JsonData => new[]
        {
            //Test("exist(one)", true),
            Test("items[0] == 'item1'", true),
            // Test("'item1' == items[0]", false), // false because string.CompareTo(JValue) will get exception
        };

        [DataTestMethod]
        [DynamicData(nameof(JsonData))]
        public void EvaluateJSON(string input, object expected)
        {
            var scope = JsonConvert.DeserializeObject(@"{
                            'one': 1,
                            'two': 2,
                            'hello': 'hello',
            
                            'items': ['item1', 'item2', 'item3']
                        }");

            var parsed = ExpressionEngine.Parse(input);
            var actual = ExpressionEngine.Evaluate(parsed, scope);
            Assert.AreEqual(expected, actual);
        }
    }
}
