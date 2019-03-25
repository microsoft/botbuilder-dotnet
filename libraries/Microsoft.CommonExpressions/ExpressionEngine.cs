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
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ExpressionErrorListener.Instance);
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

            public override Expression VisitUnaryOpExp([NotNull] ExpressionParser.UnaryOpExpContext context)
            {
                var unaryOperationName = context.GetChild(0).GetText();
                var operand = Visit(context.expression());
                return ExpressionTree.MakeExpressionTree(unaryOperationName, new List<Expression> { operand });
            }

            public override Expression VisitBinaryOpExp([NotNull] ExpressionParser.BinaryOpExpContext context)
            {
                var binaryOperationName = context.GetChild(1).GetText();
                var left = Visit(context.expression(0));
                var right = Visit(context.expression(1));
                return ExpressionTree.MakeExpressionTree(binaryOperationName, new List<Expression> { left, right });
            }

            public override Expression VisitFuncInvokeExp([NotNull] ExpressionParser.FuncInvokeExpContext context)
            {
                var parameters = ProcessArgsList(context.argsList());

                //if context.primaryExpression() is idAtom --> normal function
                if (context.primaryExpression() is ExpressionParser.IdAtomContext idAtom)
                {
                    var functionName = idAtom.GetText();
                    return ExpressionTree.MakeExpressionTree(functionName, parameters);
                }

                //if context.primaryExpression() is memberaccessExp --> accessor
                if (context.primaryExpression() is ExpressionParser.MemberAccessExpContext memberAccessExp)
                {
                    var instance = Visit(memberAccessExp.primaryExpression());
                    var functionName = memberAccessExp.IDENTIFIER().GetText();
                    return Accessor.MakeAccessor(instance, functionName);
                }

                throw new Exception("This format is wrong.");
            }

            public override Expression VisitIdAtom([NotNull] ExpressionParser.IdAtomContext context)
            {
                Expression result;
                var symbol = context.GetText();
                if (symbol == "false")
                {
                    result = Constant.MakeConstant(false);
                }
                else if (symbol == "true")
                {
                    result = Constant.MakeConstant(true);
                }
                else
                {
                    result = Accessor.MakeAccessor(null, symbol);
                }
                return result;
            }

            public override Expression VisitIndexAccessExp([NotNull] ExpressionParser.IndexAccessExpContext context)
            {
                var instance = Visit(context.primaryExpression());
                var index = Visit(context.expression());
                return ExpressionTree.MakeExpressionTree(ExpressionType.Element, new List<Expression> { instance, index });
            }

            public override Expression VisitMemberAccessExp([NotNull] ExpressionParser.MemberAccessExpContext context)
            {
                var instance = Visit(context.primaryExpression());
                return Accessor.MakeAccessor(instance, context.IDENTIFIER().GetText());
            }

            public override Expression VisitNumericAtom([NotNull] ExpressionParser.NumericAtomContext context)
            {
                if (int.TryParse(context.GetText(), out var intValue))
                    return Constant.MakeConstant(intValue);

                if (double.TryParse(context.GetText(), out var doubleValue))
                    return Constant.MakeConstant(doubleValue);

                throw new Exception($"{context.GetText()} is not a number.");
            }

            public override Expression VisitParenthesisExp([NotNull] ExpressionParser.ParenthesisExpContext context) => Visit(context.expression());

            public override Expression VisitStringAtom([NotNull] ExpressionParser.StringAtomContext context) => Constant.MakeConstant(context.GetText().Trim('\''));
        }

    }
}
