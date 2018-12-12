using System.Collections.Generic;
using System.Linq;
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
        };

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Tokenize(string input, object value)
        {
            var actual = Lexer.Tokens(input).Select(t => t.Input).ToArray();
            Assert.AreNotEqual(0, actual.Length);
        }

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
                },
            };

            var parsed = ExpressionEngine.Parse(input);
            var actual = ExpressionEngine.Evaluate(parsed, scope);
            Assert.AreEqual(expected, actual);
        }
    }
}
