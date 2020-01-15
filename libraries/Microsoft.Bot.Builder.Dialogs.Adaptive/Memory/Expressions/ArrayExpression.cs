// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// ArrayExpression - represents a property which is either a value of array of T or a string expression to bind to a array of T.
    /// </summary>
    /// <typeparam name="T">type of object in the array.</typeparam>
    /// <remarks>String values are always be interpreted as an expression, whether it has '=' prefix or not.</remarks>
    public class ArrayExpression<T> : ExpressionProperty<List<T>>
    {
        public ArrayExpression()
        {
        }

        public ArrayExpression(List<T> value)
            : base(value)
        {
        }

        public ArrayExpression(string value)
            : base(value)
        {
        }

        public ArrayExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator ArrayExpression<T>(List<T> value) => new ArrayExpression<T>(value);

        public static implicit operator ArrayExpression<T>(string value) => new ArrayExpression<T>(value);

        public static implicit operator ArrayExpression<T>(JToken value) => new ArrayExpression<T>(value);
    }
}
