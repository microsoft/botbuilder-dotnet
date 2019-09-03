using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    [TestClass]
    public class ObjectExtensionsTests
    {
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
