using System;
using System.Collections.Generic;
using static AdaptiveExpressions.FunctionUtils;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class ComparisonEvaluator : ExpressionEvaluator
    {
        public ComparisonEvaluator(string type, Func<IReadOnlyList<object>, bool> function, ValidateExpressionDelegate validator, VerifyExpression verify = null)
            : base(type, Evaluator(function, verify), ReturnType.Object, validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator(Func<IReadOnlyList<object>, bool> function, VerifyExpression verify)
        {
            return (expression, state, options) =>
            {
                var result = false;
                string error = null;
                IReadOnlyList<object> args;
                (args, error) = FunctionUtils.EvaluateChildren(expression, state, new Options(options) { NullSubstitution = null }, verify);
                if (error == null)
                {
                    // Ensure args are all of same type
                    bool? isNumber = null;
                    foreach (var arg in args)
                    {
                        var obj = arg;
                        if (isNumber.HasValue)
                        {
                            if (obj != null && obj.IsNumber() != isNumber.Value)
                            {
                                error = $"Arguments must either all be numbers or strings in {expression}";
                                break;
                            }
                        }
                        else
                        {
                            isNumber = obj.IsNumber();
                        }
                    }

                    if (error == null)
                    {
                        try
                        {
                            result = function(args);
                        }
                        catch (Exception e)
                        {
                            // NOTE: This should not happen in normal execution
                            error = e.Message;
                        }
                    }
                }
                else
                {
                    // Swallow errors and treat as false
                    error = null;
                }

                return (result, error);
            };
        }
    }
}
