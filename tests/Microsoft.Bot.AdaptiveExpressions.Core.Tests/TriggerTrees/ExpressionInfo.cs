// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.AdaptiveExpressions.Core.TriggerTrees.Tests
{
    public class ExpressionInfo
    {
        public ExpressionInfo(Expression expression)
        {
            Expression = expression;
        }

        public ExpressionInfo(Expression expression, string name, object value, string type)
        {
            Expression = expression;
            Bindings.Add(name, new Comparison(type, value));
        }

        public ExpressionInfo(Expression expression, Dictionary<string, Comparison> bindings, List<Quantifier> quantifiers = null)
        {
            Expression = expression;
            Bindings = bindings;
            if (quantifiers != null)
            {
                Quantifiers = quantifiers;
            }
        }

        public Expression Expression { get; set; }

        public Dictionary<string, Comparison> Bindings { get; set; } = new Dictionary<string, Comparison>();

        public List<Quantifier> Quantifiers { get; set; } = new List<Quantifier>();

        public override string ToString() => Expression.ToString();
    }
}
