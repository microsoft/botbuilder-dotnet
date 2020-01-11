// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    [JsonConverter(typeof(FloatExpressionConverter))]
    public class FloatExpression : ExpressionProperty<float>
    {
        public FloatExpression()
        {
        }

        public FloatExpression(float value) 
            : base(value)
        {
        }

        public FloatExpression(string value)
            : base(value)
        {
        }

        public FloatExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator FloatExpression(float value) => new FloatExpression(value);
        
        public static implicit operator FloatExpression(string value) => new FloatExpression(value);
        
        public static implicit operator FloatExpression(JToken value) => new FloatExpression(value);
    }
}
