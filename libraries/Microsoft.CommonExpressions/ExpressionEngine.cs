using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Microsoft.Expressions
{
    /// <summary>
    /// Operator binding direction.
    /// </summary>
    public enum BindingDirection
    {
        Left,
        Free,
        Right
    };

    public static class ExpressionEngine
    {
        /// <summary>
        /// Evaluate expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="scope"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        public static object Evaluate(string expression, object scope, GetValueDelegate getValue = null, GetMethodDelegate getMethod = null)
        {
            var term = Parse(expression);
            return Evaluate(term, scope, getValue, getMethod);
        }


        public static object EvaluatrWithAntlr(string expression, object scope, GetValueDelegate getValue = null, GetMethodDelegate getMethod = null)
        {
            var inputStream = new AntlrInputStream(expression);
            var lexer = new ExpressionLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ExpressionParser(tokenStream);
            parser.BuildParseTree = true;
            parser.ErrorHandler = new BailErrorStrategy();

            var exp = parser.expression();
            var evaluator = new ExpressionEvaluator();

            var result = evaluator.Evaluate(exp, scope, getValue, getMethod);
            return result;
        }


        /// <summary>
        /// Parse the input into Term
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Term Parse(string expression)
        {
            using (var tokens = InnerLexer.Tokens(expression).GetEnumerator())
            {
                InnerLexer.Next(tokens);
                return Expression(tokens, 0);
            }
        }

        /// <summary>
        /// Evaluate Term 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="scope"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        public static object Evaluate(Term term, object scope, GetValueDelegate getValue = null, GetMethodDelegate getMethod = null)
        {
            getValue = getValue ?? PropertyBinder.Auto;
            getMethod = getMethod ?? MethodBinder.All;

            var token = term.Token;
            var value = token.Value;

            // token value is identifier -> look up binding in scope
            var identifier = value as InnerLexer.Identifier;
            if (identifier != null)
            {
                return getValue(scope, identifier.Name);
            }

            // otherwise token literal value should be evaluated to a constant value
            if (value != null)
            {
                return value;
            }

            // special handling for operators
            // 1. without eagerly evaluated operands, or
            // 2. require access to the environment
            switch (token.Input)
            {
                case ".":
                    {
                        var instance = Evaluate(term.Terms[0], scope, getValue, getMethod);
                        return getValue(instance, term.Terms[1].Token.Input);
                    }
                case "[":
                    {
                        var instance = Evaluate(term.Terms[0], scope, getValue, getMethod);
                        var index = Evaluate(term.Terms[1], scope, getValue, getMethod);
                        return getValue(instance, index);
                    }
                case "(":
                    {
                        var name = term.Terms[0].Token.Input;

                        if (name.Equals("."))
                        {
                            var method = getMethod(term.Terms[0].Terms[1].Token.Input);
                            var instance = Evaluate(term.Terms[0].Terms[0], scope, getValue, getMethod);
                            var parameters = term.Terms.Skip(1).Select(t => Evaluate(t, scope, getValue, getMethod)).ToList();
                            parameters.Insert(0, instance);
                            return method(parameters);
                        }
                        else
                        {
                            var method = getMethod(name);
                            var parameters = term.Terms.Skip(1).Select(t => Evaluate(t, scope, getValue, getMethod)).ToArray();
                            return method(parameters);
                        }
                    }
            }

            // otherwise look in table for operators with eagerly evaluated operands
            var entry = term.Entry;
            if (entry != null)
            {
                var eager = entry.Evaluate;
                if (eager != null)
                {
                    var terms = term.Terms.Select(t => Evaluate(t, scope, getValue, getMethod)).ToArray();
                    if (terms.Length < entry.MinArgs || terms.Length > entry.MaxArgs)
                    {
                        throw new Exception();
                    }

                    return eager(terms);
                }
            }

            throw new NotImplementedException();
        }


        // https://en.wikipedia.org/wiki/Operator-precedence_parser
        // https://crockford.com/javascript/tdop/tdop.html
        // https://eli.thegreenplace.net/2010/01/02/top-down-operator-precedence-parsing
        // https://www.oilshell.org/blog/2016/11/01.html
        // http://effbot.org/zone/simple-top-down-parsing.htm

        private static int LeftBindingPower(Token token) =>
            OperatorTable.InfixByToken.TryGetValue(token.Input, out var entry)
            ? entry.Power
            : 0;

        private static Term Prefix(IEnumerator<Token> tokens)
        {
            var token = tokens.Current;
            InnerLexer.Next(tokens);
            if (OperatorTable.PrefixByToken.TryGetValue(token.Input, out var prefix))
            {
                return Term.From(token, prefix, Expression(tokens, prefix.Power));
            }

            switch (token.Input)
            {
                // special handling for parenthesized expressions
                case "(":
                    {
                        var term = Expression(tokens, 0);
                        InnerLexer.Match(tokens, ")");
                        return term;
                    }
                default:
                    return Term.From(token, null);
            }
        }

        private static Term Infix(IEnumerator<Token> tokens, Term left)
        {
            var token = tokens.Current;
            InnerLexer.Next(tokens);

            if (OperatorTable.InfixByToken.TryGetValue(token.Input, out var infix))
            {
                // special handling for method invocations
                switch (token.Input)
                {
                    case "(":
                        {
                            var terms = new List<Term>() { left };
                            if (tokens.Current.Input != ")")
                            {
                                while (true)
                                {
                                    var term = Expression(tokens, 0);
                                    terms.Add(term);
                                    if (tokens.Current.Input != ",")
                                    {
                                        break;
                                    }
                                    InnerLexer.Match(tokens, ",");
                                }
                            }

                            InnerLexer.Match(tokens, ")");
                            return Term.From(token, infix, terms.ToArray());
                        }
                    case "[":
                        {
                            var terms = new[] {
                                left,
                                Expression(tokens, 0)
                            };
                            InnerLexer.Match(tokens, "]");
                            return Term.From(token, infix, terms);
                        }
                    default:
                        {
                            var power = infix.Power - (infix.Direction == BindingDirection.Right ? 1 : 0);
                            return Term.From(token, infix, left, Expression(tokens, power));
                        }
                }
            }

            throw new NotImplementedException();
        }

        private static Term Expression(IEnumerator<Token> tokens, int rightBindingPower)
        {
            var left = Prefix(tokens);
            while (rightBindingPower < LeftBindingPower(tokens.Current))
            {
                left = Infix(tokens, left);
            }

            return left;
        }

    }
}
