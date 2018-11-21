using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.ComposableDialogs.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.ComposableDialogs.Tests
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public async Task TestNumber()
        {
            IDictionary<string, object> state = new ExpandoObject();
            var expression = new CSharpExpression("2 + 2");
            var result = await expression.Evaluate(state);
            Assert.AreEqual(4, result);
            Assert.IsInstanceOfType(result, typeof(int));
        }

        [TestMethod]
        public async Task TestString()
        {
            IDictionary<string, object> state = new ExpandoObject();
            state["result"] = "Joe";
            var expression = new CSharpExpression("State.result");
            var result = await expression.Evaluate(state);
            Assert.AreEqual("Joe", result);
            Assert.IsInstanceOfType(result, typeof(string));
        }

        [TestMethod]
        public async Task TestVarCheck()
        {
            IDictionary<string, object> state = new ExpandoObject();
            state["x"] = 5;
            var expression = new CSharpExpression("State.x > 3");
            var result = await expression.Evaluate(state);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task TestVarGet()
        {
            IDictionary<string, object> state = new ExpandoObject();
            state["x"] = 5;
            var expression = new CSharpExpression("State.x");
            var result = await expression.Evaluate(state);
            Assert.IsInstanceOfType(result, typeof(int));
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task TestPath()
        {
            dynamic state = new ExpandoObject();

            state.person = "Joe";
            var expression = new CSharpExpression("State.person.Length");
            var result = await expression.Evaluate(state);
            Assert.IsInstanceOfType(result, typeof(int));
            Assert.AreEqual(3, result);
        }
    }

    public class Name
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName {  get { return $"{FirstName} {LastName}"; } }
    }

    public  class Person
    {
        public Name Name { get; set; }


    }
}
