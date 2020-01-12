// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    [JsonConverter(typeof(StringExpressionConverter))]
    public class StringExpression : ExpressionProperty<string>
    {
        private LGFile lg = new LGFile();

        public StringExpression()
        {
        }

        public StringExpression(string value)
            : base(value)
        {
        }

        public StringExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator StringExpression(string value) => new StringExpression(value);

        public static implicit operator StringExpression(JToken value) => new StringExpression(value);

        public override (string Value, string Error) TryGetValue(object data)
        {
            if (this.Value != null)
            {
                // interpolated string
                return (lg.Evaluate(this.Value, data).ToString(), null);
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
