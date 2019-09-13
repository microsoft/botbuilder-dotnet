using System.Collections.Generic;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Expressions.Tests
{
    [TestClass]
    public class RewriteTests
    {
        public static IEnumerable<object[]> Data => new[]
        {
            /*
            Test("woof.blah", "woof.blah"),
            Test("!woof.blah", "!woof.blah"),
            Test("!!woof.blah", "woof.blah"),

            // Comparisons
            Test("!(woof.blah < 3)", "woof.blah >= 3"),
            Test("!(woof.blah <= 3)", "woof.blah > 3"),
            Test("!(woof.blah == 3)", "woof.blah != 3"),
            Test("!(woof.blah != 3)", "woof.blah == 3"),
            Test("!(woof.blah >= 3)", "woof.blah < 3"),
            Test("!(woof.blah > 3)", "woof.blah <= 3"),

            // Logical
            Test("bark == 1 && bark == 2", "bark == 1 && bark == 2"),
            Test("bark == 1 || bark == 2", "bark == 1 || bark == 2"),
            Test("(bark == 1 || bark == 2) && (arg == 3 || arg == 4)", 
                "or(bark == 1 && arg == 3, bark == 1 && arg == 4, bark == 2 && arg == 3, bark == 2 && arg == 4)"),
            */
            Test("!(bark == 1 && bark == 2) && arg == 3", "(bark != 1 && arg == 3) || (bark != 2 && arg == 3)"),
            Test("!(bark == 1 && bark == 2)", "bark != 1 || bark != 2"),
            Test("!(bark == 1 || bark == 2)", "bark != 1 && bark != 2"),

            // ignore
            Test("!(ignore(bark == 3))", "ignore(bark != 3)")
        };

        public static object[] Test(string input, string expected) => new object[] { input, expected };

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Evaluate(string input, string expected)
        {
            var parser = new ExpressionEngine(Lookup);
            var original = parser.Parse(input);
            var dnf = original.DisjunctiveNormalForm();
            var expectedDnf = parser.Parse(expected);
            Assert.IsTrue(dnf.DeepEquals(expectedDnf), $"{original} is {dnf}, not {expectedDnf}");
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
                eval = BuiltInFunctions.Lookup(type);
            }

            return eval;
        }
    }
}
