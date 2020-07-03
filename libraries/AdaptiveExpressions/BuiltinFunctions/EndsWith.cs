// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class EndsWith : ExpressionEvaluator
    {
        public EndsWith()
            : base(ExpressionType.EndsWith, Evaluator(), ReturnType.Boolean, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            string rawStr = FunctionUtils.ParseStringOrNull(args[0]);
                            string seekStr = FunctionUtils.ParseStringOrNull(args[1]);
                            return rawStr.EndsWith(seekStr);
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 2, ReturnType.String);
        }
    }
}
