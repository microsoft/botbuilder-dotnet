// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1602 // Enumeration items should be documented
#pragma warning disable SA1201 // Elements should appear in the correct order

using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Bot.AdaptiveExpressions.Core;
using Microsoft.Bot.AdaptiveExpressions.Core.Converters;
using Microsoft.Bot.AdaptiveExpressions.Core.Properties;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
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
            TestWithData(data);
        }

        [Fact]
        public void ExpressionPropertyTests_JObjectBindingTests()
        {
            TestWithData(JsonSerializer.SerializeToNode(data, TestSerializerContext.Default.Anonymous3));
        }

        private void TestWithData(object data)
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

        private void TestExpressionPropertyWithValue<T>(string value, T expected, object memory, JsonTypeInfo typeInfo)
        {
            var ep = new ExpressionProperty<T>(value, typeInfo);
#if AOT
#pragma warning disable IL2026, IL3050 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            var (result, error) = ep.TryGetValue(new SimpleObjectMemory(memory ?? new object(), TestSerializerContext.Default));
#pragma warning restore IL2026, IL3050 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#else
            var (result, error) = ep.TryGetValue(memory ?? new object());
#endif
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
            var state = new
            {
                test = new Foo()
                {
                    Name = "Test",
                    Age = 22
                }
            };

            var ep = new ExpressionProperty<Foo>("test", TestSerializerContext.Default.Foo);
            var (result, error) = ep.TryGetValue(state);
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
            var (result, error) = ep.TryGetValue(new object());
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
            var (result, error) = ep.TryGetValue(new object());
            Assert.Equal("Test", result.Name);
            Assert.Equal(22, result.Age);
        }

        [Fact]
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

            var json = JsonSerializer.Serialize(new Anonymous1 { Foo = "test" }, TestSerializerContext.Default.Anonymous1);

            var bar = JsonSerializer.Deserialize<Blat>(json, TestSerializerContext.Default.Blat);
            Assert.Equal(typeof(Blat), bar.GetType());
            Assert.Equal(typeof(ExpressionProperty<Foo>), bar.Foo.GetType());
            var (foo, error) = bar.Foo.TryGetValue(state);
            Assert.Equal("Test", foo.Name);
            Assert.Equal(22, foo.Age);
        }

        [Fact]
        public void TestConverterObjectAccess()
        {
            var state = new
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
            var (foo, error) = bar.Foo.TryGetValue(state);
            Assert.Equal("Test", foo.Name);
            Assert.Equal(22, foo.Age);
        }

#if !AOT
        [Fact]
        public void ExpressionPropertyTests_TestImplicitCasts()
        {
            var data = new object();

            // test implicit casts as string
            var test = new ImplicitCastTest()
            {
                Str = "test",
                Int = "13",
                Number = "3.14",
                Enm = "two",
                Bool = "true",
                Strings = new string[] { "one", "two", "three" }
            };

            Assert.Equal("test", test.Str.TryGetValue(data).Value);
            Assert.Equal(13, test.Int.TryGetValue(data).Value);
            Assert.Equal(3.14D, test.Number.TryGetValue(data).Value);
            Assert.Equal(TestEnum.Two, test.Enm.TryGetValue(data).Value);
            Assert.True(test.Bool.TryGetValue(data).Value);
            Assert.Equal("one", test.Strings.TryGetValue(data).Value[0]);
            Assert.Equal("two", test.Strings.TryGetValue(data).Value[1]);
            Assert.Equal("three", test.Strings.TryGetValue(data).Value[2]);

            // Test expressions with =
            test.Str = "='test2'";
            test.Int = "=113";
            test.Number = "=13.14";
            test.Enm = "=three";
            test.Bool = "=true";
            test.Strings = "=createArray('a','b','c')";

            Assert.Equal("test2", test.Str.TryGetValue(data).Value);
            Assert.Equal(113, test.Int.TryGetValue(data).Value);
            Assert.Equal(13.14D, test.Number.TryGetValue(data).Value);
            Assert.Equal(TestEnum.Three, test.Enm.TryGetValue(data).Value);
            Assert.True(test.Bool.TryGetValue(data).Value);
            Assert.Equal("a", test.Strings.TryGetValue(data).Value[0]);
            Assert.Equal("b", test.Strings.TryGetValue(data).Value[1]);
            Assert.Equal("c", test.Strings.TryGetValue(data).Value[2]);

            // test serialization
            var json = JsonSerializer.Serialize(test, TestSerializerContext.Default.ImplicitCastTest);
            var test2 = JsonSerializer.Deserialize(json, TestSerializerContext.Default.ImplicitCastTest);
            Assert.Equal("test2", test2.Str.TryGetValue(data).Value);
            Assert.Equal(113, test2.Int.TryGetValue(data).Value);
            Assert.Equal(13.14D, test2.Number.TryGetValue(data).Value);
            Assert.Equal(TestEnum.Three, test2.Enm.TryGetValue(data).Value);
            Assert.True(test2.Bool.TryGetValue(data).Value);
            Assert.Equal("a", test2.Strings.TryGetValue(data).Value[0]);
            Assert.Equal("b", test2.Strings.TryGetValue(data).Value[1]);
            Assert.Equal("c", test2.Strings.TryGetValue(data).Value[2]);

            // Test constant expressions.
            test.Str = Expression.ConstantExpression("test2");
            test.Int = Expression.ConstantExpression(113);
            test.Number = Expression.ConstantExpression(13.14);
            test.Enm = Expression.ConstantExpression(TestEnum.Three);
            test.Bool = Expression.ConstantExpression(true);

            Assert.Equal("test2", test.Str.TryGetValue(data).Value);
            Assert.Equal(113, test.Int.TryGetValue(data).Value);
            Assert.Equal(13.14D, test.Number.TryGetValue(data).Value);
            Assert.Equal(TestEnum.Three, test.Enm.TryGetValue(data).Value);
            Assert.True(test.Bool.TryGetValue(data).Value);

            // Test Lamda expressions.
            test.Str = Expression.Lambda((data) => "test2");
            test.Int = Expression.Lambda((data) => 113);
            test.Number = Expression.Lambda((data) => 13.14);
            test.Enm = Expression.Lambda((data) => TestEnum.Three);
            test.Bool = Expression.Lambda((data) => true);

            Assert.Equal("test2", test.Str.TryGetValue(data).Value);
            Assert.Equal(113, test.Int.TryGetValue(data).Value);
            Assert.Equal(13.14D, test.Number.TryGetValue(data).Value);
            Assert.Equal(TestEnum.Three, test.Enm.TryGetValue(data).Value);
            Assert.True(test.Bool.TryGetValue(data).Value);

            // Test func expressions.
            test.Str = new StringExpression(data => "test2");
            test.Int = new IntExpression(data => 113);
            test.Number = new NumberExpression(data => 13.14);
            test.Enm = new EnumExpression<TestEnum>(data => TestEnum.Three);
            test.Bool = new BoolExpression(data => true);

            Assert.Equal("test2", test.Str.TryGetValue(data).Value);
            Assert.Equal(113, test.Int.TryGetValue(data).Value);
            Assert.Equal(13.14D, test.Number.TryGetValue(data).Value);
            Assert.Equal(TestEnum.Three, test.Enm.TryGetValue(data).Value);
            Assert.True(test.Bool.TryGetValue(data).Value);

            // test null assignment
            var testNull = new ImplicitCastTest()
            {
                Str = default(string),
                Int = default(int),
                Number = default(float),
                Enm = default(TestEnum),
                Bool = default(bool),
                Strings = default(string[])
            };
            Assert.Equal(default(string), testNull.Str.TryGetValue(data).Value);
            Assert.Equal(default(int), testNull.Int.TryGetValue(data).Value);
            Assert.Equal(default(float), testNull.Number.TryGetValue(data).Value);
            Assert.Equal(default(TestEnum), testNull.Enm.TryGetValue(data).Value);
            Assert.Equal(default(bool), testNull.Bool.TryGetValue(data).Value);
            Assert.Equal(default(string[]), testNull.Strings.TryGetValue(data).Value);
        }
#endif

        [Fact]
        public void ExpressionPropertyTests_StringExpression()
        {
            var data = new
            {
                test = "joe"
            };
            var str = new StringExpression("test");
            Assert.Equal("=`test`", str.ExpressionText);
            Assert.Null(str.Value);
            Assert.Equal(str.ToString(), RoundTripSerialize(str, TestSerializerContext.Default.StringExpression).ToString());
            var (result, error) = str.TryGetValue(data);
            Assert.Equal("test", result);
            Assert.Null(error);

            str = new StringExpression("=test");
            Assert.Equal("=test", str.ExpressionText);
            Assert.Null(str.Value);
            Assert.Equal(str.ToString(), RoundTripSerialize(str, TestSerializerContext.Default.StringExpression).ToString());
            (result, error) = str.TryGetValue(data);
            Assert.Equal("joe", result);
            Assert.Null(error);

            str = new StringExpression("Hello ${test}");
            Assert.Equal("=`Hello ${test}`", str.ExpressionText);
            Assert.Null(str.Value);
            Assert.Equal(str.ToString(), RoundTripSerialize(str, TestSerializerContext.Default.StringExpression).ToString());
            (result, error) = str.TryGetValue(data);
            Assert.Equal("Hello joe", result);
            Assert.Null(error);

            // slashes are the chars
            str = new StringExpression("c:\\test\\test\\test");
            (result, error) = str.TryGetValue(data);
            Assert.Equal("c:\\test\\test\\test", result);
            Assert.Null(error);

            // tabs are the chars
            str = new StringExpression("c:\test\test\test");
            (result, error) = str.TryGetValue(data);
            Assert.Equal("c:\test\test\test", result);
            Assert.Null(error);

            // test backtick in stringExpression
            str = new StringExpression("test `name");
            Assert.Equal("test `name", str.TryGetValue(data).Value);

            str = new StringExpression("test //`name");
            Assert.Equal("test //`name", str.TryGetValue(data).Value);
        }

        private T RoundTripSerialize<T>(T value, JsonTypeInfo<T> typeInfo)
        {
            return JsonSerializer.Deserialize(JsonSerializer.Serialize(value, typeInfo), typeInfo);
        }

        [Fact]
        public void ExpressionPropertyTests_ValueExpression()
        {
            var data = new
            {
                test = new Anonymous2 { x = 13 }
            };

            var val = new ValueExpression("test", TestSerializerContext.Default.Object);
            Assert.Equal("=`test`", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.ValueExpression).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.Equal("test", result);
            Assert.Null(error);

            val = new ValueExpression("=test", TestSerializerContext.Default.Object);
            Assert.Equal("=test", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.ValueExpression).ToString());
            (result, error) = val.TryGetValue(data);
#pragma warning disable IL2026, IL3050
            Assert.Equal(JsonSerializer.Serialize(data.test, TestSerializerContext.Default.Anonymous2), JsonSerializer.Serialize(result, TestSerializerContext.Default.Options));
#pragma warning restore IL2026, IL3050
            Assert.Null(error);

            val = new ValueExpression(data.test, TestSerializerContext.Default.Object);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            Assert.Equal(JsonSerializer.Serialize(data.test, TestSerializerContext.Default.Anonymous2), RoundTripSerialize(val, TestSerializerContext.Default.ValueExpression).ToString());
            (result, error) = val.TryGetValue(data);
#pragma warning disable IL2026, IL3050
            Assert.Equal(JsonSerializer.Serialize(data.test, TestSerializerContext.Default.Anonymous2), JsonSerializer.Serialize(result, TestSerializerContext.Default.Options));
#pragma warning restore IL2026, IL3050
            Assert.Null(error);

            val = new ValueExpression("Hello ${test.x}", TestSerializerContext.Default.Object);
            Assert.Equal("=`Hello ${test.x}`", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.ValueExpression).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal("Hello 13", result);
            Assert.Null(error);

            // slashes are the chars
            val = new ValueExpression("c:\\test\\test\\test", TestSerializerContext.Default.Object);
            (result, error) = val.TryGetValue(data);
            Assert.Equal("c:\\test\\test\\test", result);
            Assert.Null(error);

            // tabs are the chars
            val = new ValueExpression("c:\test\test\test", TestSerializerContext.Default.Object);
            (result, error) = val.TryGetValue(data);
            Assert.Equal("c:\test\test\test", result);
            Assert.Null(error);

            // test backtick in valueExpression
            val = new ValueExpression("name `backtick", TestSerializerContext.Default.Object);
            (result, error) = val.TryGetValue(data);
            Assert.Equal("name `backtick", result);
            Assert.Null(error);

            val = new ValueExpression("name \\`backtick", TestSerializerContext.Default.Object);
            (result, error) = val.TryGetValue(data);
            Assert.Equal("name \\`backtick", result);
            Assert.Null(error);
        }

        [Fact]
        public void ExpressionPropertyTests_BoolExpression()
        {
            var data = new
            {
                test = true
            };

            var val = new BoolExpression("true");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(bool), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.BoolExpression).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression("=true");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(bool), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.BoolExpression).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression(true);
            Assert.Null(val.ExpressionText);
            Assert.True(val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.BoolExpression).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression("=test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(bool), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.BoolExpression).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void ExpressionPropertyTests_EnumExpression()
        {
            var data = new
            {
                test = TestEnum.Two
            };

            var val = new EnumExpression<TestEnum>("three", TestSerializerContext.Default.TestEnum);
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=three", TestSerializerContext.Default.TestEnum);
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=test", TestSerializerContext.Default.TestEnum);
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(TestEnum.Two, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>(TestEnum.Three, TestSerializerContext.Default.TestEnum);
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("garbage", TestSerializerContext.Default.TestEnum);
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(default(TestEnum), result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=sum(garbage)", TestSerializerContext.Default.TestEnum);
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, TestSerializerContext.Default.EnumExpressionTestEnum).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(default(TestEnum), result);
            Assert.NotNull(error);
        }

        [Fact]
        public void ExpressionPropertyTests_IntExpression()
        {
            TestNumberExpression<IntExpression, int>(new IntExpression(), 13, TestSerializerContext.Default.IntExpression);
        }

        [Fact]
        public void ExpressionPropertyTests_FloatExpression()
        {
            TestNumberExpression<NumberExpression, double>(new NumberExpression(), 3.14D, TestSerializerContext.Default.NumberExpression);
        }

        private void TestNumberExpression<TExpression, TValue>(TExpression val, TValue expected, JsonTypeInfo<TExpression> typeInfo)
            where TExpression : ExpressionProperty<TValue>, new()
        {
            var data = new
            {
                test = expected
            };

            val.SetValue("test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue("=test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue($"{expected}");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue($"={expected}");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue(expected);
            Assert.Null(val.ExpressionText);
            Assert.Equal(expected, val.Value);
            Assert.Equal(val.ToString(), RoundTripSerialize(val, typeInfo).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);
        }

        [Fact]
        public void ExpressionPropertyTests_ObjectExpression()
        {
            var data = new
            {
                test = new Foo()
                {
                    Age = 13,
                    Name = "joe"
                }
            };

            var val = new ObjectExpression<Foo>("test", TestSerializerContext.Default.Foo);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>("=test", TestSerializerContext.Default.Foo);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>(data.test, TestSerializerContext.Default.Foo);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>(JsonSerializer.SerializeToNode(data.test, TestSerializerContext.Default.Foo), TestSerializerContext.Default.Foo);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);
        }

        [Fact]
        public void ExpressionPropertyTests_ArrayExpressionString()
        {
            var data = new
            {
                test = new ArrFoo()
                {
                    Strings = new List<string>()
                    {
                        "a", "b", "c"
                    }
                }
            };

            var val = new ArrayExpression<string>("test.Strings", TestSerializerContext.Default.ListString);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonSerializer.Serialize(data.test.Strings, TestSerializerContext.Default.ListString), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListString));
            Assert.Equal(data.test.Strings, result);

            val = new ArrayExpression<string>("=test.Strings", TestSerializerContext.Default.ListString);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonSerializer.Serialize(data.test.Strings, TestSerializerContext.Default.ListString), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListString));
            Assert.Equal(data.test.Strings, result);

            val = new ArrayExpression<string>(data.test.Strings, TestSerializerContext.Default.ListString);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonSerializer.Serialize(data.test.Strings, TestSerializerContext.Default.ListString), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListString));
            Assert.Equal(data.test.Strings, result);

            val = new ArrayExpression<string>(data.test.Strings, TestSerializerContext.Default.ListString);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonSerializer.Serialize(data.test.Strings, TestSerializerContext.Default.ListString), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListString));
            Assert.Equal(data.test.Strings, result);
        }

        [Fact]
        public void ExpressionPropertyTests_ArrayExpressionObject()
        {
            var data = new
            {
                test = new ArrFoo()
                {
                    Objects = new List<Foo>()
                    {
                        new Foo()
                        {
                            Age = 13,
                            Name = "joe"
                        }
                    }
                }
            };

            var val = new ArrayExpression<Foo>("test.Objects", TestSerializerContext.Default.ListFoo);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(data.test.Objects, result);

            val = new ArrayExpression<Foo>("=test.Objects", TestSerializerContext.Default.ListFoo);
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(data.test.Objects, result);

            val = new ArrayExpression<Foo>(data.test.Objects, TestSerializerContext.Default.ListFoo);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonSerializer.Serialize(data.test.Objects, TestSerializerContext.Default.ListFoo), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListFoo));

            val = new ArrayExpression<Foo>(JsonValue.Create(data.test.Objects, TestSerializerContext.Default.ListFoo), TestSerializerContext.Default.ListFoo);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonSerializer.Serialize(data.test.Objects, TestSerializerContext.Default.ListFoo), JsonSerializer.Serialize(result, TestSerializerContext.Default.ListFoo));
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

        private class Foo
        {
            public Foo()
            {
            }

            public string Name { get; set; }

            public int Age { get; set; }
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
