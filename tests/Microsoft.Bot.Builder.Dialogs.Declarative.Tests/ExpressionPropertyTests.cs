// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    public class Bar
    {
        public Bar()
        {
        }

        public ExpressionProperty<Foo> Foo { get; set; }
    }

    public class Foo
    {
        public Foo()
        {
        }

        public string Name { get; set; }

        public int Age { get; set; }
    }

    [TestClass]
    public class ExpressionPropertyTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestExpressionAccess()
        {
            var state = new
            {
                test = new Foo()
                {
                    Name = "Test",
                    Age = 22
                }
            };

            var ep = new ExpressionProperty<Foo>("test");
            var result = ep.GetValue(state);
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(22, result.Age);
        }

        [TestMethod]
        public void TestValueAccess()
        {
            var foo = new Foo()
            {
                Name = "Test",
                Age = 22
            };

            var ep = new ExpressionProperty<Foo>(foo);
            var result = ep.GetValue(new object());
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(22, result.Age);
        }

        [TestMethod]
        public void TestJObjectAccess()
        {
            var foo = new Foo()
            {
                Name = "Test",
                Age = 22
            };

            var ep = new ExpressionProperty<Foo>(JObject.FromObject(foo));
            var result = ep.GetValue(new object());
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(22, result.Age);
        }

        [TestMethod]
        public void TestConverterExpressionAccess()
        {
            var state = new
            {
                test = new Foo()
                {
                    Name = "Test",
                    Age = 22
                }
            };

            var json = JsonConvert.SerializeObject(new
            {
                Foo = "test"
            });
            var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<ExpressionProperty<Foo>>() }
            };

            var bar = JsonConvert.DeserializeObject<Bar>(json, settings);
            Assert.AreEqual(typeof(Bar), bar.GetType());
            Assert.AreEqual(typeof(ExpressionProperty<Foo>), bar.Foo.GetType());
            var foo = bar.Foo.GetValue(state);
            Assert.AreEqual("Test", foo.Name);
            Assert.AreEqual(22, foo.Age);
        }

        [TestMethod]
        public void TestConverterObjectAccess()
        {
            var state = new
            {
            };

            var json = JsonConvert.SerializeObject(new
            {
                Foo = new
                {
                    Name = "Test",
                    Age = 22
                }
            });
            var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<ExpressionProperty<Foo>>() }
            };

            var bar = JsonConvert.DeserializeObject<Bar>(json, settings);
            Assert.AreEqual(typeof(Bar), bar.GetType());
            Assert.AreEqual(typeof(ExpressionProperty<Foo>), bar.Foo.GetType());
            var foo = bar.Foo.GetValue(state);
            Assert.AreEqual("Test", foo.Name);
            Assert.AreEqual(22, foo.Age);
        }
    }
}
