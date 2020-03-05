﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    /// <typeparam name="T">The property type to construct.</typeparam>
    public class ExpressionConverter : JsonConverter<Expression>
    {
        public override bool CanRead => true;

        public override Expression ReadJson(JsonReader reader, Type objectType, Expression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Expression.Parse((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, Expression value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }
}
