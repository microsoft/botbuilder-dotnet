#pragma once
#include "../Parser/ExpressionEvaluator.h"

namespace AdaptiveExpressions_BuiltinFunctions
{
    class Add : public ExpressionEvaluator
    {
    public:
        Add();

    private:
        static EvaluateExpressionFunction Evaluator();
        static antlrcpp::Any EvalAdd(antlrcpp::Any a, antlrcpp::Any b);
        static void Validator(Expression* expression);

        static ValueErrorTuple Sequence(std::vector<void*> args);
    };
}