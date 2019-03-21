using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Checker
{
    public class ExpressionChecker : ExpressionBaseVisitor<int>
    {
        private readonly EvaluationContext EvaluationContext;

        public ExpressionChecker(EvaluationContext evaluationContext)
        {
            EvaluationContext = evaluationContext;
        }

        public void Check(IParseTree context) => Visit(context);


        public override int VisitFuncInvokeExp([NotNull] ExpressionParser.FuncInvokeExpContext context)
        {
            var args = context.argsList();

            //if context.primaryExpression() is idAtom --> normal function
            if (context.primaryExpression() is ExpressionParser.IdAtomContext idAtom)
            {
                var functionName = idAtom.GetText();
                if (functionName == "foreach" //foreach(var,template)
                        || functionName == "map"//map(var,template)
                        || functionName == "mapjoin" //mapjoin(var,template,',')
                        || functionName == "humanize") //humanize(var,template,',')
                {
                    var checker = new LGFileChecker(EvaluationContext);
                    checker.CheckTemplateRef(args.expression(1).GetText());
                }
            }

            //if context.primaryExpression() is memberAccessExp --> lamda function
            if (context.primaryExpression() is ExpressionParser.MemberAccessExpContext memberAccessExp)
            {
                var functionName = memberAccessExp.IDENTIFIER().GetText();

                if (functionName == "foreach" //var.foreach(template)
                    || functionName == "map"  //var.map(template)
                    || functionName == "mapjoin" //var.mapjoin(template,',')
                    || functionName == "humanize") //var.humanize(template,',')
                {
                    var checker = new LGFileChecker(EvaluationContext);
                    checker.CheckTemplateRef(args.expression(0).GetText());
                }
            }

            return 0;
        }
    }
}
