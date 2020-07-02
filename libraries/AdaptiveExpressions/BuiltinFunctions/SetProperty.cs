using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class SetProperty : ExpressionEvaluator
    {
        public SetProperty()
            : base(ExpressionType.SetProperty, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args =>
            {
                var newJobj = (IDictionary<string, JToken>)args[0];
                newJobj[args[1].ToString()] = FunctionUtils.ConvertToJToken(args[2]);
                return newJobj;
            });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expr, null, ReturnType.Object, ReturnType.String, ReturnType.Object);
        }
    }
}
