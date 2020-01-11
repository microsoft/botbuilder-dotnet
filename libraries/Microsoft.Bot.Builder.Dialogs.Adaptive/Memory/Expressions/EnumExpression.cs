// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class EnumExpression<T> : ExpressionProperty<T>
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

        public override (T Value, string Error) TryGetValue(object data)
        {
            return base.TryGetValue(data);
        }

        public override void SetValue(object value)
        {
            if (value is string stringOrExpression)
            {
                try
                {
                    // see if we can parse the value as an enum (we use JsonConvert so that we get camel casing behavior
                    this.Value = JsonConvert.DeserializeObject<T>($"'{stringOrExpression.TrimStart('=')}'");
                    return;
                }
                catch
                {
                    // it must be an expression, fall through and try that.
                }
            }

            base.SetValue(value);
        }
    }
}
