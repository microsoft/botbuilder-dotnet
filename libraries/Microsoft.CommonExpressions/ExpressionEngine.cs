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

        /// <summary>
        /// Parse the input into ParserTree
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IParseTree Parse(string expression)
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

        /// <summary>
        /// Evaluate ParseTree 
        /// </summary>
        /// <param name="parseTree"></param>
        /// <param name="scope"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        public static object Evaluate(IParseTree parseTree, object scope, GetValueDelegate getValue = null, GetMethodDelegate getMethod = null)
        {
            getValue = getValue ?? PropertyBinder.Auto;
            getMethod = getMethod ?? MethodBinder.All;
            var evaluator = new ExpressionEvaluator(getValue, getMethod);

            return evaluator.Evaluate(parseTree, scope);
        }
    
    }
}
