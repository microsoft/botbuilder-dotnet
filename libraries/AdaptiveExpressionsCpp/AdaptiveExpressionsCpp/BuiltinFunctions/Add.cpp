#include "Add.h"
#include "../Parser/ExpressionType.h"
#include "../Parser/FunctionUtils.h"

#include <limits>
#include <any>
#include <cstdlib>

AdaptiveExpressions_BuiltinFunctions::Add::Add() : 
    ExpressionEvaluator(ExpressionType::Add, ReverseEvaluator(), (ReturnType)((int)ReturnType::String | (int)ReturnType::Number), Add::Validator)
{
}

EvaluateExpressionLambda AdaptiveExpressions_BuiltinFunctions::Add::Evaluator()
{
    return FunctionUtils::ApplySequenceWithError(
        [&](std::vector<std::any> args)
        {
            std::any result;
            std::string error;
            auto firstItem = args[0];
            auto secondItem = args[1];
            bool stringConcat = !FunctionUtils::isNumber(firstItem) || !FunctionUtils::isNumber(secondItem);

            if ((!firstItem.has_value() && FunctionUtils::isNumber(secondItem))
                || (!secondItem.has_value() && FunctionUtils::isNumber(firstItem)))
            {
                error = "Operator '+' or add cannot be applied to operands of type 'number' and null object.";
            }
            else
            {
                if (stringConcat)
                {
                    result = "{firstItem?.ToString()}{secondItem?.ToString()}";
                }
                else
                {
                    result = EvalAdd(args[0], args[1]);
                }
            }

            return ValueErrorTuple(result, error);
        }, FunctionUtils::VerifyNumberOrStringOrNull);
}

EvaluateExpressionLambda AdaptiveExpressions_BuiltinFunctions::Add::ReverseEvaluator()
{
    return FunctionUtils::ReverseApplyWithError;
}

ValueErrorTuple AdaptiveExpressions_BuiltinFunctions::Add::ReverseEvaluatorInternal(std::vector<std::any> args)
{
    std::any result;
    std::string error;
    auto firstItem = args[0];
    auto secondItem = args[1];

    bool stringConcat = !FunctionUtils::isNumber(firstItem) || !FunctionUtils::isNumber(secondItem);

    if ((!firstItem.has_value() && FunctionUtils::isNumber(secondItem))
        || (!secondItem.has_value() && FunctionUtils::isNumber(firstItem)))
    {
        error = "Operator '+' or add cannot be applied to operands of type 'number' and null object.";
    }
    else
    {
        if (stringConcat)
        {
            result = "{firstItem?.ToString()}{secondItem?.ToString()}";
        }
        else
        {
            result = EvalAdd(args[0], args[1]);
        }
    }

    return ValueErrorTuple(result, error);
}

std::any AdaptiveExpressions_BuiltinFunctions::Add::EvalAdd(std::any a, std::any b)
{
    if (a.has_value())
    {
        // throw new ArgumentNullException(nameof(a));
    }

    if (b.has_value())
    {
        // throw new ArgumentNullException(nameof(b));
    }

    bool valueASuccess{}, valueBSuccess{};
    if (FunctionUtils::isInteger(a) && FunctionUtils::isInteger(b))
    {
        int valueA = FunctionUtils::castToType<int>(a, valueASuccess);
        int valueB = FunctionUtils::castToType<int>(b, valueBSuccess);
        return valueA + valueB;
    }

    double valueA = FunctionUtils::castToType<double>(a, valueASuccess);
    double valueB = FunctionUtils::castToType<double>(b, valueBSuccess);
    return (valueA + valueB);
}

void AdaptiveExpressions_BuiltinFunctions::Add::Validator(Expression* expression)
{
    FunctionUtils::ValidateArityAndAnyType(expression, 2, INT_MAX, ReturnType::String | ReturnType::Number);
}
