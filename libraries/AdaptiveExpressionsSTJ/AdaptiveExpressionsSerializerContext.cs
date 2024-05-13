// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using AdaptiveExpressions.Properties;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Json serializer context for all AdaptiveExpressions types.
    /// </summary>
    [JsonSerializable(typeof(IntExpression))]
    [JsonSerializable(typeof(TimexProperty))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(double))]
    internal partial class AdaptiveExpressionsSerializerContext : JsonSerializerContext
    {
    }
}
