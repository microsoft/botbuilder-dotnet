// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class ThingTests
    {
        [Fact]
        public void ThingInits()
        {
            var type = "thing";
            var name = "name";

            var thing = new Thing(type, name);

            Assert.NotNull(thing);
            Assert.IsType<Thing>(thing);
            Assert.Equal(type, thing.Type);
            Assert.Equal(name, thing.Name);
        }
        
        [Fact]
        public void ThingInitsWithNoArgs()
        {
            var thing = new Thing();

            Assert.NotNull(thing);
            Assert.IsType<Thing>(thing);
        }
    }
}
