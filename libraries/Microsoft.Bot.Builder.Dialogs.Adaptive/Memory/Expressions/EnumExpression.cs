// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// EnumExpression - represents a property which is either a enum(T) or a string expression which resolves to a enum(T).
    /// </summary>
    /// <typeparam name="T">type of enum.</typeparam>
    /// <remarks>String values are always be interpreted as an expression, whether it has '=' prefix or not.</remarks>
    public class EnumExpression<T> : ExpressionProperty<T>
        where T : struct
    {
        public EnumExpression()
        {
        }

        public EnumExpression(T value)
            : base(value)
        {
        }

        public EnumExpression(string value)
            : base(value)
        {
        }

        public EnumExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator EnumExpression<T>(T value) => new EnumExpression<T>(value);

        public static implicit operator EnumExpression<T>(string value) => new EnumExpression<T>(value);

        public static implicit operator EnumExpression<T>(JToken value) => new EnumExpression<T>(value);

        public override void SetValue(object value)
        {
            if (value is string stringOrExpression)
            {
                // if the expression is the enum value, then use that as the value, else it is an expression.
                if (Enum.TryParse<T>(stringOrExpression.TrimStart('='), ignoreCase: true, out T val))
                {
                    this.Value = val;
                    return;
                }
            }

            base.SetValue(value);
        }
    }
}
