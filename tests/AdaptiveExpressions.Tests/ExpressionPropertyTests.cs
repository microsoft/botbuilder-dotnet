// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1602 // Enumeration items should be documented
#pragma warning disable SA1201 // Elements should appear in the correct order

using System.Collections;
using System.Collections.Generic;
using AdaptiveExpressions;
using AdaptiveExpressions.Converters;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class ExpressionPropertyTests
    {
        private object data = new
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
            Obj = new { x = "yo", y = 42 }
        };

        [Fact]
        public void ExpressionPropertyTests_ValueTests()
        {
            TestExpressionPropertyWithValue<byte>("1", 1);
            TestExpressionPropertyWithValue<short>("2", 2);
            TestExpressionPropertyWithValue<ushort>("3", 3);
            TestExpressionPropertyWithValue<uint>("5", 5);
            TestExpressionPropertyWithValue<long>("6", 6);
            TestExpressionPropertyWithValue<ulong>("7", 7);
            TestExpressionPropertyWithValue<double>("3.1", 3.1D);
        }

        [Fact]
        public void ExpressionPropertyTests_BindingTests()
        {
            TestWithData(data);
        }

        [Fact]
        public void ExpressionPropertyTests_JObjectBindingTests()
        {
            TestWithData(JObject.FromObject(data));
        }

        private void TestWithData(object data)
        {
            TestExpressionPropertyWithValue<byte>("ByteNum", 1, data);
            TestExpressionPropertyWithValue<byte>("=ByteNum", 1, data);

            TestExpressionPropertyWithValue<short>("ShortNum", 2, data);
            TestExpressionPropertyWithValue<short>("=ShortNum", 2, data);

            TestExpressionPropertyWithValue<ushort>("UShortNum", 3, data);
            TestExpressionPropertyWithValue<ushort>("=UShortNum", 3, data);

            TestExpressionPropertyWithValue<uint>("UIntNum", 5, data);
            TestExpressionPropertyWithValue<uint>("=UIntNum", 5, data);

            TestExpressionPropertyWithValue<ulong>("ULongNum", 7, data);
            TestExpressionPropertyWithValue<ulong>("=ULongNum", 7, data);

            TestExpressionPropertyWithValue<double>("DoubleNum", 3.1D, data);
            TestExpressionPropertyWithValue<double>("=DoubleNum", 3.1D, data);

            var list = new List<string>() { "a", "b", "c" };
            TestExpressionPropertyWithValue<List<string>>("StrArr", list, data);
            TestExpressionPropertyWithValue<List<string>>("=StrArr", list, data);

            TestExpressionPropertyWithValue<List<string>>("createArray('a','b','c')", list, data);
            TestExpressionPropertyWithValue<List<string>>("=createArray('a','b','c')", list, data);
        }

        private void TestExpressionPropertyWithValue<T>(string value, T expected, object memory = null)
        {
            var ep = new ExpressionProperty<T>(value);
            var (result, error) = ep.TryGetValue(memory ?? new object());
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

            var ep = new ExpressionProperty<Foo>("test");
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

            var ep = new ExpressionProperty<Foo>(foo);
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

            var ep = new ExpressionProperty<Foo>(JObject.FromObject(foo));
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

            var json = JsonConvert.SerializeObject(new
            {
                Foo = "test"
            });
            var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<Foo>() }
            };

            var bar = JsonConvert.DeserializeObject<Blat>(json, settings);
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
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<Foo>() }
            };

            var bar = JsonConvert.DeserializeObject<Blat>(json, settings);
            Assert.Equal(typeof(Blat), bar.GetType());
            Assert.Equal(typeof(ExpressionProperty<Foo>), bar.Foo.GetType());
            var (foo, error) = bar.Foo.TryGetValue(state);
            Assert.Equal("Test", foo.Name);
            Assert.Equal(22, foo.Age);
        }

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
            var json = JsonConvert.SerializeObject(test, settings: settings);
            var test2 = JsonConvert.DeserializeObject<ImplicitCastTest>(json, settings: settings);
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
            Assert.Equal(str.ToString(), JsonConvert.DeserializeObject<StringExpression>(JsonConvert.SerializeObject(str, settings: settings), settings: settings).ToString());
            var (result, error) = str.TryGetValue(data);
            Assert.Equal("test", result);
            Assert.Null(error);

            str = new StringExpression("=test");
            Assert.Equal("=test", str.ExpressionText);
            Assert.Null(str.Value);
            Assert.Equal(str.ToString(), JsonConvert.DeserializeObject<StringExpression>(JsonConvert.SerializeObject(str, settings: settings), settings: settings).ToString());
            (result, error) = str.TryGetValue(data);
            Assert.Equal("joe", result);
            Assert.Null(error);

            str = new StringExpression("Hello ${test}");
            Assert.Equal("=`Hello ${test}`", str.ExpressionText);
            Assert.Null(str.Value);
            Assert.Equal(str.ToString(), JsonConvert.DeserializeObject<StringExpression>(JsonConvert.SerializeObject(str, settings: settings), settings: settings).ToString());
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

        [Fact]
        public void ExpressionPropertyTests_ValueExpression()
        {
            var data = new
            {
                test = new { x = 13 }
            };

            var val = new ValueExpression("test");
            Assert.Equal("=`test`", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<ValueExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.Equal("test", result);
            Assert.Null(error);

            val = new ValueExpression("=test");
            Assert.Equal("=test", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<ValueExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonConvert.SerializeObject(data.test), JsonConvert.SerializeObject(result));
            Assert.Null(error);

            val = new ValueExpression(data.test);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            Assert.Equal(JsonConvert.SerializeObject(data.test, settings), JsonConvert.DeserializeObject<ValueExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonConvert.SerializeObject(data.test), JsonConvert.SerializeObject(result));
            Assert.Null(error);

            val = new ValueExpression("Hello ${test.x}");
            Assert.Equal("=`Hello ${test.x}`", val.ExpressionText);
            Assert.Null(val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<ValueExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal("Hello 13", result);
            Assert.Null(error);

            // slashes are the chars
            val = new ValueExpression("c:\\test\\test\\test");
            (result, error) = val.TryGetValue(data);
            Assert.Equal("c:\\test\\test\\test", result);
            Assert.Null(error);

            // tabs are the chars
            val = new ValueExpression("c:\test\test\test");
            (result, error) = val.TryGetValue(data);
            Assert.Equal("c:\test\test\test", result);
            Assert.Null(error);

            // test backtick in valueExpression
            val = new ValueExpression("name `backtick");
            (result, error) = val.TryGetValue(data);
            Assert.Equal("name `backtick", result);
            Assert.Null(error);

            val = new ValueExpression("name \\`backtick");
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
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<BoolExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression("=true");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(bool), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<BoolExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression(true);
            Assert.Null(val.ExpressionText);
            Assert.True(val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<BoolExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.True(result);
            Assert.Null(error);

            val = new BoolExpression("=test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(bool), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<BoolExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
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

            var val = new EnumExpression<TestEnum>("three");
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=three");
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(TestEnum.Two, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>(TestEnum.Three);
            Assert.Null(val.ExpressionText);
            Assert.Equal(TestEnum.Three, val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(TestEnum.Three, result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("garbage");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(default(TestEnum), result);
            Assert.Null(error);

            val = new EnumExpression<TestEnum>("=sum(garbage)");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TestEnum), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(default(TestEnum), result);
            Assert.NotNull(error);
        }

        [Fact]
        public void ExpressionPropertyTests_IntExpression()
        {
            TestNumberExpression<IntExpression, int>(new IntExpression(), 13);
        }

        [Fact]
        public void ExpressionPropertyTests_FloatExpression()
        {
            TestNumberExpression<NumberExpression, double>(new NumberExpression(), 3.14D);
        }

        private void TestNumberExpression<TExpression, TValue>(TExpression val, TValue expected)
            where TExpression : ExpressionProperty<TValue>, new()
        {
            var data = new
            {
                test = expected
            };

            val.SetValue("test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue("=test");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue($"{expected}");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue($"={expected}");
            Assert.NotNull(val.ExpressionText);
            Assert.Equal(default(TValue), val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.Equal(expected, result);
            Assert.Null(error);

            val.SetValue(expected);
            Assert.Null(val.ExpressionText);
            Assert.Equal(expected, val.Value);
            Assert.Equal(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
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

            var val = new ObjectExpression<Foo>("test");
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>("=test");
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>(data.test);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(13, result.Age);
            Assert.Equal("joe", result.Name);
            Assert.Null(error);

            val = new ObjectExpression<Foo>(JObject.FromObject(data.test));
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

            var val = new ArrayExpression<string>("test.Strings");
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonConvert.SerializeObject(data.test.Strings, settings), JsonConvert.SerializeObject(result, settings: settings));
            Assert.Equal(data.test.Strings, result);

            val = new ArrayExpression<string>("=test.Strings");
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonConvert.SerializeObject(data.test.Strings, settings), JsonConvert.SerializeObject(result, settings: settings));
            Assert.Equal(data.test.Strings, result);

            val = new ArrayExpression<string>(data.test.Strings);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonConvert.SerializeObject(data.test.Strings, settings), JsonConvert.SerializeObject(result, settings: settings));
            Assert.Equal(data.test.Strings, result);

            val = new ArrayExpression<string>(data.test.Strings);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonConvert.SerializeObject(data.test.Strings, settings), JsonConvert.SerializeObject(result, settings: settings));
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

            var val = new ArrayExpression<Foo>("test.Objects");
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            var (result, error) = val.TryGetValue(data);
            Assert.Equal(data.test.Objects, result);

            val = new ArrayExpression<Foo>("=test.Objects");
            Assert.NotNull(val.ExpressionText);
            Assert.Null(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(data.test.Objects, result);

            val = new ArrayExpression<Foo>(data.test.Objects);
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonConvert.SerializeObject(data.test.Objects, settings), JsonConvert.SerializeObject(result, settings));

            val = new ArrayExpression<Foo>(JArray.FromObject(data.test.Objects));
            Assert.Null(val.ExpressionText);
            Assert.NotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.Equal(JsonConvert.SerializeObject(data.test.Objects, settings), JsonConvert.SerializeObject(result, settings));
        }

        private JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>()
                {
                     new StringExpressionConverter(),
                     new ValueExpressionConverter(),
                     new BoolExpressionConverter(),
                     new IntExpressionConverter(),
                     new NumberExpressionConverter(),
                     new ExpressionPropertyConverter<short>(),
                     new ExpressionPropertyConverter<ushort>(),
                     new ExpressionPropertyConverter<uint>(),
                     new ExpressionPropertyConverter<ulong>(),
                     new ExpressionPropertyConverter<long>(),
                     new ExpressionPropertyConverter<double>(),
                     new EnumExpressionConverter<TestEnum>(),
                }
        };

        private class Blat
        {
            public ExpressionProperty<Foo> Foo { get; set; }
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

        [JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
        public enum TestEnum
        {
            One,
            Two,
            Three
        }
    }
}
