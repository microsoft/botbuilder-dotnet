#include "Or.h"
#include "../Code/ExpressionType.h"
#include "../Code/FunctionUtils.h"

#include <limits>
#include <any>
#include <cstdlib>

AdaptiveExpressions_BuiltinFunctions::Or::Or() :
    ExpressionEvaluator(ExpressionType::Or, (ReturnType)((int)ReturnType::String | (int)ReturnType::Number))
{
}

ValueErrorTuple AdaptiveExpressions_BuiltinFunctions::Or::TryEvaluate(Expression* expression, void* state, Options* options)
{
    std::any result = false;
    std::string error;

    for (int i = 0; i < expression->getChildrenCount(); ++i)
    {
        Expression* child = expression->getChildAt(i);

        // Create a childEvaluateOptions using the locale from the passed in parameter but with NullSubstitution unset
        Options childEvaluateOptions;
        childEvaluateOptions.locale = options ? options->locale : "";

        ValueErrorTuple childResult = child->TryEvaluate(state, &childEvaluateOptions);
        if (childResult.second.empty())
        {
            if (FunctionUtils::isLogicTrue(childResult.first))
            {
                result = true;
                break;
            }
        }
        else
        {
            // Interpret error as false and swallow errors
            result = false;
            error = nullptr;
        }
    }

    return ValueErrorTuple(result, error);
}

void AdaptiveExpressions_BuiltinFunctions::Or::ValidateExpression(Expression* expression)
{
    FunctionUtils::ValidateArityAndAnyType(expression, 1, INT_MAX);
}