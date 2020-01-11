// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    [JsonConverter(typeof(LongExpressionConverter))]
    public class LongExpression : ExpressionProperty<long>
    {
        public LongExpression()
        {
        }

        public LongExpression(long value)
            : base(value)
        {
        }

        public LongExpression(string value)
            : base(value)
        {
        }

        public LongExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator LongExpression(long value) => new LongExpression(value);

        public static implicit operator LongExpression(string value) => new LongExpression(value);

        public static implicit operator LongExpression(JToken value) => new LongExpression(value);
    }
}
