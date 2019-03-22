using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Expressions.Tests
{
    [TestClass]
    public class ExpressionEngineTests
    {
        public static object[] Test(string input, object value) => new object[] { input, value };

        public static IEnumerable<object[]> Data => new[]
       {
            Test("1 + 2", 3),
            Test("1.0 + 2.0", 3.0),
            Test("1 * 2 + 3", 5),
            Test("1 + 2 * 3", 7),
            Test("1 * (2 + 3)", 5),
            Test("(1 + 2) * 3", 9),
            Test("(one + two) * bag.three", 9.0),
            Test("(one + two) * bag.set.four", 12.0),
            Test("(hello + ' ' + world)", "hello world"),
            Test("items[2]", "two"),
            Test("bag.list[bag.index - 2]", "blue"),
            Test("bag.list[bag.index - 2] + 'more'", "bluemore"),
            Test("min(1.0, two) + max(one, 2.0)", 3.0),

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
            Test("greater(one, two)", false),
            Test("greaterOrEquals(one, one)", true),
            Test("greaterOrEquals(one, two)", false),
            Test("less(5, 2)", false),
            Test("less(2, 2)", false),
            Test("less(one, two)", true),
            Test("lessOrEquals(one, one)", true),
            Test("lessOrEquals(one, two)", true),




            Test("2^2", 4),
            Test("3^2^2", 81),
            Test("exp(2,2)", 4),

            Test("one > 0.5 && two < 2.5", true),
            Test("one > 0.5 || two < 1.5", true),

            Test("!one", false),
            Test("!!one", true),
            Test("!one || !!two", true),
            Test("not(one)", false),
            Test("not(not(one))", true),
            Test("not(0)", true),

        };

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Parse(string input, object value)
        {
            var parsed = ExpressionEngine.Parse(input);
            Assert.IsNotNull(parsed);
        }

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Evaluate(string input, object expected)
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

            var parsed = ExpressionEngine.Parse(input);
            var actual = ExpressionEngine.Evaluate(parsed, scope);
            Assert.AreEqual(expected, actual);
        }
    }
}
