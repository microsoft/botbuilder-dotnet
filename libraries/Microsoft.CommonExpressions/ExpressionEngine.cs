using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Microsoft.Expressions
{
    public delegate ExpressionEvaluator EvaluatorLookup(string name);

    public class ExpressionEngine
    {
        public ExpressionEngine(EvaluatorLookup lookup = null)
        {
            _lookup = lookup;
        }

        private EvaluatorLookup _lookup;

        private IParseTree AntlrParse(string expression)
        {
            var inputStream = new AntlrInputStream(expression);
            var lexer = new ExpressionLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ExpressionParser(tokenStream);
            parser.BuildParseTree = true;
            parser.ErrorHandler = new BailErrorStrategy();
            return parser.expression();
        }

        /// <summary>
        /// Parse the input into an expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>Expresion tree.</returns>
        public Expression Parse(string expression)
        {
            return new ExpressionTransformer(_lookup).Transform(AntlrParse(expression));
        }

        private class ExpressionTransformer : ExpressionBaseVisitor<Expression>
        {
            public ExpressionTransformer(EvaluatorLookup lookup)
            {
                _lookup = lookup;
            }

            private readonly EvaluatorLookup _lookup;

            public Expression Transform(IParseTree context)
            {
                return Visit(context);
            }

            private IEnumerable<Expression> ProcessArgsList(ExpressionParser.ArgsListContext context)
            {
                if (context != null)
                {
                    foreach (var expression in context.expression())
                    {
                        yield return Visit(expression);
                    }
                }
            }

            public override Expression VisitBinaryOpExp([NotNull] ExpressionParser.BinaryOpExpContext context)
            {
                var binaryOperationName = context.GetChild(1).GetText();
                var left = Visit(context.expression(0));
                var right = Visit(context.expression(1));
                return new Binary(binaryOperationName, left, right);
            }

            public override Expression VisitFuncInvokeExp([NotNull] ExpressionParser.FuncInvokeExpContext context)
            {
                var parameters = ProcessArgsList(context.argsList());

                //if context.primaryExpression() is idAtom --> normal function
                if (context.primaryExpression() is ExpressionParser.IdAtomContext idAtom)
                {
                    var functionName = idAtom.GetText();
                    return new Call(_lookup(functionName), parameters, functionName);
                }

                //if context.primaryExpression() is memberaccessExp --> accessor
                if (context.primaryExpression() is ExpressionParser.MemberAccessExpContext memberAccessExp)
                {
                    var instance = Visit(memberAccessExp.primaryExpression());
                    var functionName = memberAccessExp.IDENTIFIER().GetText();
                    return new Accessor(instance, functionName);
                }

                throw new Exception("This format is wrong.");
            }

            public override Expression VisitIdAtom([NotNull] ExpressionParser.IdAtomContext context) => new Accessor(null, context.GetText());

            public override Expression VisitIndexAccessExp([NotNull] ExpressionParser.IndexAccessExpContext context)
            {
                var instance = Visit(context.primaryExpression());
                var index = Visit(context.expression());
                return new Element(instance, index);
            }

            public override Expression VisitMemberAccessExp([NotNull] ExpressionParser.MemberAccessExpContext context)
            {
                var instance = Visit(context.primaryExpression());
                return new Accessor(instance, context.IDENTIFIER().GetText());
            }

            public override Expression VisitNumericAtom([NotNull] ExpressionParser.NumericAtomContext context)
            {
                if (int.TryParse(context.GetText(), out var intValue))
                    return new Constant(intValue);

                if (float.TryParse(context.GetText(), out var floatValue))
                    return new Constant(floatValue);

                throw new Exception($"{context.GetText()} is not a number.");
            }

            public override Expression VisitParenthesisExp([NotNull] ExpressionParser.ParenthesisExpContext context) => Visit(context.expression());

            public override Expression VisitStringAtom([NotNull] ExpressionParser.StringAtomContext context) => new Constant(context.GetText().Trim('\''));
        }

    }
}
