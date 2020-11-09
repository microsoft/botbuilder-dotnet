#include "ExpressionFunctions.h"

#include "../BuiltinFunctions/Add.h"

const std::map<std::string, ExpressionEvaluator*> ExpressionFunctions::standardFunctions = ExpressionFunctions::getStandardFunctions();

std::map<std::string, ExpressionEvaluator*> ExpressionFunctions::getStandardFunctions()
{
    std::map<std::string, ExpressionEvaluator*> lookup;

    // Math aliases
    lookup[std::string("add")] = new AdaptiveExpressions_BuiltinFunctions::Add(); // more than 1 params
    lookup[std::string("+")] = new AdaptiveExpressions_BuiltinFunctions::Add(); // more than 1 params

    return lookup;
}
