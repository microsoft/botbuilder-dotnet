// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name

using System.Collections.Generic;
using AdaptiveExpressions.Converters;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class Bar
    {
        public Bar()
        {
        }

        public ObjectExpression<ChoiceSet> Choices { get; set; }
    }

    [CollectionDefinition("Dialogs.Adaptive")]
    public class ChoiceSetTests
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new ChoiceSetConverter(),
                new ObjectExpressionConverter<ChoiceSet>()
            }
        };

        [Fact]
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

            var ep = new ObjectExpression<ChoiceSet>("choices");
            var (result, error) = ep.TryGetValue(state);
            Assert.Equal("test1", result[0].Value);
            Assert.Equal("test2", result[1].Value);
            Assert.Equal("test3", result[2].Value);
        }

        [Fact]
        public void TestValueAccess()
        {
            var state = new object();
            var ep = new ObjectExpression<ChoiceSet>(new ChoiceSet()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                });
            var (result, error) = ep.TryGetValue(state);
            Assert.Equal("test1", result[0].Value);
            Assert.Equal("test2", result[1].Value);
            Assert.Equal("test3", result[2].Value);
        }

        [Fact]
        public void TestJObjectAccess()
        {
            var foo = new List<Choice>()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                };

            var ep = new ObjectExpression<ChoiceSet>(new ChoiceSet(JArray.FromObject(foo)));
            var (result, error) = ep.TryGetValue(new object());
            Assert.Equal("test1", result[0].Value);
            Assert.Equal("test2", result[1].Value);
            Assert.Equal("test3", result[2].Value);
        }

        [Fact]
        public void TestStringArrayAccess()
        {
            var foo = new List<string>() { "test1", "test2", "test3" };
            var ep = new ObjectExpression<ChoiceSet>(new ChoiceSet(JArray.FromObject(foo)));
            var (result, error) = ep.TryGetValue(new object());
            Assert.Equal("test1", result[0].Value);
            Assert.Equal("test2", result[1].Value);
            Assert.Equal("test3", result[2].Value);
        }

        [Fact]
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

            var sample = new
            {
                Choices = "test"
            };
            var json = JsonConvert.SerializeObject(sample, settings);

            var bar = JsonConvert.DeserializeObject<Bar>(json, settings);
            Assert.Equal(typeof(Bar), bar.GetType());
            Assert.Equal(typeof(ObjectExpression<ChoiceSet>), bar.Choices.GetType());
            var (result, error) = bar.Choices.TryGetValue(state);
            Assert.Equal("test1", result[0].Value);
            Assert.Equal("test2", result[1].Value);
            Assert.Equal("test3", result[2].Value);
        }

        [Fact]
        public void TestConverterObjectAccess()
        {
            var state = new
            {
            };

            var sample = new
            {
                Choices = new List<Choice>()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                }
            };
            var json = JsonConvert.SerializeObject(sample, settings);

            var bar = JsonConvert.DeserializeObject<Bar>(json, settings);
            Assert.Equal(typeof(Bar), bar.GetType());
            Assert.Equal(typeof(ObjectExpression<ChoiceSet>), bar.Choices.GetType());
            var (result, error) = bar.Choices.TryGetValue(state);
            Assert.Equal("test1", result[0].Value);
            Assert.Equal("test2", result[1].Value);
            Assert.Equal("test3", result[2].Value);
        }

        [Fact]
        public void TestConverterStringAccess()
        {
            var state = new
            {
            };

            var sample = new
            {
                Choices = new List<string>()
                {
                    "test1",
                    "test2",
                    "test3"
                }
            };

            var json = JsonConvert.SerializeObject(sample, settings);

            var bar = JsonConvert.DeserializeObject<Bar>(json, settings);
            Assert.Equal(typeof(Bar), bar.GetType());
            Assert.Equal(typeof(ObjectExpression<ChoiceSet>), bar.Choices.GetType());
            var (result, error) = bar.Choices.TryGetValue(state);
            Assert.Equal("test1", result[0].Value);
            Assert.Equal("test2", result[1].Value);
            Assert.Equal("test3", result[2].Value);
        }

        [Fact]
        public void ChoiceSet_RoundTrip()
        {
            var foo = new ChoiceSet()
                {
                    new Choice() { Value = "test1" },
                    new Choice() { Value = "test2" },
                    new Choice() { Value = "test3" }
                };

            var bar = JsonConvert.DeserializeObject<ChoiceSet>(JsonConvert.SerializeObject(foo));
            for (var i = 0; i < foo.Count; i++)
            {
                Assert.Equal(foo[i].Value, bar[i].Value);
            }
        }

        [Fact]
        public void ChoiceSet_StringArray()
        {
            var values = new JArray
            {
                "test1",
                "test2",
                "test3"
            };

            var result = JsonConvert.DeserializeObject<ChoiceSet>(values.ToString());
            Assert.Equal("test1", result[0].Value);
            Assert.Equal("test2", result[1].Value);
            Assert.Equal("test3", result[2].Value);
        }
    }
}
