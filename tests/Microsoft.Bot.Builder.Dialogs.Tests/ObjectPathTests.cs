#pragma warning disable SA1402

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class ObjectPathTests
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

        [TestMethod]
        public void Typed_OnlyDefaultTest()
        {
            var defaultOptions = new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            };
            var overlay = new Options() { };

            var result = ObjectPath.Merge(defaultOptions, overlay);
            Assert.AreEqual(result.LastName, defaultOptions.LastName);
            Assert.AreEqual(result.FirstName, defaultOptions.FirstName);
            Assert.AreEqual(result.Age, defaultOptions.Age);
            Assert.AreEqual(result.Bool, defaultOptions.Bool);
            Assert.AreEqual(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.AreEqual(result.Location.Long, defaultOptions.Location.Long);
        }

        [TestMethod]
        public void Typed_OnlyOverlay()
        {
            var defaultOptions = new Options() { };

            var overlay = new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            };

            var result = ObjectPath.Merge(defaultOptions, overlay);
            Assert.AreEqual(result.LastName, overlay.LastName);
            Assert.AreEqual(result.FirstName, overlay.FirstName);
            Assert.AreEqual(result.Age, overlay.Age);
            Assert.AreEqual(result.Bool, overlay.Bool);
            Assert.AreEqual(result.Location.Lat, overlay.Location.Lat);
            Assert.AreEqual(result.Location.Long, overlay.Location.Long);
        }

        [TestMethod]
        public void Typed_FullOverlay()
        {
            var defaultOptions = new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            };

            var overlay = new Options()
            {
                LastName = "Grant",
                FirstName = "Eddit",
                Age = 32,
                Bool = true,
                Location = new Location() { Lat = 2.2312312F, Long = 2.234234F }
            };

            var result = ObjectPath.Merge(defaultOptions, overlay);

            Assert.AreEqual(result.LastName, overlay.LastName);
            Assert.AreEqual(result.FirstName, overlay.FirstName);
            Assert.AreEqual(result.Age, overlay.Age);
            Assert.AreEqual(result.Bool, overlay.Bool);
            Assert.AreEqual(result.Location.Lat, overlay.Location.Lat);
            Assert.AreEqual(result.Location.Long, overlay.Location.Long);
        }

        [TestMethod]
        public void Typed_PartialOverlay()
        {
            var defaultOptions = new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            };

            var overlay = new Options()
            {
                LastName = "Grant"
            };

            var result = ObjectPath.Merge(defaultOptions, overlay);

            Assert.AreEqual(result.LastName, overlay.LastName);
            Assert.AreEqual(result.FirstName, defaultOptions.FirstName);
            Assert.AreEqual(result.Age, defaultOptions.Age);
            Assert.AreEqual(result.Bool, defaultOptions.Bool);
            Assert.AreEqual(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.AreEqual(result.Location.Long, defaultOptions.Location.Long);
        }

        [TestMethod]
        public void Anonymous_OnlyDefaultTest()
        {
            dynamic defaultOptions = new
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Bool = (bool?)true,
                Location = new { Lat = 1.2312312F, Long = 3.234234F }
            };
            dynamic overlay = new { };

            var result = ObjectPath.Assign<Options>(defaultOptions, overlay);
            Assert.AreEqual(result.LastName, defaultOptions.LastName);
            Assert.AreEqual(result.FirstName, defaultOptions.FirstName);
            Assert.AreEqual(result.Age, defaultOptions.Age);
            Assert.AreEqual(result.Bool, defaultOptions.Bool);
            Assert.AreEqual(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.AreEqual(result.Location.Long, defaultOptions.Location.Long);
        }

        [TestMethod]
        public void Anonymous_OnlyOverlay()
        {
            dynamic defaultOptions = new { };

            dynamic overlay = new
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Bool = (bool?)true,
                Location = new { Lat = 1.2312312F, Long = 3.234234F }
            };

            var result = ObjectPath.Assign<Options>(defaultOptions, overlay);

            Assert.AreEqual(result.LastName, overlay.LastName);
            Assert.AreEqual(result.FirstName, overlay.FirstName);
            Assert.AreEqual(result.Age, overlay.Age);
            Assert.AreEqual(result.Bool, overlay.Bool);
            Assert.AreEqual(result.Location.Lat, overlay.Location.Lat);
            Assert.AreEqual(result.Location.Long, overlay.Location.Long);
        }

        [TestMethod]
        public void Anonymous_FullOverlay()
        {
            dynamic defaultOptions = new
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Bool = (bool?)true,
                Location = new { Lat = 1.2312312F, Long = 3.234234F }
            };

            dynamic overlay = new
            {
                LastName = "Grant",
                FirstName = "Eddit",
                Age = 32,
                Bool = (bool?)true,
                Location = new { Lat = 2.2312312F, Long = 2.234234F }
            };

            var result = ObjectPath.Assign<Options>(defaultOptions, overlay);

            Assert.AreEqual(result.LastName, overlay.LastName);
            Assert.AreEqual(result.FirstName, overlay.FirstName);
            Assert.AreEqual(result.Age, overlay.Age);
            Assert.AreEqual(result.Bool, overlay.Bool);
            Assert.AreEqual(result.Location.Lat, overlay.Location.Lat);
            Assert.AreEqual(result.Location.Long, overlay.Location.Long);
        }

        [TestMethod]
        public void Anonymous_PartialOverlay()
        {
            dynamic defaultOptions = new
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Bool = (bool?)true,
                Location = new { Lat = 1.2312312F, Long = 3.234234F }
            };

            dynamic overlay = new
            {
                LastName = "Grant"
            };
            var result = ObjectPath.Assign<Options>(defaultOptions, overlay);

            Assert.AreEqual(result.LastName, overlay.LastName);
            Assert.AreEqual(result.FirstName, defaultOptions.FirstName);
            Assert.AreEqual(result.Age, defaultOptions.Age);
            Assert.AreEqual(result.Bool, defaultOptions.Bool);
            Assert.AreEqual(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.AreEqual(result.Location.Long, defaultOptions.Location.Long);
        }

        [TestMethod]
        public void JObject_OnlyDefaultTest()
        {
            dynamic defaultOptions = JObject.FromObject(new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            });
            dynamic overlay = JObject.FromObject(new Options() { });

            var result = ObjectPath.Assign<Options>(defaultOptions, overlay);
            Assert.AreEqual(result.LastName, (string)defaultOptions.LastName);
            Assert.AreEqual(result.FirstName, (string)defaultOptions.FirstName);
            Assert.AreEqual(result.Age, (int?)defaultOptions.Age);
            Assert.AreEqual(result.Bool, (bool?)defaultOptions.Bool);
            Assert.AreEqual(result.Location.Lat, (float?)defaultOptions.Location.Lat);
            Assert.AreEqual(result.Location.Long, (float?)defaultOptions.Location.Long);
        }

        [TestMethod]
        public void JObject_OnlyOverlay()
        {
            dynamic defaultOptions = JObject.FromObject(new Options() { });

            dynamic overlay = JObject.FromObject(new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            });

            var result = ObjectPath.Assign<Options>(defaultOptions, overlay);

            Assert.AreEqual(result.LastName, (string)overlay.LastName);
            Assert.AreEqual(result.FirstName, (string)overlay.FirstName);
            Assert.AreEqual(result.Age, (int?)overlay.Age);
            Assert.AreEqual(result.Bool, (bool?)overlay.Bool);
            Assert.AreEqual(result.Location.Lat, (float?)overlay.Location.Lat);
            Assert.AreEqual(result.Location.Long, (float?)overlay.Location.Long);
        }

        [TestMethod]
        public void JObject_FullOverlay()
        {
            dynamic defaultOptions = JObject.FromObject(new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            });

            dynamic overlay = JObject.FromObject(new Options()
            {
                LastName = "Grant",
                FirstName = "Eddit",
                Age = 32,
                Bool = true,
                Location = new Location() { Lat = 2.2312312F, Long = 2.234234F }
            });

            var result = ObjectPath.Assign<Options>(defaultOptions, overlay);

            Assert.AreEqual(result.LastName, (string)overlay.LastName);
            Assert.AreEqual(result.FirstName, (string)overlay.FirstName);
            Assert.AreEqual(result.Age, (int?)overlay.Age);
            Assert.AreEqual(result.Bool, (bool?)overlay.Bool);
            Assert.AreEqual(result.Location.Lat, (float?)overlay.Location.Lat);
            Assert.AreEqual(result.Location.Long, (float?)overlay.Location.Long);
        }

        [TestMethod]
        public void JObject_PartialOverlay()
        {
            dynamic defaultOptions = JObject.FromObject(new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            });

            dynamic overlay = JObject.FromObject(new Options()
            {
                LastName = "Grant"
            });

            var result = ObjectPath.Assign<Options>(defaultOptions, overlay);

            Assert.AreEqual(result.LastName, (string)overlay.LastName);
            Assert.AreEqual(result.FirstName, (string)defaultOptions.FirstName);
            Assert.AreEqual(result.Age, (int?)defaultOptions.Age);
            Assert.AreEqual(result.Bool, (bool?)defaultOptions.Bool);
            Assert.AreEqual(result.Location.Lat, (float?)defaultOptions.Location.Lat);
            Assert.AreEqual(result.Location.Long, (float?)defaultOptions.Location.Long);
        }

        [TestMethod]
        public void NullStartObject()
        {
            var defaultOptions = new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            };

            var result = ObjectPath.Assign<Options>(null, defaultOptions);
            Assert.AreEqual(result.LastName, defaultOptions.LastName);
            Assert.AreEqual(result.FirstName, defaultOptions.FirstName);
            Assert.AreEqual(result.Age, defaultOptions.Age);
            Assert.AreEqual(result.Bool, defaultOptions.Bool);
            Assert.AreEqual(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.AreEqual(result.Location.Long, defaultOptions.Location.Long);
        }

        [TestMethod]
        public void NullOverlay()
        {
            var defaultOptions = new Options()
            {
                LastName = "Smith",
                FirstName = "Fred",
                Age = 22,
                Location = new Location() { Lat = 1.2312312F, Long = 3.234234F }
            };

            var result = ObjectPath.Assign<Options>(defaultOptions, null);
            Assert.AreEqual(result.LastName, defaultOptions.LastName);
            Assert.AreEqual(result.FirstName, defaultOptions.FirstName);
            Assert.AreEqual(result.Age, defaultOptions.Age);
            Assert.AreEqual(result.Bool, defaultOptions.Bool);
            Assert.AreEqual(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.AreEqual(result.Location.Long, defaultOptions.Location.Long);
        }

        [TestMethod]
        public void TryGetPathValue()
        {
            var test = new
            {
                test = "test",

                options = new
                {
                    Age = 15,
                    FirstName = "joe",
                    LastName = "blow",
                    Bool = false,
                },

                bar = new
                {
                    numIndex = 2,
                    strIndex = "FirstName",
                    objIndex = "options",
                    options = new Options()
                    {
                        Age = 1,
                        FirstName = "joe",
                        LastName = "blow",
                        Bool = false,
                    },
                    numbers = new int[] { 1, 2, 3, 4, 5 }
                },
            };

            // set with anonymous object
            {
                Assert.AreEqual(test, ObjectPath.GetPathValue<object>(test, string.Empty), "empty should return root");
                Assert.AreEqual(test.test, ObjectPath.GetPathValue<string>(test, "test"));
                Assert.AreEqual(test.bar.options.Age, ObjectPath.GetPathValue<int>(test, "bar.options.age"));

                Assert.IsTrue(ObjectPath.TryGetPathValue<Options>(test, "options", out Options options));
                Assert.AreEqual(test.options.Age, options.Age);
                Assert.AreEqual(test.options.FirstName, options.FirstName);

                Assert.IsTrue(ObjectPath.TryGetPathValue<Options>(test, "bar.options", out Options barOptions));
                Assert.AreEqual(test.bar.options.Age, barOptions.Age);
                Assert.AreEqual(test.bar.options.FirstName, barOptions.FirstName);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int[]>(test, "bar.numbers", out int[] numbers));
                Assert.AreEqual(5, numbers.Length);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(test, "bar.numbers[1]", out int number));
                Assert.AreEqual(2, number);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(test, "bar['options'].Age", out number));
                Assert.AreEqual(1, number);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(test, "bar[\"options\"].Age", out number));
                Assert.AreEqual(1, number);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(test, "bar.numbers[bar.numIndex]", out number));
                Assert.AreEqual(3, number);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(test, "bar.numbers[bar[bar.objIndex].Age]", out number));
                Assert.AreEqual(2, number);

                Assert.IsTrue(ObjectPath.TryGetPathValue<string>(test, "bar.options[bar.strIndex]", out string name));
                Assert.AreEqual("joe", name);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(test, "bar[bar.objIndex].Age", out int age));
                Assert.AreEqual(1, age);
            }

            // now try with JObject
            {
                var json = JsonConvert.SerializeObject(test, settings);
                dynamic jtest = JsonConvert.DeserializeObject(json);
                Assert.AreEqual(json, JsonConvert.SerializeObject(ObjectPath.GetPathValue<object>(jtest, string.Empty), settings), "empty should return root");
                Assert.AreEqual((string)jtest.test, ObjectPath.GetPathValue<string>(jtest, "test"));
                Assert.AreEqual((int)jtest.bar.options.Age, ObjectPath.GetPathValue<int>(jtest, "bar.options.age"));

                Assert.IsTrue(ObjectPath.TryGetPathValue<Options>(jtest, "options", out Options options));
                Assert.AreEqual((int)jtest.options.Age, options.Age);
                Assert.AreEqual((string)jtest.options.FirstName, options.FirstName);

                Assert.IsTrue(ObjectPath.TryGetPathValue<Options>(jtest, "bar.options", out Options barOptions));
                Assert.AreEqual((int)jtest.bar.options.Age, barOptions.Age);
                Assert.AreEqual((string)jtest.bar.options.FirstName, barOptions.FirstName);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int[]>(jtest, "bar.numbers", out int[] numbers));
                Assert.AreEqual(5, numbers.Length);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(jtest, "bar.numbers[1]", out int number));
                Assert.AreEqual(2, number);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(jtest, "bar['options'].Age", out number));
                Assert.AreEqual(1, number);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(jtest, "bar[\"options\"].Age", out number));
                Assert.AreEqual(1, number);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(jtest, "bar.numbers[bar.numIndex]", out int number2));
                Assert.AreEqual(3, number2);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(jtest, "bar.numbers[bar[bar.objIndex].Age]", out int number3));
                Assert.AreEqual(2, number3);

                Assert.IsTrue(ObjectPath.TryGetPathValue<string>(jtest, "bar.options[bar.strIndex]", out string name));
                Assert.AreEqual("joe", name);

                Assert.IsTrue(ObjectPath.TryGetPathValue<int>(jtest, "bar[bar.objIndex].Age", out int age));
                Assert.AreEqual(1, age);

                jtest.bar["x.y.z"] = "test";

                Assert.IsTrue(ObjectPath.TryGetPathValue<string>(jtest, "bar['x.y.z']", out string split));
                Assert.AreEqual("test", split);

                Assert.IsTrue(ObjectPath.TryGetPathValue<string>(jtest, "bar[\"x.y.z\"]", out split));
                Assert.AreEqual("test", split);
            }
        }

        public void AssertGetSetValueType<T>(object test, T val)
        {
            ObjectPath.SetPathValue(test, val.GetType().Name, val);
            var result = ObjectPath.GetPathValue<T>(test, typeof(T).Name);
            Assert.AreEqual(val, result);
            Assert.AreEqual(val.GetType(), result.GetType());
        }

        [TestMethod]
        public void SetPathValue()
        {
            Dictionary<string, object> test = new Dictionary<string, object>();
            ObjectPath.SetPathValue(test, "x.y.z", 15);
            ObjectPath.SetPathValue(test, "x.p", "hello");
            ObjectPath.SetPathValue(test, "foo", new { Bar = 15, Blat = "yo" });
            ObjectPath.SetPathValue(test, "x.a[1]", "yabba");
            ObjectPath.SetPathValue(test, "x.a[0]", "dabba");
            ObjectPath.SetPathValue(test, "null", null);
            ObjectPath.SetPathValue(test, "enum", TypeCode.Empty);

            Assert.AreEqual(15, ObjectPath.GetPathValue<int>(test, "x.y.z"));
            Assert.AreEqual("hello", ObjectPath.GetPathValue<string>(test, "x.p"));
            Assert.AreEqual(15, ObjectPath.GetPathValue<int>(test, "foo.bar"));
            Assert.AreEqual("yo", ObjectPath.GetPathValue<string>(test, "foo.Blat"));
            Assert.IsFalse(ObjectPath.TryGetPathValue<string>(test, "foo.Blatxxx", out var value));
            Assert.IsTrue(ObjectPath.TryGetPathValue<string>(test, "x.a[1]", out var value2));
            Assert.AreEqual("yabba", value2);
            Assert.IsTrue(ObjectPath.TryGetPathValue<string>(test, "x.a[0]", out value2));
            Assert.AreEqual("dabba", value2);
            Assert.IsFalse(ObjectPath.TryGetPathValue<object>(test, "null", out var nullValue));
            Assert.AreEqual(TypeCode.Empty, ObjectPath.GetPathValue<TypeCode>(test, "enum"));

            // value type tests
#pragma warning disable SA1121 // Use built-in type alias
            AssertGetSetValueType(test, true);
            AssertGetSetValueType(test, DateTime.UtcNow);
            AssertGetSetValueType(test, DateTimeOffset.UtcNow);
            AssertGetSetValueType(test, Byte.MaxValue);
            AssertGetSetValueType(test, Int16.MaxValue);
            AssertGetSetValueType(test, Int32.MaxValue);
            AssertGetSetValueType(test, Int64.MaxValue);
            AssertGetSetValueType(test, UInt16.MaxValue);
            AssertGetSetValueType(test, UInt32.MaxValue);
            AssertGetSetValueType(test, UInt64.MaxValue);
            AssertGetSetValueType(test, Single.MaxValue);
            AssertGetSetValueType(test, Decimal.MaxValue);
            AssertGetSetValueType(test, Double.MaxValue);
#pragma warning restore SA1121 // Use built-in type alias
        }

        [TestMethod]
        public void RemovePathValue()
        {
            Dictionary<string, object> test = new Dictionary<string, object>();
            ObjectPath.SetPathValue(test, "x.y.z", 15);
            ObjectPath.SetPathValue(test, "x.p", "hello");
            ObjectPath.SetPathValue(test, "foo", new { Bar = 15, Blat = "yo" });
            ObjectPath.SetPathValue(test, "x.a[1]", "yabba");
            ObjectPath.SetPathValue(test, "x.a[0]", "dabba");

            ObjectPath.RemovePathValue(test, "x.y.z");
            try
            {
                ObjectPath.GetPathValue<int>(test, "x.y.z");
                Assert.Fail("should have throw exception");
            }
            catch
            {
            }

            Assert.IsNull(ObjectPath.GetPathValue<string>(test, "x.y.z", null));
            Assert.AreEqual(99, ObjectPath.GetPathValue<int>(test, "x.y.z", 99));
            Assert.IsFalse(ObjectPath.TryGetPathValue<string>(test, "x.y.z", out var value));
            ObjectPath.RemovePathValue(test, "x.a[1]");
            Assert.IsFalse(ObjectPath.TryGetPathValue<string>(test, "x.a[1]", out string value2));
            Assert.IsTrue(ObjectPath.TryGetPathValue<string>(test, "x.a[0]", out value2));
            Assert.AreEqual("dabba", value2);
        }
    }

    public class Location
    {
        public float? Lat { get; set; }

        public float? Long { get; set; }
    }

    public class Options
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? Age { get; set; }

        public bool? Bool { get; set; }

        public Location Location { get; set; }
    }
}
