using System;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Flex Objects")]
    public class FlexObjectTests
    {
        [TestMethod]
        public void FlexObject_DynamicProperties()
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
        public void FlexObject_AccessDeclaredProperties()
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


        [TestMethod]
        public void FlexObject_DynamicPropertyAccess()
        {
            dynamic context = new Context();
            context.Test = "test";

            Assert.AreEqual(context.Test, "test");
        }

        [TestMethod]
        public void FlexObject_PropertyAccess()
        {
            var context = new Context();
            context.Name = "name";

            Assert.AreEqual(context.Name, "name");
        }


        [TestMethod]
        public void FlexObject_IndexPropertyAccess()
        {
            var context = new Context();
            context["Name"] = "name";

            Assert.AreEqual(context["Name"], "name");
            Assert.AreEqual(context["Name"], context.Name);
        }

        [TestMethod]
        public void FlexObject_SerializeDynamic()
        {
            dynamic context = new Context();
            context.Name = "name";
            context.Test = "test";

            var context2 = new Context();
            context2.Name = "name";
            context2["Test"] = "test";

            var json = JsonConvert.SerializeObject(context);
            var json2 = JsonConvert.SerializeObject(context2);
            Assert.AreEqual(json, json2, "expect dynamic and typed serialization to be the same");

            var context3 = JsonConvert.DeserializeObject<Context>(json);
            var context4 = JsonConvert.DeserializeObject<Context>(json2);
            Assert.AreEqual(context3.Name, context.Name, "typed should roundtrip");
            Assert.AreEqual(context3["Test"], context["Test"], "indexed should roundtrip");
            Assert.AreEqual(((dynamic)context3).Test, ((dynamic)context).Test, "indexed should roundtrip");

            Assert.AreEqual(context4.Name, context2.Name, "typed should roundtrip");
            Assert.AreEqual(context4["Test"], context2["Test"], "indexed should roundtrip");
            Assert.AreEqual(((dynamic)context4).Test, ((dynamic)context2).Test, "dynamic should roundtrip");
        }
    }
}
