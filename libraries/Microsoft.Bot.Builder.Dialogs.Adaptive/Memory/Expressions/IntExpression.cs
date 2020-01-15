// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// IntExpression - represents a property which is either an Integer or a string expression which resolves to a Integer.
    /// </summary>
    /// <remarks>String values are always be interpreted as an expression, whether it has '=' prefix or not.</remarks>
    [JsonConverter(typeof(IntExpressionConverter))]
    public class IntExpression : ExpressionProperty<int>
    {
        public IntExpression()
        {
        }

        public IntExpression(int value)
            : base(value)
        {
        }

        public IntExpression(string value)
            : base(value)
        {
        }

        public IntExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator IntExpression(int value) => new IntExpression(value);

        public static implicit operator IntExpression(string value) => new IntExpression(value);

        public static implicit operator IntExpression(JToken value) => new IntExpression(value);
    }
}
