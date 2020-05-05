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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class ExpressionPropertyTests
    {
        public TestContext TestContext { get; set; }

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

        [TestMethod]
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

        [TestMethod]
        public void ExpressionPropertyTests_BindingTests()
        {
            TestWithData(data);
        }

        [TestMethod]
        public void ExpressionPropertyTests_JObjectBindingTests()
        {
            TestWithData(JObject.FromObject(data));
        }

        public void TestWithData(object data)
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

        public void TestExpressionPropertyWithValue<T>(string value, T expected, object memory = null)
        {
            var ep = new ExpressionProperty<T>(value);
            var (result, error) = ep.TryGetValue(memory ?? new object());
            if (result is ICollection)
            {
                CollectionAssert.AreEqual((ICollection)expected, (ICollection)result);
            }
            else
            {
                Assert.AreEqual(expected, result);
            }

            Assert.IsNull(error);
        }

        public void TestErrorExpression<T>(string value, object memory = null)
        {
            var ep = new ExpressionProperty<T>(value);
            var (result, error) = ep.TryGetValue(memory ?? new object());
            Assert.IsNotNull(error);
        }

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
            var (result, error) = ep.TryGetValue(state);
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
            var (result, error) = ep.TryGetValue(new object());
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
            var (result, error) = ep.TryGetValue(new object());
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
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<Foo>() }
            };

            var bar = JsonConvert.DeserializeObject<Blat>(json, settings);
            Assert.AreEqual(typeof(Blat), bar.GetType());
            Assert.AreEqual(typeof(ExpressionProperty<Foo>), bar.Foo.GetType());
            var (foo, error) = bar.Foo.TryGetValue(state);
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
                Converters = new List<JsonConverter>() { new ExpressionPropertyConverter<Foo>() }
            };

            var bar = JsonConvert.DeserializeObject<Blat>(json, settings);
            Assert.AreEqual(typeof(Blat), bar.GetType());
            Assert.AreEqual(typeof(ExpressionProperty<Foo>), bar.Foo.GetType());
            var (foo, error) = bar.Foo.TryGetValue(state);
            Assert.AreEqual("Test", foo.Name);
            Assert.AreEqual(22, foo.Age);
        }

        [TestMethod]
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

            Assert.AreEqual("test", test.Str.TryGetValue(data).Value);
            Assert.AreEqual(13, test.Int.TryGetValue(data).Value);
            Assert.AreEqual(3.14F, test.Number.TryGetValue(data).Value);
            Assert.AreEqual(TestEnum.Two, test.Enm.TryGetValue(data).Value);
            Assert.AreEqual(true, test.Bool.TryGetValue(data).Value);
            Assert.AreEqual("one", test.Strings.TryGetValue(data).Value[0]);
            Assert.AreEqual("two", test.Strings.TryGetValue(data).Value[1]);
            Assert.AreEqual("three", test.Strings.TryGetValue(data).Value[2]);

            // Test expressions with =
            test.Str = "='test2'";
            test.Int = "=113";
            test.Number = "=13.14";
            test.Enm = "=three";
            test.Bool = "=true";
            test.Strings = "=createArray('a','b','c')";

            Assert.AreEqual("test2", test.Str.TryGetValue(data).Value);
            Assert.AreEqual(113, test.Int.TryGetValue(data).Value);
            Assert.AreEqual(13.14F, test.Number.TryGetValue(data).Value);
            Assert.AreEqual(TestEnum.Three, test.Enm.TryGetValue(data).Value);
            Assert.AreEqual(true, test.Bool.TryGetValue(data).Value);
            Assert.AreEqual("a", test.Strings.TryGetValue(data).Value[0]);
            Assert.AreEqual("b", test.Strings.TryGetValue(data).Value[1]);
            Assert.AreEqual("c", test.Strings.TryGetValue(data).Value[2]);

            // test serialization
            var json = JsonConvert.SerializeObject(test, settings: settings);
            var test2 = JsonConvert.DeserializeObject<ImplicitCastTest>(json, settings: settings);
            Assert.AreEqual("test2", test2.Str.TryGetValue(data).Value);
            Assert.AreEqual(113, test2.Int.TryGetValue(data).Value);
            Assert.AreEqual(13.14F, test2.Number.TryGetValue(data).Value);
            Assert.AreEqual(TestEnum.Three, test2.Enm.TryGetValue(data).Value);
            Assert.AreEqual(true, test2.Bool.TryGetValue(data).Value);
            Assert.AreEqual("a", test2.Strings.TryGetValue(data).Value[0]);
            Assert.AreEqual("b", test2.Strings.TryGetValue(data).Value[1]);
            Assert.AreEqual("c", test2.Strings.TryGetValue(data).Value[2]);

            // Test constant expressions.
            test.Str = Expression.ConstantExpression("test2");
            test.Int = Expression.ConstantExpression(113);
            test.Number = Expression.ConstantExpression(13.14);
            test.Enm = Expression.ConstantExpression(TestEnum.Three);
            test.Bool = Expression.ConstantExpression(true);

            Assert.AreEqual("test2", test.Str.TryGetValue(data).Value);
            Assert.AreEqual(113, test.Int.TryGetValue(data).Value);
            Assert.AreEqual(13.14F, test.Number.TryGetValue(data).Value);
            Assert.AreEqual(TestEnum.Three, test.Enm.TryGetValue(data).Value);
            Assert.AreEqual(true, test.Bool.TryGetValue(data).Value);

            // Test Lamda expressions.
            test.Str = Expression.Lambda((data) => "test2");
            test.Int = Expression.Lambda((data) => 113);
            test.Number = Expression.Lambda((data) => 13.14);
            test.Enm = Expression.Lambda((data) => TestEnum.Three);
            test.Bool = Expression.Lambda((data) => true);

            Assert.AreEqual("test2", test.Str.TryGetValue(data).Value);
            Assert.AreEqual(113, test.Int.TryGetValue(data).Value);
            Assert.AreEqual(13.14F, test.Number.TryGetValue(data).Value);
            Assert.AreEqual(TestEnum.Three, test.Enm.TryGetValue(data).Value);
            Assert.AreEqual(true, test.Bool.TryGetValue(data).Value);

            // Test func expressions.
            test.Str = new StringExpression(data => "test2");
            test.Int = new IntExpression(data => 113);
            test.Number = new NumberExpression(data => 13.14);
            test.Enm = new EnumExpression<TestEnum>(data => TestEnum.Three);
            test.Bool = new BoolExpression(data => true);

            Assert.AreEqual("test2", test.Str.TryGetValue(data).Value);
            Assert.AreEqual(113, test.Int.TryGetValue(data).Value);
            Assert.AreEqual(13.14F, test.Number.TryGetValue(data).Value);
            Assert.AreEqual(TestEnum.Three, test.Enm.TryGetValue(data).Value);
            Assert.AreEqual(true, test.Bool.TryGetValue(data).Value);

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
            Assert.AreEqual(default(string), testNull.Str.TryGetValue(data).Value);
            Assert.AreEqual(default(int), testNull.Int.TryGetValue(data).Value);
            Assert.AreEqual(default(float), testNull.Number.TryGetValue(data).Value);
            Assert.AreEqual(default(TestEnum), testNull.Enm.TryGetValue(data).Value);
            Assert.AreEqual(default(bool), testNull.Bool.TryGetValue(data).Value);
            Assert.AreEqual(default(string[]), testNull.Strings.TryGetValue(data).Value);
        }

        [TestMethod]
        public void ExpressionPropertyTests_StringExpression()
        {
            var data = new
            {
                test = "joe"
            };
            var str = new StringExpression("test");
            Assert.AreEqual("=`test`", str.ExpressionText);
            Assert.IsNull(str.Value);
            Assert.AreEqual(str.ToString(), JsonConvert.DeserializeObject<StringExpression>(JsonConvert.SerializeObject(str, settings: settings), settings: settings).ToString());
            var (result, error) = str.TryGetValue(data);
            Assert.AreEqual("test", result);
            Assert.IsNull(error);

            str = new StringExpression("=test");
            Assert.AreEqual("=test", str.ExpressionText);
            Assert.IsNull(str.Value);
            Assert.AreEqual(str.ToString(), JsonConvert.DeserializeObject<StringExpression>(JsonConvert.SerializeObject(str, settings: settings), settings: settings).ToString());
            (result, error) = str.TryGetValue(data);
            Assert.AreEqual("joe", result);
            Assert.IsNull(error);

            str = new StringExpression("Hello ${test}");
            Assert.AreEqual("=`Hello ${test}`", str.ExpressionText);
            Assert.IsNull(str.Value);
            Assert.AreEqual(str.ToString(), JsonConvert.DeserializeObject<StringExpression>(JsonConvert.SerializeObject(str, settings: settings), settings: settings).ToString());
            (result, error) = str.TryGetValue(data);
            Assert.AreEqual("Hello joe", result);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ExpressionPropertyTests_ValueExpression()
        {
            var data = new
            {
                test = new { x = 13 }
            };

            var val = new ValueExpression("test");
            Assert.AreEqual("=`test`", val.ExpressionText);
            Assert.IsNull(val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<ValueExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.AreEqual("test", result);
            Assert.IsNull(error);

            val = new ValueExpression("=test");
            Assert.AreEqual("=test", val.ExpressionText);
            Assert.IsNull(val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<ValueExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test), JsonConvert.SerializeObject(result));
            Assert.IsNull(error);

            val = new ValueExpression(data.test);
            Assert.IsNull(val.ExpressionText);
            Assert.IsNotNull(val.Value);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test, settings), JsonConvert.DeserializeObject<ValueExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test), JsonConvert.SerializeObject(result));
            Assert.IsNull(error);

            val = new ValueExpression("Hello ${test.x}");
            Assert.AreEqual("=`Hello ${test.x}`", val.ExpressionText);
            Assert.IsNull(val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<ValueExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual("Hello 13", result);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ExpressionPropertyTests_BoolExpression()
        {
            var data = new
            {
                test = true
            };

            var val = new BoolExpression("true");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(bool), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<BoolExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.IsTrue(result);
            Assert.IsNull(error);

            val = new BoolExpression("=true");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(bool), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<BoolExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.IsTrue(result);
            Assert.IsNull(error);

            val = new BoolExpression(true);
            Assert.IsNull(val.ExpressionText);
            Assert.IsTrue(val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<BoolExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.IsTrue(result);
            Assert.IsNull(error);

            val = new BoolExpression("=test");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(bool), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<BoolExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.IsTrue(result);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ExpressionPropertyTests_EnumExpression()
        {
            var data = new
            {
                test = TestEnum.Two
            };

            var val = new EnumExpression<TestEnum>("three");
            Assert.IsNull(val.ExpressionText);
            Assert.AreEqual(TestEnum.Three, val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.AreEqual(TestEnum.Three, result);
            Assert.IsNull(error);

            val = new EnumExpression<TestEnum>("=three");
            Assert.IsNull(val.ExpressionText);
            Assert.AreEqual(TestEnum.Three, val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(TestEnum.Three, result);
            Assert.IsNull(error);

            val = new EnumExpression<TestEnum>("=test");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(TestEnum), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(TestEnum.Two, result);
            Assert.IsNull(error);

            val = new EnumExpression<TestEnum>(TestEnum.Three);
            Assert.IsNull(val.ExpressionText);
            Assert.AreEqual(TestEnum.Three, val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(TestEnum.Three, result);
            Assert.IsNull(error);

            val = new EnumExpression<TestEnum>("garbage");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(TestEnum), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(default(TestEnum), result);
            Assert.IsNull(error);

            val = new EnumExpression<TestEnum>("=sum(garbage)");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(TestEnum), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<EnumExpression<TestEnum>>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(default(TestEnum), result);
            Assert.IsNotNull(error);
        }

        [TestMethod]
        public void ExpressionPropertyTests_IntExpression()
        {
            TestNumberExpression<IntExpression, int>(new IntExpression(), 13);
        }

        [TestMethod]
        public void ExpressionPropertyTests_FloatExpression()
        {
            TestNumberExpression<NumberExpression, float>(new NumberExpression(), 3.14F);
        }

        private void TestNumberExpression<TExpression, TValue>(TExpression val, TValue expected)
            where TExpression : ExpressionProperty<TValue>, new()
        {
            var data = new
            {
                test = expected
            };

            val.SetValue("test");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(TValue), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            var (result, error) = val.TryGetValue(data);
            Assert.AreEqual(expected, result);
            Assert.IsNull(error);

            val.SetValue("=test");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(TValue), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(expected, result);
            Assert.IsNull(error);

            val.SetValue($"{expected}");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(TValue), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(expected, result);
            Assert.IsNull(error);

            val.SetValue($"={expected}");
            Assert.IsNotNull(val.ExpressionText);
            Assert.AreEqual(default(TValue), val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(expected, result);
            Assert.IsNull(error);

            val.SetValue(expected);
            Assert.IsNull(val.ExpressionText);
            Assert.AreEqual(expected, val.Value);
            Assert.AreEqual(val.ToString(), JsonConvert.DeserializeObject<TExpression>(JsonConvert.SerializeObject(val, settings: settings), settings: settings).ToString());
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(expected, result);
            Assert.IsNull(error);
        }

        [TestMethod]
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
            Assert.IsNotNull(val.ExpressionText);
            Assert.IsNull(val.Value);
            var (result, error) = val.TryGetValue(data);
            Assert.AreEqual(13, result.Age);
            Assert.AreEqual("joe", result.Name);
            Assert.IsNull(error);

            val = new ObjectExpression<Foo>("=test");
            Assert.IsNotNull(val.ExpressionText);
            Assert.IsNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(13, result.Age);
            Assert.AreEqual("joe", result.Name);
            Assert.IsNull(error);

            val = new ObjectExpression<Foo>(data.test);
            Assert.IsNull(val.ExpressionText);
            Assert.IsNotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(13, result.Age);
            Assert.AreEqual("joe", result.Name);
            Assert.IsNull(error);

            val = new ObjectExpression<Foo>(JObject.FromObject(data.test));
            Assert.IsNull(val.ExpressionText);
            Assert.IsNotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(13, result.Age);
            Assert.AreEqual("joe", result.Name);
            Assert.IsNull(error);
        }

        [TestMethod]
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
            Assert.IsNotNull(val.ExpressionText);
            Assert.IsNull(val.Value);
            var (result, error) = val.TryGetValue(data);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test.Strings, settings), JsonConvert.SerializeObject(result, settings: settings));
            CollectionAssert.AreEqual(data.test.Strings, result);

            val = new ArrayExpression<string>("=test.Strings");
            Assert.IsNotNull(val.ExpressionText);
            Assert.IsNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test.Strings, settings), JsonConvert.SerializeObject(result, settings: settings));
            CollectionAssert.AreEqual(data.test.Strings, result);

            val = new ArrayExpression<string>(data.test.Strings);
            Assert.IsNull(val.ExpressionText);
            Assert.IsNotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test.Strings, settings), JsonConvert.SerializeObject(result, settings: settings));
            CollectionAssert.AreEqual(data.test.Strings, result);

            val = new ArrayExpression<string>(data.test.Strings);
            Assert.IsNull(val.ExpressionText);
            Assert.IsNotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test.Strings, settings), JsonConvert.SerializeObject(result, settings: settings));
            CollectionAssert.AreEqual(data.test.Strings, result);
        }

        [TestMethod]
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
            Assert.IsNotNull(val.ExpressionText);
            Assert.IsNull(val.Value);
            var (result, error) = val.TryGetValue(data);
            CollectionAssert.AreEqual(data.test.Objects, result);

            val = new ArrayExpression<Foo>("=test.Objects");
            Assert.IsNotNull(val.ExpressionText);
            Assert.IsNull(val.Value);
            (result, error) = val.TryGetValue(data);
            CollectionAssert.AreEqual(data.test.Objects, result);

            val = new ArrayExpression<Foo>(data.test.Objects);
            Assert.IsNull(val.ExpressionText);
            Assert.IsNotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test.Objects, settings), JsonConvert.SerializeObject(result, settings));

            val = new ArrayExpression<Foo>(JArray.FromObject(data.test.Objects));
            Assert.IsNull(val.ExpressionText);
            Assert.IsNotNull(val.Value);
            (result, error) = val.TryGetValue(data);
            Assert.AreEqual(JsonConvert.SerializeObject(data.test.Objects, settings), JsonConvert.SerializeObject(result, settings));
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
