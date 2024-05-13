using System.Collections.Generic;
using Xunit;

namespace AdaptiveExpressions.Tests
{
    public class RewriteTests
    {
        public static IEnumerable<object[]> Data => new[]
        {
            Test("!(bark == 1 && bark == 2) && arg == 3", "(bark != 1 && arg == 3) || (bark != 2 && arg == 3)"),
            Test("!(bark == 1 && bark == 2)", "bark != 1 || bark != 2"),
            Test("!(bark == 1 || bark == 2)", "bark != 1 && bark != 2"),

            // ignore
            Test("!(ignore(bark == 3))", "ignore(bark != 3)")
        };

        public static object[] Test(string input, string expected) => new object[] { input, expected };

        [Theory]
        [MemberData(nameof(Data))]
        public void Evaluate(string input, string expected)
        {
            var original = Expression.Parse(input, Lookup);
            var dnf = original.DisjunctiveNormalForm();
            var expectedDnf = Expression.Parse(expected, Lookup);
            Assert.True(dnf.DeepEquals(expectedDnf));
        }

        private ExpressionEvaluator Lookup(string type)
        {
            ExpressionEvaluator eval;
            if (type == "ignore")
            {
                eval = new ExpressionEvaluator("ignore", null);
                eval.Negation = eval;
            }
            else
            {
                eval = Expression.Lookup(type);
            }

            return eval;
        }
    }
}
