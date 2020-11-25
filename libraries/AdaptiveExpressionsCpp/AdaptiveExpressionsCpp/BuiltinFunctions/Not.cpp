#include "Not.h"
#include "../Code/ExpressionType.h"
#include "../Code/FunctionUtils.h"

#include <limits>
#include <any>
#include <cstdlib>

AdaptiveExpressions_BuiltinFunctions::Not::Not() :
    ExpressionEvaluator(ExpressionType::Not, (ReturnType)((int)ReturnType::String | (int)ReturnType::Number))
{
}

ValueErrorTuple AdaptiveExpressions_BuiltinFunctions::Not::TryEvaluate(Expression* expression, void* state, Options* options)
{
    std::any result;
    std::string error;

    // Create a childEvaluateOptions using the locale from the passed in parameter but with NullSubstitution unset
    Options childEvaluateOptions;
    childEvaluateOptions.locale = options ? options->locale : "";

    ValueErrorTuple childResult = expression->getChildAt(0)->TryEvaluate(state, &childEvaluateOptions);
    if (childResult.second.empty())
    {
        result = !FunctionUtils::isLogicTrue(childResult.first);
    }
    else
    {
        error = nullptr;
        result = true;
    }

    return ValueErrorTuple(result, error);
}

void AdaptiveExpressions_BuiltinFunctions::Not::ValidateExpression(Expression* expression)
{
    FunctionUtils::ValidateArityAndAnyType(expression, 1, 1);
}