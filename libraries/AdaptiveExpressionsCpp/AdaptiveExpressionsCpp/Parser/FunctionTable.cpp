#include "FunctionTable.h"
#include "../BuiltinFunctions/Add.h"

FunctionTable::FunctionTable() : std::map<std::string, ExpressionEvaluator*>()
{
    PopulateStandardFunctions();
}

void FunctionTable::PopulateStandardFunctions()
{
    // Math aliases
    this->insert(std::pair<std::string, ExpressionEvaluator*>("add", new AdaptiveExpressions_BuiltinFunctions::Add())); // more than 1 params
    this->insert(std::pair<std::string, ExpressionEvaluator*>("+", new AdaptiveExpressions_BuiltinFunctions::Add())); // more than 1 params
}
