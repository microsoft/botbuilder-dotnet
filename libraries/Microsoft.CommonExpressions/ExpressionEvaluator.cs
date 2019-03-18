using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Microsoft.Expressions
{
    public class ExpressionEvaluator : ExpressionBaseVisitor<object>
    {
        private readonly GetValueDelegate GetValue;
        private readonly GetMethodDelegate GetMethod;
        private object Scope;

        public ExpressionEvaluator(GetValueDelegate getValue = null, GetMethodDelegate getMethod = null)
        {
            GetValue = getValue;
            GetMethod = getMethod;
        }

        public object Evaluate(IParseTree context, object scope)
        {
            Scope = scope;
            return Visit(context);
        }

        public override object VisitArgsList([NotNull] ExpressionParser.ArgsListContext context)
        {
            var parameters = new List<object>();

            foreach (var expression in context.expression())
            {
                parameters.Add(Visit(expression));
            }
            return parameters;
        }

        public override object VisitBinaryOpExp([NotNull] ExpressionParser.BinaryOpExpContext context)
        {
            var binaryOperationName = context.GetChild(1).GetText();
            var method = MethodBinder.All(binaryOperationName);

            var left = Visit(context.expression(0));
            var right = Visit(context.expression(1));
            return method(new List<object>{left,right});
        }

        public override object VisitFuncInvokeExp([NotNull] ExpressionParser.FuncInvokeExpContext context)
        {
            var parameters = new List<object>();
            if (context.argsList() != null)
            {
                parameters = Visit(context.argsList()) as List<object>;
            }

            //if context.primaryExpression() is idAtom --> normal function
            if (context.primaryExpression() is ExpressionParser.IdAtomContext idAtom)
            {
                var functionName = idAtom.GetText();
                var method = GetMethod(functionName);
                return method(parameters.ToArray());
            }

            //if context.primaryExpression() is memberaccessExp --> lamda function
            if (context.primaryExpression() is ExpressionParser.MemberAccessExpContext memberAccessExp)
            {
                var instance = Visit(memberAccessExp.primaryExpression());
                var functionName = memberAccessExp.IDENTIFIER().GetText();
                parameters.Insert(0, instance);
                var method = GetMethod(functionName);
                return method(parameters.ToArray());
            }

            throw new Exception("This format is wrong.");
        }

        public override object VisitIdAtom([NotNull] ExpressionParser.IdAtomContext context) => GetValue(Scope, context.GetText());

        public override object VisitIndexAccessExp([NotNull] ExpressionParser.IndexAccessExpContext context)
        {
            var instance = Visit(context.primaryExpression());
            var property = Visit(context.expression());
            return GetValue(instance, property);
        }

        public override object VisitMemberAccessExp([NotNull] ExpressionParser.MemberAccessExpContext context)
        {
            var instance = Visit(context.primaryExpression());
            return GetValue(instance, context.IDENTIFIER().GetText());
        }

        public override object VisitNumericAtom([NotNull] ExpressionParser.NumericAtomContext context)
        {
            if (int.TryParse(context.GetText(), out var intValue))
                return intValue;

            if (float.TryParse(context.GetText(), out var floatValue))
                return floatValue;

            throw new Exception($"{context.GetText()} is not a number.");
        }

        public override object VisitParenthesisExp([NotNull] ExpressionParser.ParenthesisExpContext context) => Visit(context.expression());

        public override object VisitStringAtom([NotNull] ExpressionParser.StringAtomContext context) => context.GetText().Trim('\'');
    }
}
