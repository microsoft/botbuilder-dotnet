using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class ExpressionAnalyzerVisitor : ReferenceVisitor
    {
        private readonly EvaluationContext EvaluationContext;

        public ExpressionAnalyzerVisitor(EvaluationContext evaluationContext)
        {
            EvaluationContext = evaluationContext;
        }

        public override void VisitChildren(ExpressionWithChildren expression)
        {
            TerminatePath();
            foreach (var child in expression.Children)
            {
                if (child is Constant cnst && cnst.Value is string str)
                {
                    if (str.StartsWith("[") && str.EndsWith("]"))
                    {
                        // Template reference
                        var analyzer = new Analyzer(EvaluationContext);
                        foreach (var reference in analyzer.AnalyzeTemplate(str.Substring(1, str.Length - 2)))
                        {
                            References.Add(reference);
                        }
                    }
                    else if (str.StartsWith("{") && str.EndsWith("}"))
                    {
                        // Expression
                        var expr = str.Substring(1, str.Length - 2);
                        var parse = new ExpressionEngine(new GetMethodExtensions(null).GetMethodX).Parse(expr);
                        parse.Accept(this);
                    }
                    else
                    {
                        child.Accept(this);
                    }
                }
                else
                {
                    child.Accept(this);
                }
                TerminatePath();
            }
        }
    }
}
