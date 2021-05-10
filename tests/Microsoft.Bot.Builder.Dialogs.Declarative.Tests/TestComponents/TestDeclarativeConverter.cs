// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests.TestComponents
{
    internal class TestDeclarativeConverter : JsonConverter<TestDeclarativeType>
    {
        public override TestDeclarativeType ReadJson(JsonReader reader, Type objectType, TestDeclarativeType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new TestDeclarativeType("fromConverter");
        }

        public override void WriteJson(JsonWriter writer, TestDeclarativeType value, JsonSerializer serializer)
        {
        }
    }
}
