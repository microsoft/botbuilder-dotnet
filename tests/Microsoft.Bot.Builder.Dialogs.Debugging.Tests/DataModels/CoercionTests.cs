// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging.DataModels;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.DataModels
{
    public sealed class CoercionTests
    {
        [Fact]
        public void Coercion_Coerce()
        {
            ICoercion coercion = new Coercion();

            var result = coercion.Coerce("source", typeof(string));

            Assert.Equal("source", result);
        }

        [Fact]
        public void Coercion_Coerce_JToken()
        {
            ICoercion coercion = new Coercion();
            var writer = new JTokenWriter();
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("request");
            writer.WritePropertyName("command");
            writer.WriteValue("launch");
            writer.WriteEndObject();

            var result = coercion.Coerce(writer.Token, typeof(JToken));

            Assert.Equal(writer.Token, result);
        }
    }
}
