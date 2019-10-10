#pragma warning disable SA1401 // Fields should be private
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Expressions;

namespace Microsoft.Bot.Expressions.TriggerTrees.Tests
{
    public class Comparison
    {
        public string Type;
        public object Value;

        public Comparison(string type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
