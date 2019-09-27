// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class Bar
    {
        public Bar()
        {
        }

        public ChoiceSet Choices { get; set; }
    }

    [TestClass]
    public class ChoiceSetTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestExpressionAccess()
        {
            var state = new
            {
                choices = new List<Choice>()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                }
            };

            var ep = new Adaptive.ChoiceSet("choices");
            var result = ep.GetValue(state);
            Assert.AreEqual("test1", result[0].Value);
            Assert.AreEqual("test2", result[1].Value);
            Assert.AreEqual("test3", result[2].Value);
        }

        [TestMethod]
        public void TestValueAccess()
        {
            var state = new object();
            var ep = new Adaptive.ChoiceSet(new List<Choice>()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                });
            var result = ep.GetValue(state);
            Assert.AreEqual("test1", result[0].Value);
            Assert.AreEqual("test2", result[1].Value);
            Assert.AreEqual("test3", result[2].Value);
        }

        [TestMethod]
        public void TestJObjectAccess()
        {
            var foo = new List<Choice>()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                };

            var ep = new Adaptive.ChoiceSet(JArray.FromObject(foo));
            var result = ep.GetValue(new object());
            Assert.AreEqual("test1", result[0].Value);
            Assert.AreEqual("test2", result[1].Value);
            Assert.AreEqual("test3", result[2].Value);
        }

        [TestMethod]
        public void TestStringArrayAccess()
        {
            var foo = new List<string>() { "test1", "test2", "test3" };
            var ep = new Adaptive.ChoiceSet(JArray.FromObject(foo));
            var result = ep.GetValue(new object());
            Assert.AreEqual("test1", result[0].Value);
            Assert.AreEqual("test2", result[1].Value);
            Assert.AreEqual("test3", result[2].Value);
        }

        [TestMethod]
        public void TestConverterExpressionAccess()
        {
            var state = new
            {
                test = new List<Choice>()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                }
            };

            var json = JsonConvert.SerializeObject(new
            {
                Choices = "test"
            });
            var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<ChoiceSet>() }
            };

            var bar = JsonConvert.DeserializeObject<Bar>(json, settings);
            Assert.AreEqual(typeof(Bar), bar.GetType());
            Assert.AreEqual(typeof(ChoiceSet), bar.Choices.GetType());
            var result = bar.Choices.GetValue(state);
            Assert.AreEqual("test1", result[0].Value);
            Assert.AreEqual("test2", result[1].Value);
            Assert.AreEqual("test3", result[2].Value);
        }

        [TestMethod]
        public void TestConverterObjectAccess()
        {
            var state = new
            {
            };

            var json = JsonConvert.SerializeObject(new
            {
                Choices = new List<Choice>()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                }
            });
            var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<ChoiceSet>() }
            };

            var bar = JsonConvert.DeserializeObject<Bar>(json, settings);
            Assert.AreEqual(typeof(Bar), bar.GetType());
            Assert.AreEqual(typeof(ChoiceSet), bar.Choices.GetType());
            var result = bar.Choices.GetValue(state);
            Assert.AreEqual("test1", result[0].Value);
            Assert.AreEqual("test2", result[1].Value);
            Assert.AreEqual("test3", result[2].Value);
        }

        [TestMethod]
        public void TestConverterStringAccess()
        {
            var state = new
            {
            };

            var json = JsonConvert.SerializeObject(new
            {
                Choices = new List<string>()
                {
                    "test1",
                    "test2",
                    "test3" 
                }
            });
            var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<ChoiceSet>() }
            };

            var bar = JsonConvert.DeserializeObject<Bar>(json, settings);
            Assert.AreEqual(typeof(Bar), bar.GetType());
            Assert.AreEqual(typeof(ChoiceSet), bar.Choices.GetType());
            var result = bar.Choices.GetValue(state);
            Assert.AreEqual("test1", result[0].Value);
            Assert.AreEqual("test2", result[1].Value);
            Assert.AreEqual("test3", result[2].Value);
        }
    }
}
