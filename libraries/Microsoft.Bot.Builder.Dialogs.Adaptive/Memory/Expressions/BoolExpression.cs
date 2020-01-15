// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// BoolExpression - represents a property which is either a boolean or a string expression which resolves to a boolean.
    /// </summary>
    /// <remarks>String values are always be interpreted as an expression, whether it has '=' prefix or not.</remarks>
    [JsonConverter(typeof(BoolExpressionConverter))]
    public class BoolExpression : ExpressionProperty<bool>
    {
        public BoolExpression()
        {
        }

        public BoolExpression(bool value) 
            : base(value)
        {
        }

        public BoolExpression(string value)
            : base(value)
        {
        }

        public BoolExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator BoolExpression(bool value) => new BoolExpression(value);

        public static implicit operator BoolExpression(string value) => new BoolExpression(value);

        public static implicit operator BoolExpression(JToken value) => new BoolExpression(value);
    }
}
