#pragma once
#include "../Parser/ExpressionEvaluator.h"

namespace AdaptiveExpressions_BuiltinFunctions
{
    class Add : public ExpressionEvaluator
    {
    public:
        Add();
        static std::any EvalAdd(std::any a, std::any b);
        static ValueErrorTuple ReverseEvaluatorInternal(std::vector<std::any> args);



    private:
        static EvaluateExpressionLambda Evaluator();
        static EvaluateExpressionLambda ReverseEvaluator();
        static void Validator(Expression* expression);
    };
}