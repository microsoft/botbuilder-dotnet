#include "FunctionTable.h"
#include "../BuiltinFunctions/Add.h"
#include "../BuiltinFunctions/Subtract.h"
#include "../BuiltinFunctions/And.h"
#include "../BuiltinFunctions/Or.h"
#include "../BuiltinFunctions/Not.h"

FunctionTable::FunctionTable() : std::map<std::string, ExpressionEvaluator*>()
{
    PopulateStandardFunctions();
}

void FunctionTable::PopulateStandardFunctions()
{
    // Math aliases
    this->insert(std::pair<std::string, ExpressionEvaluator*>("add", new AdaptiveExpressions_BuiltinFunctions::Add()));
    this->insert(std::pair<std::string, ExpressionEvaluator*>("+", new AdaptiveExpressions_BuiltinFunctions::Add()));
    this->insert(std::pair<std::string, ExpressionEvaluator*>("subtract", new AdaptiveExpressions_BuiltinFunctions::Subtract()));
    this->insert(std::pair<std::string, ExpressionEvaluator*>("-", new AdaptiveExpressions_BuiltinFunctions::Subtract()));

    // Logic
    this->insert(std::pair<std::string, ExpressionEvaluator*>("and", new AdaptiveExpressions_BuiltinFunctions::And()));
    this->insert(std::pair<std::string, ExpressionEvaluator*>("&&", new AdaptiveExpressions_BuiltinFunctions::And()));
    this->insert(std::pair<std::string, ExpressionEvaluator*>("or", new AdaptiveExpressions_BuiltinFunctions::Or()));
    this->insert(std::pair<std::string, ExpressionEvaluator*>("||", new AdaptiveExpressions_BuiltinFunctions::Or()));
    this->insert(std::pair<std::string, ExpressionEvaluator*>("not", new AdaptiveExpressions_BuiltinFunctions::Not()));
    this->insert(std::pair<std::string, ExpressionEvaluator*>("!", new AdaptiveExpressions_BuiltinFunctions::Not()));
}
