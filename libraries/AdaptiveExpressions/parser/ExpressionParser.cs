// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AdaptiveExpressions.parser;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json.Linq;

[assembly: CLSCompliant(false)]

namespace AdaptiveExpressions
{
    /// <summary>
    /// Parser to turn strings into an <see cref="Expression"/>.
    /// </summary>
    public class ExpressionParser : IExpressionParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionParser"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="lookup">Delegate to lookup evaluation information from type string.</param>
        public ExpressionParser(EvaluatorLookup lookup = null)
        {
            EvaluatorLookup = lookup ?? Expression.Lookup;
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
            var lexer = new ExpressionAntlrLexer(inputStream);
            lexer.RemoveErrorListeners();
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ExpressionAntlrParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ParserErrorListener.Instance);
            parser.BuildParseTree = true;
            return parser.file()?.expression();
        }

        private class ExpressionTransformer : ExpressionAntlrParserBaseVisitor<Expression>
        {
            private readonly Regex escapeRegex = new Regex(@"\\[^\r\n]?");
            private readonly EvaluatorLookup _lookupFunction;

            public ExpressionTransformer(EvaluatorLookup lookup)
            {
                _lookupFunction = lookup;
            }

            public Expression Transform(IParseTree context) => Visit(context);

            public override Expression VisitUnaryOpExp([NotNull] ExpressionAntlrParser.UnaryOpExpContext context)
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

            public override Expression VisitBinaryOpExp([NotNull] ExpressionAntlrParser.BinaryOpExpContext context)
            {
                var binaryOperationName = context.GetChild(1).GetText();
                var left = Visit(context.expression(0));
                var right = Visit(context.expression(1));
                return MakeExpression(binaryOperationName, left, right);
            }

            public override Expression VisitFuncInvokeExp([NotNull] ExpressionAntlrParser.FuncInvokeExpContext context)
            {
                var parameters = ProcessArgsList(context.argsList()).ToList();

                // Remove the check to check primaryExpression is just an IDENTIFIER to support "." in template name
                var functionName = context.primaryExpression().GetText();
                if (context.NON() != null)
                {
                    functionName += context.NON().GetText();
                }

                return MakeExpression(functionName, parameters.ToArray());
            }

            public override Expression VisitIdAtom([NotNull] ExpressionAntlrParser.IdAtomContext context)
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

            public override Expression VisitIndexAccessExp([NotNull] ExpressionAntlrParser.IndexAccessExpContext context)
            {
                Expression instance;
                var property = Visit(context.expression());

                instance = Visit(context.primaryExpression());
                return MakeExpression(ExpressionType.Element, instance, property);
            }

            public override Expression VisitMemberAccessExp([NotNull] ExpressionAntlrParser.MemberAccessExpContext context)
            {
                var property = context.IDENTIFIER().GetText();
                var instance = Visit(context.primaryExpression());

                return MakeExpression(ExpressionType.Accessor, Expression.ConstantExpression(property), instance);
            }

            public override Expression VisitNumericAtom([NotNull] ExpressionAntlrParser.NumericAtomContext context)
            {
                if (int.TryParse(context.GetText(), out var intValue))
                {
                    return Expression.ConstantExpression(intValue);
                }

                if (double.TryParse(context.GetText(), NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
                {
                    return Expression.ConstantExpression(doubleValue);
                }

                throw new Exception($"{context.GetText()} is not a number in expression '{context.GetText()}'");
            }

            public override Expression VisitParenthesisExp([NotNull] ExpressionAntlrParser.ParenthesisExpContext context) => Visit(context.expression());

            public override Expression VisitArrayCreationExp([NotNull] ExpressionAntlrParser.ArrayCreationExpContext context)
            {
                var parameters = ProcessArgsList(context.argsList()).ToList();
                return MakeExpression(ExpressionType.CreateArray, parameters.ToArray());
            }

            public override Expression VisitStringAtom([NotNull] ExpressionAntlrParser.StringAtomContext context)
            {
                var text = context.GetText();
                if (text.StartsWith("'") && text.EndsWith("'"))
                {
                    text = text.Substring(1, text.Length - 2).Replace("\\'", "'");
                }
                else if (text.StartsWith("\"") && text.EndsWith("\""))
                {
                    text = text.Substring(1, text.Length - 2).Replace("\\\"", "\"");
                }
                else
                {
                    throw new Exception($"Invalid string {text}");
                }

                return Expression.ConstantExpression(EvalEscape(text));
            }

            public override Expression VisitJsonCreationExp([NotNull] ExpressionAntlrParser.JsonCreationExpContext context)
            {
                var expr = this.MakeExpression(ExpressionType.Json, new Constant("{}"));
                if (context.keyValuePairList() != null)
                {
                    foreach (var kvPair in context.keyValuePairList().keyValuePair())
                    {
                        var key = string.Empty;
                        var keyNode = kvPair.key().children[0];
                        if (keyNode is ITerminalNode node)
                        {
                            if (node.Symbol.Type == ExpressionAntlrParser.IDENTIFIER)
                            {
                                key = node.GetText();
                            }
                            else
                            {
                                key = node.GetText().Substring(1, node.GetText().Length - 2);
                            }
                        }

                        expr = this.MakeExpression(ExpressionType.SetProperty, expr, new Constant(key), this.Visit(kvPair.expression()));
                    }
                }

                return expr;
            }

            public override Expression VisitStringInterpolationAtom([NotNull] ExpressionAntlrParser.StringInterpolationAtomContext context)
            {
                var children = new List<Expression>();
                foreach (var child in context.stringInterpolation().children)
                {
                    if (child is ITerminalNode node)
                    {
                        switch (node.Symbol.Type)
                        {
                            case ExpressionAntlrParser.TEMPLATE:
                                var expressionString = TrimExpression(node.GetText());
                                children.Add(Expression.Parse(expressionString, _lookupFunction));
                                break;
                            case ExpressionAntlrParser.ESCAPE_CHARACTER:
                                children.Add(Expression.ConstantExpression(EvalEscape(node.GetText().Replace("\\`", "`").Replace("\\$", "$"))));
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        // text content
                        var text = EvalEscape(child.GetText());
                        children.Add(Expression.ConstantExpression(text));
                    }
                }

                return MakeExpression(ExpressionType.Concat, children.ToArray());
            }

            private Expression MakeExpression(string functionType, params Expression[] children)
                => Expression.MakeExpression(_lookupFunction(functionType) ?? throw new SyntaxErrorException($"{functionType} does not have an evaluator, it's not a built-in function or a custom function."), children);

            private IEnumerable<Expression> ProcessArgsList(ExpressionAntlrParser.ArgsListContext context)
            {
                if (context != null)
                {
                    foreach (var expression in context.expression())
                    {
                        yield return Visit(expression);
                    }
                }
            }

            private string EvalEscape(string text)
            {
                if (text == null)
                {
                    return string.Empty;
                }

                return escapeRegex.Replace(text, new MatchEvaluator(m =>
                {
                    var value = m.Value;
                    var commonEscapes = new List<string>() { "\\r", "\\n", "\\t", "\\\\" };
                    if (commonEscapes.Contains(value))
                    {
                        return Regex.Unescape(value);
                    }

                    return value;
                }));
            }

            private string TrimExpression(string expression)
            {
                var result = expression.Trim().TrimStart('$').Trim();

                if (result.StartsWith("{") && result.EndsWith("}"))
                {
                    result = result.Substring(1, result.Length - 2);
                }

                return result;
            }
        }
    }
}
