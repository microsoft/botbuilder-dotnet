// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1201 // Elements should appear in the correct order

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class DictionaryExtensionTests
    {
        public class Foo
        {
            public int Num { get; set; } = 1000;

            public string String { get; set; } = "string";

            /// <summary>
            /// Gets or sets any extra properties to include in the results.
            /// </summary>
            /// <value>
            /// Any extra properties to include in the results.
            /// </value>
            [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only  (we can't change this without breaking binary compat)
            public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
#pragma warning restore CA2227 // Collection properties should be read only
        }

        public class Bar : Foo
        {
            [JsonProperty("count")]
            public int Count { get; set; } = 30;
        }

        [Fact]
        public void DictionaryCoercianTests()
        {
            var data = new CachedBotStateDictionary()
            {
                { "str", "string" },
                { "short", (short)16 },
                { "int", (int)32 },
                { "long", 64L },
                { "ushort", (ushort)16 },
                { "uint", 32U },
                { "ulong", 64UL },
                { "single", 1.0F },
                { "double", (double)2.0 },
                { "foo", new Foo() },
                { "bar", new Bar() },
                { "arr", new Foo[] { new Foo(), new Bar() } }
            };
            
            Assert.Equal("string", data.MapValueTo<string>("str"));
            Assert.Equal(16, data.MapValueTo<short>("short"));
            Assert.Equal(16, data.MapValueTo<ushort>("ushort"));
            Assert.Equal(32, data.MapValueTo<int>("int"));
            Assert.Equal(32U, data.MapValueTo<uint>("uint"));
            Assert.Equal(64, data.MapValueTo<long>("long"));
            Assert.Equal(64UL, data.MapValueTo<ulong>("ulong"));
            Assert.Equal(1.0F, data.MapValueTo<float>("single"));
            Assert.Equal((double)2.0, data.MapValueTo<double>("double"));

            Assert.Equal(1000, data.MapValueTo<Foo>("foo").Num);

            Assert.Equal(1000, data.MapValueTo<Bar>("bar").Num);
            Assert.Equal(30, data.MapValueTo<Bar>("bar").Count);

            Assert.Equal(1000, data.MapValueTo<Foo[]>("arr")[0].Num);
            Assert.Equal(data.MapValueTo<Foo>("foo").GetHashCode(), data.MapValueTo<Foo>("foo").GetHashCode());

            var serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.None };
            data = JObject.FromObject(data, serializer).ToObject<CachedBotStateDictionary>(serializer);
            Assert.IsType<JObject>(data["foo"]);
            Assert.IsType<JObject>(data["bar"]);
            Assert.IsType<JArray>(data["arr"]);

            Assert.Equal("string", data.MapValueTo<string>("str"));
            Assert.Equal(16, data.MapValueTo<short>("short"));
            Assert.Equal(16, data.MapValueTo<ushort>("ushort"));
            Assert.Equal(32, data.MapValueTo<int>("int"));
            Assert.Equal(32U, data.MapValueTo<uint>("uint"));
            Assert.Equal(64, data.MapValueTo<long>("long"));
            Assert.Equal(64UL, data.MapValueTo<ulong>("ulong"));
            Assert.Equal(1.0F, data.MapValueTo<float>("single"));
            Assert.Equal((double)2.0, data.MapValueTo<double>("double"));

            Assert.Equal(1000, data.MapValueTo<Foo>("foo").Num);

            Assert.Equal(1000, data.MapValueTo<Bar>("bar").Num);
            Assert.Equal(30, data.MapValueTo<Bar>("bar").Count);

            Assert.Equal(1000, data.MapValueTo<Foo[]>("arr")[0].Num);
            Assert.Equal(data.MapValueTo<Foo>("foo").GetHashCode(), data.MapValueTo<Foo>("foo").GetHashCode());
            Assert.Equal(data.MapValueTo<Foo>("bar").GetHashCode(), data.MapValueTo<Bar>("bar").GetHashCode());

            Assert.IsType<Foo>(data["foo"]);
            Assert.IsAssignableFrom<Foo>(data["bar"]);
            Assert.IsType<Bar>(data["bar"]);
            Assert.IsType<Foo[]>(data["arr"]);
        }
    }
}
