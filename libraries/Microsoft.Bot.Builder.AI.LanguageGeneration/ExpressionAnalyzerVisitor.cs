using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class ExpressionAnalyzerVisitor : ExpressionBaseVisitor<List<string>>
    {
        private readonly EvaluationContext EvaluationContext;

        public ExpressionAnalyzerVisitor(EvaluationContext evaluationContext)
        {
            EvaluationContext = evaluationContext;
        }

        public List<string> Analyzer(IParseTree context) => Visit(context);

        public override List<string> VisitBinaryOpExp([NotNull] ExpressionParser.BinaryOpExpContext context)
        {
            var result = new List<string>();
            result.AddRange(Visit(context.expression(0)));
            result.AddRange(Visit(context.expression(1)));

            return result;
        }

        public override List<string> VisitFuncInvokeExp([NotNull] ExpressionParser.FuncInvokeExpContext context)
        {
            var result = new List<string>();

            var args = context.argsList();

            //if context.primaryExpression() is idAtom --> normal function
            if (context.primaryExpression() is ExpressionParser.IdAtomContext idAtom)
            {
                var functionName = idAtom.GetText();
                if(functionName == "count")// count(var)
                {
                    result.AddRange(Visit(args.expression(0)));
                }
                else if(functionName == "join") // join(var, ',','and')
                {
                    foreach(var expression in args.expression())
                    {
                        result.AddRange(Visit(expression));
                    }
                }
                else
                {
                    if (functionName == "foreach" //foreach(var,templateRef)
                        || functionName == "map"//map(var,templateRef)
                        || functionName == "mapjoin" //mapjoin(var,template,',')
                        || functionName == "humanize") //humanize(var,template,',')
                    {
                        result.AddRange(Visit(args.expression(0)));
                        var analyzer = new Analyzer(EvaluationContext);
                        result.AddRange(analyzer.AnalyzeTemplate(args.expression(1).GetText()));
                        if(args.expression().Length > 2)
                        {
                            for(var i=2;i< args.expression().Length; i++)
                            {
                                result.AddRange(Visit(args.expression(i)));
                            }
                        }
                    }
                }
            }

            //if context.primaryExpression() is memberAccessExp --> lamda function
            if (context.primaryExpression() is ExpressionParser.MemberAccessExpContext memberAccessExp)
            {
                var primaryExpressionResult = Visit(memberAccessExp.primaryExpression());
                result.AddRange(primaryExpressionResult);

                var functionName = memberAccessExp.IDENTIFIER().GetText();

                if (functionName == "foreach" //var.foreach(templateRef)
                    || functionName == "map"  //var.map(templateRef)
                    || functionName == "mapjoin" //var.mapjoin(template,',')
                    || functionName == "humanize") //var.humanize(template,',')
                {
                    var analyzer = new Analyzer(EvaluationContext);
                    result.AddRange(analyzer.AnalyzeTemplate(args.expression(0).GetText()));
                    if (args.expression().Length > 1)
                    {
                        for (var i = 1; i < args.expression().Length; i++)
                        {
                            result.AddRange(Visit(args.expression(i)));
                        }
                    }
                }
            }

            return result;
        }

        public override List<string> VisitIdAtom([NotNull] ExpressionParser.IdAtomContext context) => new List<string>() { context.GetText() };

        public override List<string> VisitIndexAccessExp([NotNull] ExpressionParser.IndexAccessExpContext context)
        {
            var result = new List<string>();
            result.AddRange(Visit(context.primaryExpression()));
            result.AddRange(Visit(context.expression()));
            return result;
        }

        public override List<string> VisitMemberAccessExp([NotNull] ExpressionParser.MemberAccessExpContext context) => Visit(context.primaryExpression());

        public override List<string> VisitNumericAtom([NotNull] ExpressionParser.NumericAtomContext context) => new List<string>();

        public override List<string> VisitParenthesisExp([NotNull] ExpressionParser.ParenthesisExpContext context) => Visit(context.expression());

        public override List<string> VisitStringAtom([NotNull] ExpressionParser.StringAtomContext context) => new List<string>();
    }
}
