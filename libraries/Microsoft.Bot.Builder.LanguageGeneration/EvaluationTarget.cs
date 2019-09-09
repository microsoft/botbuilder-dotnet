using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    internal class EvaluationTarget
    {
        public EvaluationTarget(string templateName, object scope)
        {
            TemplateName = templateName;
            Scope = scope;
        }

        public string TemplateName { get; set; }

        public object Scope { get; set; }

        /// <summary>
        /// Gets expression that evaluated by current template target.
        /// </summary>
        /// <value>
        /// Expression that evaluated by current template target.
        /// </value>
        public Dictionary<string, (object, string)> ExpressionHistory { get; } = new Dictionary<string, (object, string)>();

        public void AddExpression(string expression, (object, string) result) => ExpressionHistory[expression] = result;
    }
}
