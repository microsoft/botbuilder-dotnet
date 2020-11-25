#include "And.h"
#include "../Code/ExpressionType.h"
#include "../Code/FunctionUtils.h"

#include <limits>
#include <any>
#include <cstdlib>

AdaptiveExpressions_BuiltinFunctions::And::And() :
    ExpressionEvaluator(ExpressionType::And, (ReturnType)((int)ReturnType::String | (int)ReturnType::Number))
{
}

ValueErrorTuple AdaptiveExpressions_BuiltinFunctions::And::TryEvaluate(Expression* expression, void* state, Options* options)
{
    std::any result = true;
    std::string error;

    // Create a childEvaluateOptions using the locale from the passed in parameter but with NullSubstitution unset
    Options childEvaluateOptions;
    childEvaluateOptions.locale = options ? options->locale : "";

    for (int i = 0; i < expression->getChildrenCount(); ++i)
    {
        Expression* child = expression->getChildAt(i);

        ValueErrorTuple childResult = child->TryEvaluate(state, &childEvaluateOptions);
        if (childResult.second.empty())
        {
            if (FunctionUtils::isLogicTrue(childResult.first))
            {
                result = true;
            }
            else
            {
                result = false;
                break;
            }
        }
        else
        {
            // We interpret any error as false and swallow the error
            result = false;
            error = nullptr;
            break;
        }
    }

    return ValueErrorTuple(result, error);
}

void AdaptiveExpressions_BuiltinFunctions::And::ValidateExpression(Expression* expression)
{
    FunctionUtils::ValidateArityAndAnyType(expression, 1, INT_MAX);
}