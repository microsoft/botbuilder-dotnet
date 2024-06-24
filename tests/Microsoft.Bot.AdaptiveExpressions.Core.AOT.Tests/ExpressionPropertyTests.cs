// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1602 // Enumeration items should be documented
#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Bot.AdaptiveExpressions.Core;
using Microsoft.Bot.AdaptiveExpressions.Core.Converters;
using Microsoft.Bot.AdaptiveExpressions.Core.Memory;
using Microsoft.Bot.AdaptiveExpressions.Core.Properties;
using Xunit;

namespace Microsoft.Bot.AdaptiveExpressions.Core.AOT.Tests
{
    /// <summary>
    /// ExpressionProperty tests class.
    /// </summary>
    public partial class ExpressionPropertyTests
    {
        private Anonymous3 data = new Anonymous3
        {
            test = "hello",
            T = true,
            testEnum = TestEnum.Three,
            F = false,
            ByteNum = 1,
            ShortNum = 2,
            UShortNum = 3,
            IntNum = 4,
            UIntNum = 5,
            LongNum = 6,
            ULongNum = 7,
            FloatNum = 3.1F,
            DoubleNum = 3.1D,
            StrArr = new List<string>() { "a", "b", "c" },
            Obj = new Anonymous4 { x = "yo", y = 42 }
        };

        [Fact]
        public void ExpressionPropertyTests_ValueTests()
        {
            TestExpressionPropertyWithValue<byte>("1", 1, TestSerializerContext.Default.Byte);
            TestExpressionPropertyWithValue<short>("2", 2, TestSerializerContext.Default.Int16);
            TestExpressionPropertyWithValue<ushort>("3", 3, TestSerializerContext.Default.UInt16);
            TestExpressionPropertyWithValue<uint>("5", 5, TestSerializerContext.Default.UInt32);
            TestExpressionPropertyWithValue<long>("6", 6, TestSerializerContext.Default.Int64);
            TestExpressionPropertyWithValue<ulong>("7", 7, TestSerializerContext.Default.UInt64);
            TestExpressionPropertyWithValue<double>("3.1", 3.1D, TestSerializerContext.Default.Double);
        }

        [Fact]
        public void ExpressionPropertyTests_BindingTests()
        {
            TestWithData(JsonSerializer.SerializeToNode(data, TestSerializerContext.Default.Anonymous3));
        }

        [Fact]
        public void ExpressionPropertyTests_JObjectBindingTests()
        {
            TestWithData(JsonSerializer.SerializeToNode(data, TestSerializerContext.Default.Anonymous3));
        }

        private void TestWithData(JsonNode data)
        {
            TestExpressionPropertyWithValue<byte>("ByteNum", 1, data, TestSerializerContext.Default.Byte);
            TestExpressionPropertyWithValue<byte>("=ByteNum", 1, data, TestSerializerContext.Default.Byte);

            TestExpressionPropertyWithValue<short>("ShortNum", 2, data, TestSerializerContext.Default.Int16);
            TestExpressionPropertyWithValue<short>("=ShortNum", 2, data, TestSerializerContext.Default.Int16);

            TestExpressionPropertyWithValue<ushort>("UShortNum", 3, data, TestSerializerContext.Default.UInt16);
            TestExpressionPropertyWithValue<ushort>("=UShortNum", 3, data, TestSerializerContext.Default.UInt16);

            TestExpressionPropertyWithValue<uint>("UIntNum", 5, data, TestSerializerContext.Default.UInt32);
            TestExpressionPropertyWithValue<uint>("=UIntNum", 5, data, TestSerializerContext.Default.UInt32);

            TestExpressionPropertyWithValue<ulong>("ULongNum", 7, data, TestSerializerContext.Default.UInt64);
            TestExpressionPropertyWithValue<ulong>("=ULongNum", 7, data, TestSerializerContext.Default.UInt64);

            TestExpressionPropertyWithValue<double>("DoubleNum", 3.1D, data, TestSerializerContext.Default.Double);
            TestExpressionPropertyWithValue<double>("=DoubleNum", 3.1D, data, TestSerializerContext.Default.Double);

            var list = new List<string>() { "a", "b", "c" };
            TestExpressionPropertyWithValue<List<string>>("StrArr", list, data, TestSerializerContext.Default.ListString);
            TestExpressionPropertyWithValue<List<string>>("=StrArr", list, data, TestSerializerContext.Default.ListString);

            TestExpressionPropertyWithValue<List<string>>("createArray('a','b','c')", list, data, TestSerializerContext.Default.ListString);
            TestExpressionPropertyWithValue<List<string>>("=createArray('a','b','c')", list, data, TestSerializerContext.Default.ListString);
        }

        private void TestExpressionPropertyWithValue<T>(string value, T expected, JsonTypeInfo typeInfo)
        {
            TestExpressionPropertyWithValue(value, expected, null, typeInfo);
        }

        private void TestExpressionPropertyWithValue<T>(string value, T expected, JsonNode data, JsonTypeInfo typeInfo)
        {
            var ep = new ExpressionProperty<T>(value, typeInfo);
            var (result, error) = ep.TryGetValue(data, TestSerializerContext.Default);
            if (result is ICollection)
            {
                Assert.Equal((ICollection)expected, (ICollection)result);
            }
            else
            {
                Assert.Equal(expected, result);
            }

            Assert.Null(error);
        }

        [Fact]
        public void TestExpressionAccess()
        {
            var state = MakeJsonObject(
                "test",
                new Foo()
                {
                    Name = "Test",
                    Age = 22
                }, TestSerializerContext.Default.Foo);

            var ep = new ExpressionProperty<Foo>("test", TestSerializerContext.Default.Foo);
            var (result, error) = ep.TryGetValue(state, TestSerializerContext.Default);
            Assert.Equal("Test", result.Name);
            Assert.Equal(22, result.Age);
        }

        [Fact]
        public void TestValueAccess()
        {
            var foo = new Foo()
            {
                Name = "Test",
                Age = 22
            };

            var ep = new ExpressionProperty<Foo>(foo, TestSerializerContext.Default.Foo);
            var (result, error) = ep.TryGetValue(new JsonObject(), TestSerializerContext.Default);
            Assert.Equal("Test", result.Name);
            Assert.Equal(22, result.Age);
        }

        [Fact]
        public void TestJObjectAccess()
        {
            var foo = new Foo()
            {
                Name = "Test",
                Age = 22
            };

            var ep = new ExpressionProperty<Foo>(JsonSerializer.SerializeToNode(foo, TestSerializerContext.Default.Foo), TestSerializerContext.Default.Foo);
            var (result, error) = ep.TryGetValue(new JsonObject(), TestSerializerContext.Default);
            Assert.Equal("Test", result.Name);
            Assert.Equal(22, result.Age);
        }

        [Fact]
        public void TestConverterExpressionAccess()
        {
            var state = MakeJsonObject(
                "test",
                new Foo()
                {
                    Name = "Test",
                    Age = 22
                }, TestSerializerContext.Default.Foo);

            var json = JsonSerializer.Serialize(new Anonymous1 { Foo = "test" }, TestSerializerContext.Default.Anonymous1);

            var bar = JsonSerializer.Deserialize<Blat>(json, TestSerializerContext.Default.Blat);
            Assert.Equal(typeof(Blat), bar.GetType());
            Assert.Equal(typeof(ExpressionProperty<Foo>), bar.Foo.GetType());
            var (foo, error) = bar.Foo.TryGetValue(state, TestSerializerContext.Default);
            Assert.Equal("Test", foo.Name);
            Assert.Equal(22, foo.Age);
        }

        [Fact]
        public void TestConverterObjectAccess()
        {
            var state = new JsonObject
            {
            };

#pragma warning disable SA1116 // Split parameters should start on line after declaration
            var json = JsonSerializer.Serialize(new Anonymous5
            {
                Foo = new Anonymous6
                {
                    Name = "Test",
                    Age = 22
                }
            }, TestSerializerContext.Default.Anonymous5);
#pragma warning restore SA1116 // Split parameters should start on line after declaration

            var bar = JsonSerializer.Deserialize(json, TestSerializerContext.Default.Blat);
            Assert.Equal(typeof(Blat), bar.GetType());
            Assert.Equal(typeof(ExpressionProperty<Foo>), bar.Foo.GetType());
            var (foo, error) = bar.Foo.TryGetValue(state, TestSerializerContext.Default);
            Assert.Equal("Test", foo.Name);
            Assert.Equal(22, foo.Age);
        }

        [Fact]
        public void ExpressionPropertyTests_StringExpression()
        {
            var data = new JsonObject
            {
                { "test", "joe" }
            };
            var str = new StringExpression("test");
            Assert.Equal("=`test`", str.ExpressionText);
            Assert.Null(str.Value);
            Assert.Equal(str.ToString(), RoundTripSerialize(str, TestSerializerContext.Default.StringExpression).ToString());
            var (result, error) = str.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("test", result);
            Assert.Null(error);

            str = new StringExpression("=test");
            Assert.Equal("=test", str.ExpressionText);
            Assert.Null(str.Value);
            Assert.Equal(str.ToString(), RoundTripSerialize(str, TestSerializerContext.Default.StringExpression).ToString());
            (result, error) = str.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("joe", result);
            Assert.Null(error);

            str = new StringExpression("Hello ${test}");
            Assert.Equal("=`Hello ${test}`", str.ExpressionText);
            Assert.Null(str.Value);
            Assert.Equal(str.ToString(), RoundTripSerialize(str, TestSerializerContext.Default.StringExpression).ToString());
            (result, error) = str.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("Hello joe", result);
            Assert.Null(error);

            // slashes are the chars
            str = new StringExpression("c:\\test\\test\\test");
            (result, error) = str.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("c:\\test\\test\\test", result);
            Assert.Null(error);

            // tabs are the chars
            str = new StringExpression("c:\test\test\test");
            (result, error) = str.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("c:\test\test\test", result);
            Assert.Null(error);

            // test backtick in stringExpression
            str = new StringExpression("test `name");
            Assert.Equal("test `name", str.TryGetValue(data, TestSerializerContext.Default).Value);

            str = new StringExpression("test //`name");
            Assert.Equal("test //`name", str.TryGetValue(data, TestSerializerContext.Default).Value);
        }

        private T RoundTripSerialize<T>(T value, JsonTypeInfo<T> typeInfo)
        {
            return JsonSerializer.Deserialize(JsonSerializer.Serialize(value, typeInfo), typeInfo);
        }

        [Fact]
        public void ExpressionPropertyTests_ValueExpression()
        {
            var data = MakeJsonObject("test", new Anonymous2 { x = 13 }, TestSerializerContext.Default.Anonymous2);

            var val = new ValueExpression("test", TestSerializerContext.Default.Object);
            Assert.Equal("=`test`", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.ValueExpression).ToString());
            var (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("test", result);
            Assert.Null(error);

            val = new ValueExpression("=test", TestSerializerContext.Default.Object);
            Assert.Equal("=test", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.ValueExpression).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(data["test"].ToString(), result.ToString());
            Assert.Null(error);

            val = new ValueExpression(data["test"], TestSerializerContext.Default.Object);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            Assert.Equal(data["test"].ToString(), JsonSerializer.SerializeToNode(val, TestSerializerContext.Default.ValueExpression).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(data["test"].ToString(), result.ToString());
            Assert.Null(error);

            val = new ValueExpression("Hello ${test.x}", TestSerializerContext.Default.Object);
            Assert.Equal("=`Hello ${test.x}`", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), JsonSerializer.SerializeToNode(val, TestSerializerContext.Default.ValueExpression).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("Hello 13", result);
            Assert.Null(error);

            // slashes are the chars
            val = new ValueExpression("c:\\test\\test\\test", TestSerializerContext.Default.Object);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("c:\\test\\test\\test", result);
            Assert.Null(error);

            // tabs are the chars
            val = new ValueExpression("c:\test\test\test", TestSerializerContext.Default.Object);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("c:\test\test\test", result);
            Assert.Null(error);

            // test backtick in valueExpression
            val = new ValueExpression("name `backtick", TestSerializerContext.Default.Object);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("name `backtick", result);
            Assert.Null(error);

            val = new ValueExpression("name \\`backtick", TestSerializerContext.Default.Object);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal("name \\`backtick", result);
            Assert.Null(error);
        }

        [Fact]
        public void ExpressionPropertyTests_BoolExpression()
        {
            var data = MakeJsonObject("test", true, TestSerializerContext.Default.Boolean);

            var val = new BoolExpression("true");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(bool), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.BoolExpression).ToString());
            var (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression("=true");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(bool), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.BoolExpression).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression(true);
            Assert.Null(val.ExpressionText);
            Assert.True(val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.BoolExpression).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression("=test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(bool), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.BoolExpression).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void ExpressionPropertyTests_EnumExpression()
        {
            var data = MakeJsonObject("test", TestEnum.Two, TestSerializerContext.Default.TestEnum);

            var val = new EnumExpression<TestEnum>("three", TestSerializerContext.Default.TestEnum);
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            var (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=three", TestSerializerContext.Default.TestEnum);
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=test", TestSerializerContext.Default.TestEnum);
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(TestEnum.Two, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>(TestEnum.Three, TestSerializerContext.Default.TestEnum);
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("garbage", TestSerializerContext.Default.TestEnum);
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(default(TestEnum), result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=sum(garbage)", TestSerializerContext.Default.TestEnum);
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(default(TestEnum), result);
            Assert.NotNull(error);
        }

        [Fact]
        public void ExpressionPropertyTests_IntExpression()
        {
            TestNumberExpression<IntExpression, int>(new IntExpression(), 13, TestSerializerContext.Default.IntExpression, TestSerializerContext.Default.Int32);
        }

        [Fact]
        public void ExpressionPropertyTests_FloatExpression()
        {
            TestNumberExpression<NumberExpression, double>(new NumberExpression(), 3.14D, TestSerializerContext.Default.NumberExpression, TestSerializerContext.Default.Double);
        }

        private void TestNumberExpression<TExpression, TValue>(TExpression val, TValue expected, JsonTypeInfo<TExpression> typeInfo, JsonTypeInfo<TValue> valueTypeInfo)
            where TExpression : ExpressionProperty<TValue>, new()
        {
            var data = MakeJsonObject<TValue>("test", expected, valueTypeInfo);

            val.SetValue("test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            var (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue("=test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue($"{expected}");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue($"={expected}");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue(expected);
            Assert.Null(val.ExpressionText);
            Assert.Equal(expected, val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(expected, result);
            Assert.Null(error);
        }

        [Fact]
        public void ExpressionPropertyTests_ObjectExpression()
        {
            var data = MakeJsonObject(
                "test",
                new Foo()
                {
                    Age = 13,
                    Name = "joe"
                }, TestSerializerContext.Default.Foo);

            var val = new ObjectExpression<Foo>("test", TestSerializerContext.Default.Foo);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>("=test", TestSerializerContext.Default.Foo);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>(data["test"], TestSerializerContext.Default.Foo);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>(data["test"], TestSerializerContext.Default.Foo);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);
        }

        [Fact]
        public void ExpressionPropertyTests_ArrayExpressionString()
        {
            var arrFoo = new ArrFoo()
            {
                Strings = new List<string>()
                {
                    "a", "b", "c"
                }
            };
            var data = MakeJsonObject("test", arrFoo, TestSerializerContext.Default.ArrFoo);

            var val = new ArrayExpression<string>("test.Strings", TestSerializerContext.Default.ListString);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(JsonSerializer.Serialize(arrFoo.Strings, TestSerializerContext.Default.ListString), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListString));
            Assert.Equal(arrFoo.Strings, result);

            val = new ArrayExpression<string>("=test.Strings", TestSerializerContext.Default.ListString);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(JsonSerializer.Serialize(arrFoo.Strings, TestSerializerContext.Default.ListString), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListString));
            Assert.Equal(arrFoo.Strings, result);

            val = new ArrayExpression<string>(arrFoo.Strings, TestSerializerContext.Default.ListString);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(JsonSerializer.Serialize(arrFoo.Strings, TestSerializerContext.Default.ListString), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListString));
            Assert.Equal(arrFoo.Strings, result);

            val = new ArrayExpression<string>(arrFoo.Strings, TestSerializerContext.Default.ListString);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(JsonSerializer.Serialize(arrFoo.Strings, TestSerializerContext.Default.ListString), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListString));
            Assert.Equal(arrFoo.Strings, result);
        }

        [Fact]
        public void ExpressionPropertyTests_ArrayExpressionObject()
        {
            var testObj = new ArrFoo()
            {
                Objects = new List<Foo>()
                    {
                        new Foo()
                        {
                            Age = 13,
                            Name = "joe"
                        }
                    }
            };

            var data = MakeJsonObject("test", testObj, TestSerializerContext.Default.ArrFoo);

            var val = new ArrayExpression<Foo>("test.Objects", TestSerializerContext.Default.ListFoo);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(testObj.Objects, result);

            val = new ArrayExpression<Foo>("=test.Objects", TestSerializerContext.Default.ListFoo);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(testObj.Objects, result);

            val = new ArrayExpression<Foo>(testObj.Objects, TestSerializerContext.Default.ListFoo);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(JsonSerializer.Serialize(testObj.Objects, TestSerializerContext.Default.ListFoo), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListFoo));

            val = new ArrayExpression<Foo>(JsonValue.Create(testObj.Objects, TestSerializerContext.Default.ListFoo), TestSerializerContext.Default.ListFoo);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data, TestSerializerContext.Default);
            Assert.Equal(JsonSerializer.Serialize(testObj.Objects, TestSerializerContext.Default.ListFoo), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListFoo));
        }

        private JsonNode MakeJsonObject<T>(string property, T obj, JsonTypeInfo<T> typeinfo)
        {
            return new JsonObject
            {
                { property, JsonSerializer.SerializeToNode(obj, typeinfo) }
            };
        }

        [JsonSerializable(typeof(byte))]
        [JsonSerializable(typeof(bool))]
        [JsonSerializable(typeof(short))]
        [JsonSerializable(typeof(ushort))]
        [JsonSerializable(typeof(int))]
        [JsonSerializable(typeof(uint))]
        [JsonSerializable(typeof(Blat))]
        [JsonSerializable(typeof(Foo))]
        [JsonSerializable(typeof(Anonymous1))]
        [JsonSerializable(typeof(Anonymous2))]
        [JsonSerializable(typeof(Anonymous3))]
        [JsonSerializable(typeof(Anonymous4))]
        [JsonSerializable(typeof(Anonymous5))]
        [JsonSerializable(typeof(Anonymous6))]
        [JsonSerializable(typeof(ArrFoo))]
        [JsonSerializable(typeof(BoolExpression))]
        [JsonSerializable(typeof(IntExpression))]
        [JsonSerializable(typeof(NumberExpression))]
        [JsonSerializable(typeof(StringExpression))]
        [JsonSerializable(typeof(ValueExpression))]
        [JsonSerializable(typeof(ExpressionProperty<short>))]
        [JsonSerializable(typeof(ExpressionProperty<ushort>))]
        [JsonSerializable(typeof(ExpressionProperty<uint>))]
        [JsonSerializable(typeof(ExpressionProperty<ulong>))]
        [JsonSerializable(typeof(ExpressionProperty<long>))]
        [JsonSerializable(typeof(ExpressionProperty<double>))]
        [JsonSerializable(typeof(ExpressionProperty<TestEnum>))]
        [JsonSerializable(typeof(ExpressionProperty<Foo>))]
        [JsonSerializable(typeof(List<object>))]
        [JsonSerializable(typeof(List<string>))]
        [JsonSerializable(typeof(List<Foo>))]
        [JsonSerializable(typeof(ImplicitCastTest))]
        [JsonSerializable(typeof(TestEnum))]
        [JsonSerializable(typeof(JsonObject))]
        [JsonSerializable(typeof(JsonArray))]
        [JsonSourceGenerationOptions(
            WriteIndented = true,
#pragma warning disable SA1118 // Parameter should not span multiple lines
            Converters =
            [
                typeof(ExpressionPropertyConverter<short>),
                typeof(ExpressionPropertyConverter<ushort>),
                typeof(ExpressionPropertyConverter<uint>),
                typeof(ExpressionPropertyConverter<ulong>),
                typeof(ExpressionPropertyConverter<long>),
                typeof(ExpressionPropertyConverter<double>),
                typeof(ExpressionPropertyConverter<Foo>),
                typeof(EnumExpressionConverter<TestEnum>),
            ])]
#pragma warning restore SA1118 // Parameter should not span multiple lines
        private partial class TestSerializerContext : JsonSerializerContext
        {
        }

        private class Blat
        {
            public ExpressionProperty<Foo> Foo { get; set; }
        }

        private class Anonymous1
        {
            public string Foo { get; set; }
        }

        private class Anonymous2
        {
#pragma warning disable SA1300 // Element should begin with upper-case letter
            public int x { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
        }

        private class Foo : IComparable<Foo>
        {
            public Foo()
            {
            }

            public string Name { get; set; }

            public int Age { get; set; }

            public int CompareTo(Foo other)
            {
                if (Name.CompareTo(other.Name) is int result && result != 0)
                {
                    return result;
                }

                if (Age.CompareTo(other.Age) is int result2 && result2 != 0)
                {
                    return result;
                }

                return 0;
            }
        }

        private class ArrFoo
        {
            public List<Foo> Objects { get; set; }

            public List<string> Strings { get; set; }
        }

        public class ImplicitCastTest
        {
            public StringExpression Str { get; set; }

            public IntExpression Int { get; set; }

            public EnumExpression<TestEnum> Enm { get; set; }

            public NumberExpression Number { get; set; }

            public ValueExpression Value { get; set; }

            public BoolExpression Bool { get; set; }

            public ArrayExpression<string> Strings { get; set; }
        }

        //[JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
        public enum TestEnum
        {
            One,
            Two,
            Three
        }

#pragma warning disable SA1300, SA1516 // Element should begin with upper-case letter

        private class Anonymous3
        {
            public string test { get; set; }
            public bool T { get; set; }
            public TestEnum testEnum { get; set; }
            public bool F { get; set; }
            public int ByteNum { get; set; }
            public int ShortNum { get; set; }
            public int UShortNum { get; set; }
            public int IntNum { get; set; }
            public int UIntNum { get; set; }
            public int LongNum { get; set; }
            public int ULongNum { get; set; }
            public float FloatNum { get; set; }
            public double DoubleNum { get; set; }
            public List<string> StrArr { get; set; }
            public Anonymous4 Obj { get; set; }
        }

        private class Anonymous4
        {
            public string x { get; set; }
            public int y { get; set; }
        }

        private class Anonymous5
        {
            public Anonymous6 Foo { get; set; }
        }

        private class Anonymous6
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

#pragma warning restore SA1300, SA1516 // Element should begin with upper-case letter

    }
}
