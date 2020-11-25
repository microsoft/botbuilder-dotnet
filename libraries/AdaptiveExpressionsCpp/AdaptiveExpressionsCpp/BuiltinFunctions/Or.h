#pragma once
#include "../Code/ExpressionEvaluatorWithArgs.h"

namespace AdaptiveExpressions_BuiltinFunctions
{
    class Or : public ExpressionEvaluator
    {
    public:
        Or();
        void ValidateExpression(Expression* expression);
        virtual ValueErrorTuple TryEvaluate(Expression* expression, void* state, Options* options) override;
    };
}