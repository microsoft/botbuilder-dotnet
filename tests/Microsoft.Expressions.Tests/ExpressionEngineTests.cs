using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Expressions.Tests
{
    [TestClass]
    public class ExpressionEngineTests
    {
        public static object[] Test(string input, object value, string[] paths = null) => new object[] { input, value, paths };

        public static IEnumerable<object[]> Data => new[]
       {
            Test("1 + 2", 3),
            Test("1.0 + 2.0", 3.0),
            Test("1 * 2 + 3", 5),
            Test("1 + 2 * 3", 7),
            Test("1 * (2 + 3)", 5),
            Test("(1 + 2) * 3", 9),
            Test("(one + two) * bag.three", 9.0, new string[] {"one", "two", "bag.three" }),
            Test("(one + two) * bag.set.four", 12.0, new string[] {"one", "two", "bag.set.four" } ),
            Test("(hello + ' ' + world)", "hello world", new string[] {"hello", "world" }),
            Test("items[2]", "two", new string[] { "items[2]" }),
            Test("bag.list[bag.index - 2]", "blue", new string[] {"bag.list", "bag.index" }),
            Test("bag.list[bag.index - 2] + 'more'", "bluemore", new string[] {"bag.list", "bag.index" }),
            Test("min(1.0, two) + max(one, 2.0)", 3.0, new string[]{ "two", "one" }),

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
            Test("greater(one, two)", false, new string[]{"one", "two" }),
            Test("greaterOrEquals(one, one)", true, new string[]{"one", "two" }),
            Test("greaterOrEquals(one, two)", false, new string[]{"one", "two" }),
            Test("less(5, 2)", false),
            Test("less(2, 2)", false),
            Test("less(one, two)", true, new string[]{"one", "two" }),
            Test("lessOrEquals(one, one)", true, new string[]{"one", "two" }),
            Test("lessOrEquals(one, two)", true, new string[]{"one", "two" }),

            Test("2^2", 4),
            Test("3^2^2", 81),
            Test("exp(2,2)", 4),

            Test("one > 0.5 && two < 2.5", true, new string[]{"one", "two" }),
            Test("one > 0.5 || two < 1.5", true, new string[]{"one", "two" }),

            Test("!one", false, new string[] {"one" }),
            Test("!!one", true, new string[] {"one" }),
            Test("!one || !!two", true, new string[]{"one", "two" }),
            Test("not(one)", false, new string[] {"one" }),
            Test("not(not(one))", true, new string[] {"one" }),
            Test("not(0)", true),

        };

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Evaluate(string input, object expected, string[] expectedRefs)
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
            if (expectedRefs == null)
            {
                expectedRefs = new string[0];
            }
            var parsed = new ExpressionEngine().Parse(input);
            Assert.IsNotNull(parsed);
            var (actual, msg) = parsed.TryEvaluate(null);
            Assert.AreEqual(expected, actual);
            var actualRefs = parsed.References();
            Assert.AreEqual(expectedRefs.Count(), actualRefs.Count());
            for (var i = 0; i < expectedRefs.Count(); ++i)
            {
                Assert.AreEqual(expectedRefs[i], actualRefs[i]);
            }
        }
    }
}
