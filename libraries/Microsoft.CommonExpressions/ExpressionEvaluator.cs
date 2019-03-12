using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Linq;

namespace Microsoft.Expressions
{
    public class ExpressionEvaluator : ExpressionBaseVisitor<object>
    {
        private  GetValueDelegate GetValue;
        private  GetMethodDelegate GetMethod;
        private  object Scope;

        public object Evaluate(ExpressionParser.ExpressionContext context, object scope, GetValueDelegate getValue = null, GetMethodDelegate getMethod = null)
        {
            Scope = scope;
            GetValue = getValue;
            GetMethod = getMethod;
            var result = Visit(context);
            return result;
        }


        public override object VisitAddSubExp([NotNull] ExpressionParser.AddSubExpContext context)
        {
            var list = context.children;
            if (list.Count == 2)//format like:+1,-2
            {
                var firstChild = (ITerminalNode)list[0];
                var valusStr = Visit(list[1]);
                var isInt = int.TryParse(valusStr.ToString(), out var secondValue);
                if (!isInt)
                {
                    throw new Exception($"{list[1].GetText()} is not a number.");
                }

                switch (firstChild.Symbol.Type)
                {
                    case ExpressionParser.PLUS:return secondValue;
                    case ExpressionParser.MINUS:return -secondValue;
                    default: throw new Exception($"{firstChild.Symbol.Type} type error.");
                }
            }
            else if(list.Count == 3)//format like: 1+2, s+k
            {
                var firstValueStr = Visit(list[0]);
                var secondValue = Visit(list[2]);

                var isFirstInt = int.TryParse(firstValueStr.ToString(), out var firstValue);
                var isSecondInt = int.TryParse(secondValue.ToString(), out var secondVaule);
                var secondChild = (ITerminalNode)list[1];
                if (isFirstInt && isSecondInt)
                {
                    switch (secondChild.Symbol.Type)
                    {
                        case ExpressionParser.PLUS: return firstValue + secondVaule;
                        case ExpressionParser.MINUS: return firstValue - secondVaule;
                        default: throw new Exception($"{secondChild.Symbol.Type} type error.");
                    }
                }
                else
                {
                    switch (secondChild.Symbol.Type)
                    {
                        case ExpressionParser.ASTERISK: return list[0].GetText() + list[2].GetText();//string concat
                        default: throw new Exception($"{secondChild.Symbol.Type} type error.");
                    }
                }
            }

            throw new Exception($"list params count is not right.");
        }

        public override object VisitFunctionExp([NotNull] ExpressionParser.FunctionExpContext context)
        {
            var children = context.children;
            var functionName = children[0].GetText();
            var parameters = new List<object>(); 
            foreach(var child in children)
            {
                if (child.ChildCount == 0)
                    continue;

                parameters.Add(Visit(child));
            }

            var method = GetMethod(functionName);
            return method(parameters.ToArray());
        }

        public override object VisitMulDivExp([NotNull] ExpressionParser.MulDivExpContext context)
        {
            var list = context.children;
            if (list.Count != 3)//format like: 1*2, 5/2
            {
                throw new Exception($"list params count is not right.");
            }

            var firstValueStr = Visit(list[0]);
            var secondValue = Visit(list[2]);
            var isFirstInt = int.TryParse(firstValueStr.ToString(), out var firstValue);
            var isSecondInt = int.TryParse(secondValue.ToString(), out var secondVaule);
            var secondChild = (ITerminalNode)list[1];
            if (isFirstInt && isSecondInt)
            {
                switch (secondChild.Symbol.Type)
                {
                    case ExpressionParser.ASTERISK: return firstValue * secondVaule;
                    case ExpressionParser.SLASH:
                        {
                            if(secondVaule == 0)
                                throw new Exception("0 cannot be divisible");

                            return firstValue / secondVaule;
                        }
                    default: throw new Exception($"{secondChild.Symbol.Type} type error.");
                }
            }

            throw new Exception("* and / should be linked by number");
        }

        public override object VisitNumericAtomExp([NotNull] ExpressionParser.NumericAtomExpContext context)
        {
            var numberStr =  context.GetText();
            var parseSuccess = int.TryParse(numberStr, out var num);
            if(!parseSuccess)
                throw new Exception($"{numberStr} is not a number.");
            return num;

        }

        public override object VisitParenthesisExp([NotNull] ExpressionParser.ParenthesisExpContext context)
        {
            return Visit(context.expression());
        }

        public override object VisitStringExp([NotNull] ExpressionParser.StringExpContext context)
        {
            var originStr = context.GetText();
            return originStr.Trim('\'');
        }

        public override object VisitIdExp([NotNull] ExpressionParser.IdExpContext context)
        {
            return GetValue(Scope, context.GetText());
        }

        public override object VisitBinaryLogicExp([NotNull] ExpressionParser.BinaryLogicExpContext context)
        {
            var list = context.children;
            if (list.Count != 3)//format like: 1 == 2, 5 > 2
            {
                throw new Exception($"list params count is not right.");
            }

            var firstValueStr = Visit(list[0]);
            var secondValue = Visit(list[2]);

            var secondChild = (ITerminalNode)list[1];

            switch (secondChild.Symbol.Type)
            {
                case ExpressionParser.EQUALS: return firstValueStr.Equals(secondValue);
                case ExpressionParser.NOTEQUALS: return !firstValueStr.Equals(secondValue);
                default: throw new Exception($"{secondChild.Symbol.Type} type error.");
            }

            throw new Exception("Binary Logic should be linked by two items");
        }

        public override object VisitBracketExp([NotNull] ExpressionParser.BracketExpContext context)
        {
            var instance = GetValue(Scope, context.IDENTIFIER().GetText());
            var index = Visit(context.expression());
            return GetValue(instance, index);
        }

        public override object VisitDotExp([NotNull] ExpressionParser.DotExpContext context)
        {
            //two situations
            //1. a.b  ----->IDENTIFIER.IDENTIFIER
            //2. a.b()  ------>IDENTIFIER.functionExp   //lambda function

            var instance = GetValue(Scope, context.IDENTIFIER().GetText());
            var expression = context.expression();
            if (expression.ChildCount == 1)// Situation 1
            {
                return GetValue(instance, expression.GetText());
            }
            else
            {
                var children = expression.children;
                // Situation 2
                var methodName = children[0].GetText();

                var parameters = new List<object>();
                foreach (var child in children)
                {
                    if (child.ChildCount == 0)
                        continue;

                    parameters.Add(Visit(child));
                }

                parameters.Insert(0, instance);

                var method = GetMethod(methodName);
                return method(parameters.ToArray());
            }
        }
    }
}
