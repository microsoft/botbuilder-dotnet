// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// DialogExpression - represents a property which is either a Dialog or a string expression for a dialogId.
    /// </summary>
    /// <remarks>String values are always be interpreted as an expression, whether it has '=' prefix or not.</remarks>
    public class DialogExpression : ObjectExpression<Dialog>
    {
        public DialogExpression()
        {
        }

        public DialogExpression(Dialog value)
            : base(value)
        {
        }

        public DialogExpression(string value)
            : base(value)
        {
        }

        public DialogExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator DialogExpression(Dialog value) => new DialogExpression(value);

        public static implicit operator DialogExpression(string value) => new DialogExpression(value);

        public static implicit operator DialogExpression(JToken value) => new DialogExpression(value);

        public override void SetValue(object value)
        {
            if (value is string str)
            {
                if (!str.StartsWith("="))
                {
                    // Resource Id's will be resolved to actual dialog value
                    // if it's not a = then we want to convert to a constant string expressions to represent a 
                    // external dialog id resolved by dc.FindDialog()
                    value = $"='{str}'";
                }
            }

            base.SetValue(value);
        }
    }
}
