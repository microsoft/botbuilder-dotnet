// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// ValueExpression - represents a property which is an object of any kind or a string expression.
    /// </summary>
    /// <remarks>
    /// If the value is 
    /// * a string with '=' prefix then the string is treated as an expression to resolve to a string. 
    /// * a string without '=' then value is treated as string with string interpolation.
    /// * any other type, then it is of that type (int, bool, object, etc.)
    /// You can escape the '=' prefix by putting a backslash.  
    /// Examples: 
    ///     prop = true ==> true
    ///     prop = "Hello @{user.name}" => "Hello Joe"
    ///     prop = "=length(user.name)" => 3
    ///     prop = "=user.age" => 45.
    ///     prop = "\=user.age" => "=user.age".
    /// </remarks>
    [JsonConverter(typeof(ValueExpressionConverter))]
    public class ValueExpression : ExpressionProperty<object>
    {
        private LGFile lg = new LGFile();

        public ValueExpression()
        {
        }

        public ValueExpression(object value)
            : base(value)
        {
        }

        public static implicit operator ValueExpression(string value) => new ValueExpression(value);

        public static implicit operator ValueExpression(JToken value) => new ValueExpression(value);

        public override (object Value, string Error) TryGetValue(object data)
        {
            if (this.Value != null && this.Value is string val)
            {
                // value is a string (not an expression) should be interpreted as string, which means interperlated
                return (lg.Evaluate(val, data).ToString(), null);
            }

            return base.TryGetValue(data);
        }

        public override void SetValue(object value)
        {
            var stringOrExpression = (value as string) ?? (value as JValue)?.Value as string;

            if (stringOrExpression != null)
            {
                // if it starts with = it always is an expression
                if (stringOrExpression.StartsWith("="))
                {
                    Expression = new ExpressionEngine().Parse(stringOrExpression.TrimStart('='));
                    return;
                }
                else if (stringOrExpression.StartsWith("\\="))
                {
                    // then trim off the escape char for equals (\=foo) should simply be the string (=foo), and not an expression (but it could still be stringTemplate)
                    stringOrExpression = stringOrExpression.TrimStart('\\');
                }

                this.Value = stringOrExpression;
                return;
            }

            base.SetValue(value);
        }
    }
}
