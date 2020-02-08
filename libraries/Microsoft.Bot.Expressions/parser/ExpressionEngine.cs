// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Expressions.parser;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions
{
    /// <summary>
    /// Parser to turn strings into an <see cref="Expression"/>.
    /// </summary>
    public class ExpressionEngine : IExpressionParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEngine"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="lookup">Delegate to lookup evaluation information from type string.</param>
        public ExpressionEngine(EvaluatorLookup lookup = null)
        {
            EvaluatorLookup = lookup ?? ExpressionFunctions.Lookup;
        }

        /// <summary>
        /// Gets the elegate to lookup function information from the type.
        /// </summary>
        /// <value>
        /// The elegate to lookup function information from the type.
        /// </value>
        public EvaluatorLookup EvaluatorLookup { get; }

        /// <summary>
        /// Parse the input into an expression.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <returns>Expression tree.</returns>
        public Expression Parse(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return Expression.ConstantExpression(string.Empty);
            }
            else
            {
                return new ExpressionTransformer(EvaluatorLookup).Transform(AntlrParse(expression));
            }
        }

        protected static IParseTree AntlrParse(string expression)
        {
            var inputStream = new AntlrInputStream(expression);
            var lexer = new ExpressionLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ExpressionParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ParserErrorListener.Instance);
            parser.BuildParseTree = true;
            return parser.file()?.expression();
        }

        private class ExpressionTransformer : ExpressionParserBaseVisitor<Expression>
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

                // Remove the check to check primaryExpression is just an IDENTIFIER to support "." in template name
                var functionName = context.primaryExpression().GetText();
                return MakeExpression(functionName, parameters.ToArray());
            }

            public override Expression VisitIdAtom([NotNull] ExpressionParser.IdAtomContext context)
            {
                Expression result;
                var symbol = context.GetText();
                var normalized = symbol.ToLower();
                if (normalized == "false")
                {
                    result = Expression.ConstantExpression(false);
                }
                else if (normalized == "true")
                {
                    result = Expression.ConstantExpression(true);
                }
                else if (normalized == "null")
                {
                    result = Expression.ConstantExpression(null);
                }
                else
                {
                    result = MakeExpression(ExpressionType.Accessor, Expression.ConstantExpression(symbol));
                }

                return result;
            }

            public override Expression VisitIndexAccessExp([NotNull] ExpressionParser.IndexAccessExpContext context)
            {
                Expression instance;
                var property = Visit(context.expression());

                instance = Visit(context.primaryExpression());
                return MakeExpression(ExpressionType.Element, instance, property);
            }

            public override Expression VisitMemberAccessExp([NotNull] ExpressionParser.MemberAccessExpContext context)
            {
                var property = context.IDENTIFIER().GetText();
                var instance = Visit(context.primaryExpression());

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

            public override Expression VisitStringInterpolationAtom([NotNull] ExpressionParser.StringInterpolationAtomContext context)
            {
                var children = new List<Expression>();
                foreach (ITerminalNode node in context.stringInterpolation().children)
                {
                    switch (node.Symbol.Type)
                    {
                        case ExpressionParser.TEMPLATE:
                            var expressionString = TrimExpression(node.GetText());
                            children.Add(new ExpressionEngine(_lookup).Parse(expressionString));
                            break;
                        case ExpressionParser.TEXT_CONTENT:
                            children.Add(Expression.ConstantExpression(node.GetText()));
                            break;
                        case ExpressionParser.ESCAPE_CHARACTER:
                            children.Add(Expression.ConstantExpression(EvalEscape(node.GetText())));
                            break;
                        default:
                            break;
                    }
                }

                return MakeExpression(ExpressionType.Concat, children.ToArray());
            }

            public override Expression VisitConstantAtom([NotNull] ExpressionParser.ConstantAtomContext context)
            {
                var text = context.GetText();
                if (text.StartsWith("[") && text.EndsWith("]") && string.IsNullOrWhiteSpace(text.Substring(1, text.Length - 2))) 
                {
                    return Expression.ConstantExpression(new JArray());
                }

                if (text.StartsWith("{") && text.EndsWith("}") && string.IsNullOrWhiteSpace(text.Substring(1, text.Length - 2)))
                {
                    return Expression.ConstantExpression(new JObject());
                }

                throw new Exception($"Unrecognized constant: {text}");
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

            private string EvalEscape(string exp)
            {
                var commonEscapes = new List<string>() { "\\r", "\\n", "\\t" };
                if (commonEscapes.Contains(exp))
                {
                    return Regex.Unescape(exp);
                }

                return exp.Substring(1);
            }

            private string TrimExpression(string expression)
            {
                var result = expression.Trim().TrimStart('@').Trim();

                if (result.StartsWith("{") && result.EndsWith("}"))
                {
                    result = result.Substring(1, result.Length - 2);
                }

                return result;
            }
        }
    }
}
