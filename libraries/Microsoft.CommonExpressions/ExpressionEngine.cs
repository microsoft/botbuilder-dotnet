using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Microsoft.Expressions
{
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
            var parser = Parse(expression);
            return Evaluate(parser, scope, getValue, getMethod);   
        }


        public static bool TryEvaluate(string expression,
                               object scope,
                               out object result,
                               GetValueDelegate getValue = null,
                               GetMethodDelegate getMethod = null)
        {
            return ExpressionEngine.TryEvaluate(expression, scope, out result, out string _);
        }

        public static bool TryEvaluate(string expression, 
                                       object scope, 
                                       out object result, 
                                       out string errorMessage,
                                       GetValueDelegate getValue = null,
                                       GetMethodDelegate getMethod = null)
        {
            try
            {
                result = ExpressionEngine.Evaluate(expression, scope, getValue, getMethod);
                errorMessage = null;
                return true;
            } catch (Exception e) {
                errorMessage = e.Message;
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Parse the input into ParserTree
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IParseTree Parse(string expression)
        {
            try
            {
                var inputStream = new AntlrInputStream(expression);
                var lexer = new ExpressionLexer(inputStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new ExpressionParser(tokenStream);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(ExpressionErrorListener.Instance);
                parser.BuildParseTree = true;

                return parser.expression();
            }
            catch (Exception e)
            {
                string msg = $"Not a valid expression: {expression}, Error: ${e.Message}";
                throw new ExpressionParsingException(msg);
            }

        }

        /// <summary>
        /// Evaluate ParseTree 
        /// </summary>
        /// <param name="parseTree"></param>
        /// <param name="scope"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        public static object Evaluate(IParseTree parseTree, object scope, GetValueDelegate getValue = null, GetMethodDelegate getMethod = null)
        {
            try
            {
                getValue = getValue ?? PropertyBinder.Auto;
                getMethod = getMethod ?? MethodBinder.All;

                var wrappedGetMethod = new GetMethodDelegateWrapper(getMethod);
                var wrappedGetValue = new GetValueDelegateWrapper(getValue);

                var evaluator = new ExpressionEvaluator(wrappedGetValue.GetValue, wrappedGetMethod.GetMethod);

                return evaluator.Evaluate(parseTree, scope);
            }
            catch (Exception e)
            {
                string msg = $"Error occurs when evaluating: {parseTree.GetText()}, Error: {e.Message}";
                throw new ExpressionEvaluationException(msg);
            }
        }
    
    }
}
