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
        /// <summary>
        /// constant short hand currently have.
        /// </summary>
        private static readonly Dictionary<string, string> ShorthandPrefixMap = new Dictionary<string, string>()
        {
            { "#", $"turn.recognized.intents" },
            { "@", $"turn.recognized.entities" },
            { "@@", $"turn.recognized.entities" },
            { "$", $"" },
            { "%", $"dialog.options" },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEngine"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="lookup">If present delegate to lookup evaluation information from type string.</param>
        public ExpressionEngine(EvaluatorLookup lookup = null)
        {
            EvaluatorLookup = lookup ?? BuiltInFunctions.Lookup;
        }

        public EvaluatorLookup EvaluatorLookup { get; }

        /// <summary>
        /// Parse the input into an expression.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <returns>Expresion tree.</returns>
        public Expression Parse(string expression) => new ExpressionTransformer(EvaluatorLookup).Transform(AntlrParse(expression));

        protected static IParseTree AntlrParse(string expression)
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

            public override Expression VisitShorthandAccessorExp([NotNull] ExpressionParser.ShorthandAccessorExpContext context)
            {
                if (context.primaryExpression() is ExpressionParser.ShorthandAtomContext shorthandAtom)
                {
                    var shorthandMark = shorthandAtom.GetText();

                    var property = Expression.ConstantExpression(context.IDENTIFIER().GetText());

                    if (shorthandMark == "$")
                    {
                        return MakeExpression(ExpressionType.CallstackScope, property);
                    }

                    if (!ShorthandPrefixMap.ContainsKey(shorthandMark))
                    {
                        throw new Exception($"{shorthandMark} is not a shorthand");
                    }

                    var accessorExpression = this.Transform(AntlrParse(ShorthandPrefixMap[shorthandMark]));
                    var expression = MakeExpression(ExpressionType.Accessor, property, accessorExpression);

                    return shorthandMark == "@" ? MakeExpression(ExpressionType.SimpleEntity, expression) : expression;
                }

                throw new Exception($"{context.primaryExpression().GetText()} is not a shorthand.");
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

                if (context.primaryExpression() is ExpressionParser.ShorthandAtomContext shorthandAtom)
                {
                    var shorthandMark = shorthandAtom.GetText();

                    if (shorthandMark == "$")
                    {
                        return MakeExpression(ExpressionType.CallstackScope, property);
                    }

                    if (!ShorthandPrefixMap.ContainsKey(shorthandMark))
                    {
                        throw new Exception($"{shorthandMark} is not a shorthand");
                    }

                    instance = this.Transform(AntlrParse(ShorthandPrefixMap[shorthandMark]));
                    var expression = MakeExpression(ExpressionType.Element, instance, property);

                    return shorthandMark == "@" ? MakeExpression(ExpressionType.SimpleEntity, expression) : expression;
                }

                instance = Visit(context.primaryExpression());
                return MakeExpression(ExpressionType.Element, instance, property);
            }

            public override Expression VisitMemberAccessExp([NotNull] ExpressionParser.MemberAccessExpContext context)
            {
                var property = context.IDENTIFIER().GetText();
                if (context.primaryExpression() is ExpressionParser.ShorthandAtomContext shorthandAtom)
                {
                    throw new Exception($"{context.GetText()} is not a valid shorthand. Maybe you mean '{context.primaryExpression().GetText()}{property}'?");
                }

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
        }
    }
}
