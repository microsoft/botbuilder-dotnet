// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.Expressions.Parser
{
    /// <summary>
    /// Parser to turn strings into an <see cref="Expression"/>.
    /// </summary>
    public class ExpressionEngine : IExpressionParser
    {
        private readonly EvaluatorLookup _lookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEngine"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="lookup">If present delegate to lookup evaluation information from type string.</param>
        public ExpressionEngine(EvaluatorLookup lookup = null)
        {
            _lookup = lookup ?? BuiltInFunctions.Lookup;
        }

        /// <summary>
        /// Parse the input into an expression.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <returns>Expresion tree.</returns>
        public Expression Parse(string expression) => new ExpressionTransformer(_lookup).Transform(AntlrParse(expression));

        private IParseTree AntlrParse(string expression)
        {
            var inputStream = new AntlrInputStream(expression);
            var lexer = new ExpressionLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ExpressionParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ErrorListener.Instance);
            parser.BuildParseTree = true;
            return parser.expression();
        }

        private class ExpressionTransformer : ExpressionBaseVisitor<Expression>
        {
            private readonly EvaluatorLookup _lookup;

            public ExpressionTransformer(EvaluatorLookup lookup)
            {
                _lookup = lookup;
            }

            public Expression Transform(IParseTree context) => Visit(context);

            public override Expression VisitUnaryOpExp([NotNull] ExpressionParser.UnaryOpExpContext context)
            {
                var unaryOperationName = context.GetChild(0).GetText();
                var operand = Visit(context.expression());
                if (unaryOperationName == ExpressionType.Subtract
                    || unaryOperationName == ExpressionType.Add)
                {
                    return MakeExpression(unaryOperationName, new Constant(0), operand);
                }

                return MakeExpression(unaryOperationName, operand);
            }

            public override Expression VisitBinaryOpExp([NotNull] ExpressionParser.BinaryOpExpContext context)
            {
                var binaryOperationName = context.GetChild(1).GetText();
                var left = Visit(context.expression(0));
                var right = Visit(context.expression(1));
                return MakeExpression(binaryOperationName, left, right);
            }

            public override Expression VisitFuncInvokeExp([NotNull] ExpressionParser.FuncInvokeExpContext context)
            {
                var parameters = ProcessArgsList(context.argsList()).ToList();

                // Current only IdAtom is supported as function name
                if (context.primaryExpression() is ExpressionParser.IdAtomContext functionNameItem)
                {
                    var functionName = functionNameItem.GetText();
                    return MakeExpression(functionName, parameters.ToArray());
                }

                throw new Exception($"This format is wrong in expression '{context.GetText()}'");
            }

            public override Expression VisitIdAtom([NotNull] ExpressionParser.IdAtomContext context)
            {
                Expression result;
                var symbol = context.GetText();
                if (symbol == "false")
                {
                    result = Expression.ConstantExpression(false);
                }
                else if (symbol == "true")
                {
                    result = Expression.ConstantExpression(true);
                }
                else if (symbol == "null")
                {
                    result = Expression.ConstantExpression(null);
                }
                else if (IsShortHandExpression(symbol))
                {
                    result = MakeShortHandExpression(symbol);
                }
                else
                {
                    result = MakeExpression(ExpressionType.Accessor, Expression.ConstantExpression(symbol));
                }

                return result;
            }

            public override Expression VisitIndexAccessExp([NotNull] ExpressionParser.IndexAccessExpContext context)
            {
                var instance = Visit(context.primaryExpression());
                var index = Visit(context.expression());
                return MakeExpression(ExpressionType.Element, instance, index);
            }

            public override Expression VisitMemberAccessExp([NotNull] ExpressionParser.MemberAccessExpContext context)
            {
                var instance = Visit(context.primaryExpression());
                var property = context.IDENTIFIER().GetText();

                if (IsShortHandExpression(property))
                {
                    throw new Exception($"shorthand like {property} is not allowed in an accessor in expression '{context.GetText()}'");
                }

                return MakeExpression(ExpressionType.Accessor, Expression.ConstantExpression(property), instance);
            }

            public override Expression VisitNumericAtom([NotNull] ExpressionParser.NumericAtomContext context)
            {
                if (int.TryParse(context.GetText(), out var intValue))
                {
                    return Expression.ConstantExpression(intValue);
                }

                if (double.TryParse(context.GetText(), out var doubleValue))
                {
                    return Expression.ConstantExpression(doubleValue);
                }

                throw new Exception($"{context.GetText()} is not a number in expression '{context.GetText()}'");
            }

            public override Expression VisitParenthesisExp([NotNull] ExpressionParser.ParenthesisExpContext context) => Visit(context.expression());

            public override Expression VisitStringAtom([NotNull] ExpressionParser.StringAtomContext context)
            {
                var text = context.GetText();
                if (text.StartsWith("'"))
                {
                    return Expression.ConstantExpression(Regex.Unescape(text.Trim('\'')));
                }
                else
                {
                    // start with "
                    return Expression.ConstantExpression(Regex.Unescape(text.Trim('"')));
                }
            }

            private Expression MakeExpression(string type, params Expression[] children)
                => Expression.MakeExpression(_lookup(type), children);

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

            private bool IsShortHandExpression(string name)
                => name.StartsWith("#") || name.StartsWith("@") || name.StartsWith("$");

            private Expression MakeShortHandExpression(string name)
            {
                if (!IsShortHandExpression(name))
                {
                    throw new Exception($"variable name:{name} is not a shorthand");
                }

                var prefix = name[0];
                name = name.Substring(1);

                // $title == dialog.result.title
                // @city == turn.entities.city
                // #BookFlight == turn.intents.BookFlight
                switch (prefix)
                {
                    case '#':
                        return MakeExpression(
                            ExpressionType.Accessor,
                            Expression.ConstantExpression(name),
                            MakeExpression(
                                ExpressionType.Accessor,
                                Expression.ConstantExpression("intents"),
                                MakeExpression(ExpressionType.Accessor, Expression.ConstantExpression("turn"))));

                    case '@':
                        return MakeExpression(
                            ExpressionType.Accessor,
                            Expression.ConstantExpression(name),
                            MakeExpression(
                                ExpressionType.Accessor,
                                Expression.ConstantExpression("entities"),
                                MakeExpression(ExpressionType.Accessor, Expression.ConstantExpression("turn"))));

                    case '$':
                        return MakeExpression(
                            ExpressionType.Accessor,
                            Expression.ConstantExpression(name),
                            MakeExpression(
                                ExpressionType.Accessor,
                                Expression.ConstantExpression("result"),
                                MakeExpression(ExpressionType.Accessor, Expression.ConstantExpression("dialog"))));
                }

                throw new Exception($"no match for shorthand prefix: {prefix}");
            }
        }
    }
}
