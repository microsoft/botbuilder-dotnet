using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class PropertyPathExpression : StringExpression
    {
        public PropertyPathExpression()
            : base()
        {
        }

        public PropertyPathExpression(string value)
        {
            SetValue($"={value.TrimStart('=')}");
        }

        public PropertyPathExpression(JToken value)
        {
            SetValue($"={value.ToString().TrimStart('=')}");
        }
    }
}
