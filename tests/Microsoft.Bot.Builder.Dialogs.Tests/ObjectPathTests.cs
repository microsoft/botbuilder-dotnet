#pragma warning disable SA1402

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class ObjectPathTests
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

        [Fact]
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
            Assert.Equal(result.LastName, defaultOptions.LastName);
            Assert.Equal(result.FirstName, defaultOptions.FirstName);
            Assert.Equal(result.Age, defaultOptions.Age);
            Assert.Equal(result.Bool, defaultOptions.Bool);
            Assert.Equal(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.Equal(result.Location.Long, defaultOptions.Location.Long);
        }

        [Fact]
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
            Assert.Equal(result.LastName, overlay.LastName);
            Assert.Equal(result.FirstName, overlay.FirstName);
            Assert.Equal(result.Age, overlay.Age);
            Assert.Equal(result.Bool, overlay.Bool);
            Assert.Equal(result.Location.Lat, overlay.Location.Lat);
            Assert.Equal(result.Location.Long, overlay.Location.Long);
        }

        [Fact]
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

            Assert.Equal(result.LastName, overlay.LastName);
            Assert.Equal(result.FirstName, overlay.FirstName);
            Assert.Equal(result.Age, overlay.Age);
            Assert.Equal(result.Bool, overlay.Bool);
            Assert.Equal(result.Location.Lat, overlay.Location.Lat);
            Assert.Equal(result.Location.Long, overlay.Location.Long);
        }

        [Fact]
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

            Assert.Equal(result.LastName, overlay.LastName);
            Assert.Equal(result.FirstName, defaultOptions.FirstName);
            Assert.Equal(result.Age, defaultOptions.Age);
            Assert.Equal(result.Bool, defaultOptions.Bool);
            Assert.Equal(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.Equal(result.Location.Long, defaultOptions.Location.Long);
        }

        [Fact]
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
            Assert.Equal(result.LastName, defaultOptions.LastName);
            Assert.Equal(result.FirstName, defaultOptions.FirstName);
            Assert.Equal(result.Age, defaultOptions.Age);
            Assert.Equal(result.Bool, defaultOptions.Bool);
            Assert.Equal(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.Equal(result.Location.Long, defaultOptions.Location.Long);
        }

        [Fact]
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

            Assert.Equal(result.LastName, overlay.LastName);
            Assert.Equal(result.FirstName, overlay.FirstName);
            Assert.Equal(result.Age, overlay.Age);
            Assert.Equal(result.Bool, overlay.Bool);
            Assert.Equal(result.Location.Lat, overlay.Location.Lat);
            Assert.Equal(result.Location.Long, overlay.Location.Long);
        }

        [Fact]
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

            Assert.Equal(result.LastName, overlay.LastName);
            Assert.Equal(result.FirstName, overlay.FirstName);
            Assert.Equal(result.Age, overlay.Age);
            Assert.Equal(result.Bool, overlay.Bool);
            Assert.Equal(result.Location.Lat, overlay.Location.Lat);
            Assert.Equal(result.Location.Long, overlay.Location.Long);
        }

        [Fact]
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

            Assert.Equal(result.LastName, overlay.LastName);
            Assert.Equal(result.FirstName, defaultOptions.FirstName);
            Assert.Equal(result.Age, defaultOptions.Age);
            Assert.Equal(result.Bool, defaultOptions.Bool);
            Assert.Equal(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.Equal(result.Location.Long, defaultOptions.Location.Long);
        }

        [Fact]
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
            Assert.Equal(result.LastName, (string)defaultOptions.LastName);
            Assert.Equal(result.FirstName, (string)defaultOptions.FirstName);
            Assert.Equal(result.Age, (int?)defaultOptions.Age);
            Assert.Equal(result.Bool, (bool?)defaultOptions.Bool);
            Assert.Equal(result.Location.Lat, (float?)defaultOptions.Location.Lat);
            Assert.Equal(result.Location.Long, (float?)defaultOptions.Location.Long);
        }

        [Fact]
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

            Assert.Equal(result.LastName, (string)overlay.LastName);
            Assert.Equal(result.FirstName, (string)overlay.FirstName);
            Assert.Equal(result.Age, (int?)overlay.Age);
            Assert.Equal(result.Bool, (bool?)overlay.Bool);
            Assert.Equal(result.Location.Lat, (float?)overlay.Location.Lat);
            Assert.Equal(result.Location.Long, (float?)overlay.Location.Long);
        }

        [Fact]
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

            Assert.Equal(result.LastName, (string)overlay.LastName);
            Assert.Equal(result.FirstName, (string)overlay.FirstName);
            Assert.Equal(result.Age, (int?)overlay.Age);
            Assert.Equal(result.Bool, (bool?)overlay.Bool);
            Assert.Equal(result.Location.Lat, (float?)overlay.Location.Lat);
            Assert.Equal(result.Location.Long, (float?)overlay.Location.Long);
        }

        [Fact]
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

            Assert.Equal(result.LastName, (string)overlay.LastName);
            Assert.Equal(result.FirstName, (string)defaultOptions.FirstName);
            Assert.Equal(result.Age, (int?)defaultOptions.Age);
            Assert.Equal(result.Bool, (bool?)defaultOptions.Bool);
            Assert.Equal(result.Location.Lat, (float?)defaultOptions.Location.Lat);
            Assert.Equal(result.Location.Long, (float?)defaultOptions.Location.Long);
        }

        [Fact]
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
            Assert.Equal(result.LastName, defaultOptions.LastName);
            Assert.Equal(result.FirstName, defaultOptions.FirstName);
            Assert.Equal(result.Age, defaultOptions.Age);
            Assert.Equal(result.Bool, defaultOptions.Bool);
            Assert.Equal(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.Equal(result.Location.Long, defaultOptions.Location.Long);
        }

        [Fact]
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
            Assert.Equal(result.LastName, defaultOptions.LastName);
            Assert.Equal(result.FirstName, defaultOptions.FirstName);
            Assert.Equal(result.Age, defaultOptions.Age);
            Assert.Equal(result.Bool, defaultOptions.Bool);
            Assert.Equal(result.Location.Lat, defaultOptions.Location.Lat);
            Assert.Equal(result.Location.Long, defaultOptions.Location.Long);
        }

        [Fact]
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
                Assert.Equal(test, ObjectPath.GetPathValue<object>(test, string.Empty));
                Assert.Equal(test.test, ObjectPath.GetPathValue<string>(test, "test"));
                Assert.Equal(test.bar.options.Age, ObjectPath.GetPathValue<int>(test, "bar.options.age"));

                Assert.True(ObjectPath.TryGetPathValue<Options>(test, "options", out Options options));
                Assert.Equal(test.options.Age, options.Age);
                Assert.Equal(test.options.FirstName, options.FirstName);

                Assert.True(ObjectPath.TryGetPathValue<Options>(test, "bar.options", out Options barOptions));
                Assert.Equal(test.bar.options.Age, barOptions.Age);
                Assert.Equal(test.bar.options.FirstName, barOptions.FirstName);

                Assert.True(ObjectPath.TryGetPathValue<int[]>(test, "bar.numbers", out int[] numbers));
                Assert.Equal(5, numbers.Length);

                Assert.True(ObjectPath.TryGetPathValue<int>(test, "bar.numbers[1]", out int number));
                Assert.Equal(2, number);

                Assert.True(ObjectPath.TryGetPathValue<int>(test, "bar['options'].Age", out number));
                Assert.Equal(1, number);

                Assert.True(ObjectPath.TryGetPathValue<int>(test, "bar[\"options\"].Age", out number));
                Assert.Equal(1, number);

                Assert.True(ObjectPath.TryGetPathValue<int>(test, "bar.numbers[bar.numIndex]", out number));
                Assert.Equal(3, number);

                Assert.True(ObjectPath.TryGetPathValue<int>(test, "bar.numbers[bar[bar.objIndex].Age]", out number));
                Assert.Equal(2, number);

                Assert.True(ObjectPath.TryGetPathValue<string>(test, "bar.options[bar.strIndex]", out string name));
                Assert.Equal("joe", name);

                Assert.True(ObjectPath.TryGetPathValue<int>(test, "bar[bar.objIndex].Age", out int age));
                Assert.Equal(1, age);
            }

            // now try with JObject
            {
                var json = JsonConvert.SerializeObject(test, settings);
                dynamic jtest = JsonConvert.DeserializeObject(json);
                Assert.Equal(json, JsonConvert.SerializeObject(ObjectPath.GetPathValue<object>(jtest, string.Empty), settings));
                Assert.Equal((string)jtest.test, ObjectPath.GetPathValue<string>(jtest, "test"));
                Assert.Equal((int)jtest.bar.options.Age, ObjectPath.GetPathValue<int>(jtest, "bar.options.age"));

                Assert.True(ObjectPath.TryGetPathValue<Options>(jtest, "options", out Options options));
                Assert.Equal((int)jtest.options.Age, options.Age);
                Assert.Equal((string)jtest.options.FirstName, options.FirstName);

                Assert.True(ObjectPath.TryGetPathValue<Options>(jtest, "bar.options", out Options barOptions));
                Assert.Equal((int)jtest.bar.options.Age, barOptions.Age);
                Assert.Equal((string)jtest.bar.options.FirstName, barOptions.FirstName);

                Assert.True(ObjectPath.TryGetPathValue<int[]>(jtest, "bar.numbers", out int[] numbers));
                Assert.Equal(5, numbers.Length);

                Assert.True(ObjectPath.TryGetPathValue<int>(jtest, "bar.numbers[1]", out int number));
                Assert.Equal(2, number);

                Assert.True(ObjectPath.TryGetPathValue<int>(jtest, "bar['options'].Age", out number));
                Assert.Equal(1, number);

                Assert.True(ObjectPath.TryGetPathValue<int>(jtest, "bar[\"options\"].Age", out number));
                Assert.Equal(1, number);

                Assert.True(ObjectPath.TryGetPathValue<int>(jtest, "bar.numbers[bar.numIndex]", out int number2));
                Assert.Equal(3, number2);

                Assert.True(ObjectPath.TryGetPathValue<int>(jtest, "bar.numbers[bar[bar.objIndex].Age]", out int number3));
                Assert.Equal(2, number3);

                Assert.True(ObjectPath.TryGetPathValue<string>(jtest, "bar.options[bar.strIndex]", out string name));
                Assert.Equal("joe", name);

                Assert.True(ObjectPath.TryGetPathValue<int>(jtest, "bar[bar.objIndex].Age", out int age));
                Assert.Equal(1, age);

                jtest.bar["x.y.z"] = "test";

                Assert.True(ObjectPath.TryGetPathValue<string>(jtest, "bar['x.y.z']", out string split));
                Assert.Equal("test", split);

                Assert.True(ObjectPath.TryGetPathValue<string>(jtest, "bar[\"x.y.z\"]", out split));
                Assert.Equal("test", split);
            }
        }

        [Fact]
        public void SetPathValue()
        {
            const string dateISO = "2021-11-30T23:59:59:000Z";
            var test = new Dictionary<string, object>();

            ObjectPath.SetPathValue(test, "x.y.z", 15);
            ObjectPath.SetPathValue(test, "x.p", "hello");
            ObjectPath.SetPathValue(test, "foo", new { Bar = 15, Blat = "yo" });
            ObjectPath.SetPathValue(test, "x.a[1]", "yabba");
            ObjectPath.SetPathValue(test, "x.a[0]", "dabba");
            ObjectPath.SetPathValue(test, "null", null);
            ObjectPath.SetPathValue(test, "enum", TypeCode.Empty);
            ObjectPath.SetPathValue(test, "date.string.iso", dateISO);
            ObjectPath.SetPathValue(test, "date.string.jtoken.iso", new JValue(dateISO));
            ObjectPath.SetPathValue(test, "date.object", new { iso = dateISO });
            ObjectPath.SetPathValue(test, "date.object.jtoken", JToken.FromObject(new { iso = dateISO }));

            Assert.Equal(15, ObjectPath.GetPathValue<int>(test, "x.y.z"));
            Assert.Equal("hello", ObjectPath.GetPathValue<string>(test, "x.p"));
            Assert.Equal(15, ObjectPath.GetPathValue<int>(test, "foo.bar"));
            Assert.Equal("yo", ObjectPath.GetPathValue<string>(test, "foo.Blat"));
            Assert.False(ObjectPath.TryGetPathValue<string>(test, "foo.Blatxxx", out var value));
            Assert.True(ObjectPath.TryGetPathValue<string>(test, "x.a[1]", out var value2));
            Assert.Equal("yabba", value2);
            Assert.True(ObjectPath.TryGetPathValue<string>(test, "x.a[0]", out value2));
            Assert.Equal("dabba", value2);
            Assert.False(ObjectPath.TryGetPathValue<object>(test, "null", out var nullValue));
            Assert.Equal(TypeCode.Empty, ObjectPath.GetPathValue<TypeCode>(test, "enum"));
            Assert.Equal(dateISO, ObjectPath.GetPathValue<string>(test, "date.string.iso"));
            Assert.Equal(dateISO, ObjectPath.GetPathValue<string>(test, "date.string.jtoken.iso"));
            Assert.Equal(dateISO, ObjectPath.GetPathValue<string>(test, "date.object.iso"));
            Assert.Equal(dateISO, ObjectPath.GetPathValue<string>(test, "date.object.jtoken.iso"));

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

        [Fact]
        public void RemovePathValue()
        {
            var test = new Dictionary<string, object>();
            ObjectPath.SetPathValue(test, "x.y.z", 15);
            ObjectPath.SetPathValue(test, "x.p", "hello");
            ObjectPath.SetPathValue(test, "foo", new { Bar = 15, Blat = "yo" });
            ObjectPath.SetPathValue(test, "x.a[1]", "yabba");
            ObjectPath.SetPathValue(test, "x.a[0]", "dabba");

            ObjectPath.RemovePathValue(test, "x.y.z");
            try
            {
                ObjectPath.GetPathValue<int>(test, "x.y.z");
                throw new XunitException("should have throw exception");
            }
            catch
            {
            }

            Assert.Null(ObjectPath.GetPathValue<string>(test, "x.y.z", null));
            Assert.Equal(99, ObjectPath.GetPathValue<int>(test, "x.y.z", 99));
            Assert.False(ObjectPath.TryGetPathValue<string>(test, "x.y.z", out var value));
            ObjectPath.RemovePathValue(test, "x.a[1]");
            Assert.False(ObjectPath.TryGetPathValue<string>(test, "x.a[1]", out string value2));
            Assert.True(ObjectPath.TryGetPathValue<string>(test, "x.a[0]", out value2));
            Assert.Equal("dabba", value2);
        }

        private void AssertGetSetValueType<T>(object test, T val)
        {
            ObjectPath.SetPathValue(test, val.GetType().Name, val);
            var result = ObjectPath.GetPathValue<T>(test, typeof(T).Name);
            Assert.Equal(val, result);
            Assert.Equal(val.GetType(), result.GetType());
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
