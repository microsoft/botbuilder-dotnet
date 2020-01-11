// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
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
