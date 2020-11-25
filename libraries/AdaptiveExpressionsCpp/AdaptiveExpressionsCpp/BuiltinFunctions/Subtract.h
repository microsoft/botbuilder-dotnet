#pragma once
#include "../Code/ExpressionEvaluatorWithArgs.h"

namespace AdaptiveExpressions_BuiltinFunctions
{
    class Subtract : public ExpressionEvaluatorWithArgs
    {
    public:
        Subtract();
        static std::any EvalSubtract(std::any a, std::any b);
        void ValidateExpression(Expression* expression);

    private:
        virtual ValueErrorTuple EvaluateOperator(std::vector<std::any> args) override;

    };
}