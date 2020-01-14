// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    [JsonConverter(typeof(NumberExpressionConverter))]
    public class NumberExpression : ExpressionProperty<float>
    {
        public NumberExpression()
        {
        }

        public NumberExpression(float value) 
            : base(value)
        {
        }

        public NumberExpression(string value)
            : base(value)
        {
        }

        public NumberExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator NumberExpression(float value) => new NumberExpression(value);
        
        public static implicit operator NumberExpression(string value) => new NumberExpression(value);
        
        public static implicit operator NumberExpression(JToken value) => new NumberExpression(value);
    }
}
