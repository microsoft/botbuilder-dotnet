using System;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class FlexObjectTests
    {
        [TestMethod]
        public void DynamicContext_DynamicProperties()
        {
            var testValue = "testValue";
            var context = new FlexObject();
            context["test"] = testValue;
            Assert.AreEqual(context["test"], "testValue");
        }

        class Context : FlexObject
        {
            public string Name { get; set; }
        }

        class TestBinder : GetMemberBinder
        {
            public TestBinder(string name, bool ignoreCase) : base(name, ignoreCase)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void DynamicContext_AccessDeclaredProperties()
        {
            var testValue = "testValue";
            dynamic context = new Context();
            context.Name = testValue;
            context.Test = testValue;

            Assert.AreEqual(context.Name, context.Test);

            object value;
            Assert.IsTrue((context as Context).TryGetMember(new TestBinder("Name", false), out value));
            Assert.AreEqual(testValue, value as string);
        }


    }
}
