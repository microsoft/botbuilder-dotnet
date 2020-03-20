// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// DialogExpression - represents a property which is either a Dialog or a string expression for a dialogId.
    /// </summary>
    /// <remarks>String values are always interpreted as a string with interpolation, unless it has '=' prefix or not. The result is interpreted as a resource Id or dialogId.</remarks>
    public class DialogExpression : ObjectExpression<Dialog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogExpression"/> class.
        /// </summary>
        public DialogExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogExpression"/> class.
        /// </summary>
        /// <param name="value">dialog value.</param>
        public DialogExpression(Dialog value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogExpression"/> class.
        /// </summary>
        /// <param name="dialogIdOrExpression">dialogId or expression to dialogId.</param>
        public DialogExpression(string dialogIdOrExpression)
            : base(dialogIdOrExpression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogExpression"/> class.
        /// </summary>
        /// <param name="value">JToken which is either dialog or dialogId.</param>
        public DialogExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator DialogExpression(Dialog value) => new DialogExpression(value);

        public static implicit operator DialogExpression(string dialogIdOrExpression) => new DialogExpression(dialogIdOrExpression);

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

        public override string ToString()
        {
            if (this.Value != null)
            {
                return this.Value.Id;
            }

            return base.ToString();
        }
    }
}
