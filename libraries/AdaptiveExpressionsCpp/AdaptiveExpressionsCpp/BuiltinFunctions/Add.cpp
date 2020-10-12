#include "Add.h"
#include "../Parser/ExpressionType.h"
#include "../Parser/FunctionUtils.h"

#include <limits>

AdaptiveExpressions_BuiltinFunctions::Add::Add() : 
    ExpressionEvaluator(ExpressionType::Add, Evaluator(), (ReturnType)((int)ReturnType::String | (int)ReturnType::Number), Add::Validator)
{
}

EvaluateExpressionFunction AdaptiveExpressions_BuiltinFunctions::Add::Evaluator()
{
    return nullptr;
}

antlrcpp::Any AdaptiveExpressions_BuiltinFunctions::Add::EvalAdd(antlrcpp::Any a, antlrcpp::Any b)
{
    if (a.isNull())
    {
        // throw new ArgumentNullException(nameof(a));
    }

    if (b.isNull())
    {
        // throw new ArgumentNullException(nameof(b));
    }

    if (FunctionUtils::isInteger(a) && FunctionUtils::isInteger(b))
    {
        long long int valueA = a.as<long long int>();
        long long int valueB = b.as<long long int>();
        return antlrcpp::Any(valueA + valueB);
    }

    double valueA = a.as<double>();
    double valueB = b.as<double>();
    return antlrcpp::Any(valueA + valueB);
}

void AdaptiveExpressions_BuiltinFunctions::Add::Validator(Expression* expression)
{
    FunctionUtils::ValidateArityAndAnyType(expression, 2, INT_MAX, (ReturnType)((int)ReturnType::String | (int)ReturnType::Number));
}

ValueErrorTuple AdaptiveExpressions_BuiltinFunctions::Add::Sequence(std::vector<void*> args)
{
    void* result = nullptr;
    std::string error = nullptr;
    auto firstItem = args[0];
    auto secondItem = args[1];
    bool stringConcat = !firstItem.IsNumber() || !secondItem.IsNumber();

    if ((firstItem == null && secondItem.IsNumber())
        || (secondItem == null && firstItem.IsNumber()))
    {
        error = "Operator '+' or add cannot be applied to operands of type 'number' and null object.";
    }
    else
    {
        if (stringConcat)
        {
            result = $"{firstItem?.ToString()}{secondItem?.ToString()}";
        }
        else
        {
            result = EvalAdd(args[0], args[1]);
        }
    }

    return (result, error);

    return ValueErrorTuple();
}
