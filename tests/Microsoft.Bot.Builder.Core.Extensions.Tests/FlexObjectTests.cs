// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
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

        [TestMethod]
        public void FlexObject_JsonSimpleSeralizeFormat()
        {
            var flex = new FlexObject();
            flex["test"] = "testProperty";

            var json = JsonConvert.SerializeObject(flex);

            
            string targetJson = "{'test':'testProperty'}";
            JObject target = JObject.Parse(targetJson);
            JObject fromFlexObject = JObject.Parse(json);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match"); 
        }

        [TestMethod]
        public void FlexObject_JsonNestedDynamicSeralizeFormat()
        {
            var parent = new FlexObject();
            parent["test"] = "testProperty";

            var child = new FlexObject();
            child["prop1"] = "property1";

            parent["nested"] = child; 

            var parentJson = JsonConvert.SerializeObject(parent);

            string targetJson = @"
                {
                    'test' : 'testProperty',
                    'nested' : {
                        'prop1' : 'property1'
                    }
                }";

            JObject target = JObject.Parse(targetJson);
            JObject fromFlexObject = JObject.Parse(parentJson);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match");
        }

        public class Nested
        {
            public string Property1 { get; set; } = "one";
        }

        [TestMethod]
        public void FlexObject_JsonConcreteSeralizeFormat()
        {
            
            var parent = new FlexObject();
            parent["test"] = "testProperty";
            parent["nested"] = new Nested(); 
            
            var parentJson = JsonConvert.SerializeObject(parent);

            string correctJson = @"
                {
                    'test' : 'testProperty',
                    'nested' : {
                        'Property1' : 'one'
                    }
                }";

            JObject target = JObject.Parse(correctJson);
            JObject fromFlexObject = JObject.Parse(parentJson);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match");
        }

        
        public class NestedFlex : FlexObject
        {
            public string Property1 { get; set; } = "one";
        }

        [TestMethod]
        public void FlexObject_JsonMixedSeralizeFormat()
        {
            var parent = new NestedFlex();
            parent["test"] = "testProperty";            

            var parentJson = JsonConvert.SerializeObject(parent);

            string correctJson = @"
                {
                    'Property1' : 'one',
                    'test' : 'testProperty' 
                }";

            JObject target = JObject.Parse(correctJson);
            JObject fromFlexObject = JObject.Parse(parentJson);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match");
        }

        public class OverrideDefaultName : FlexObject
        {
            [JsonProperty("differentName")]
            public string Property1 { get; set; } = "one";
        }

        [TestMethod]
        public void FlexObject_OverrideDefaultNameViaAttribute()
        {
            var parent = new OverrideDefaultName();            
            var parentJson = JsonConvert.SerializeObject(parent);

            string correctJson = @"
                {
                    'differentName' : 'one'
                }";

            JObject target = JObject.Parse(correctJson);
            JObject fromFlexObject = JObject.Parse(parentJson);

            bool areSame = JToken.DeepEquals(target, fromFlexObject);
            Assert.IsTrue(areSame, "Json documents did not match");
        }
    }
}