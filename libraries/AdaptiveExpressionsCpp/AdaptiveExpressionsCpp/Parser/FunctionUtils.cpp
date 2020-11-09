#include "FunctionUtils.h"

#include <any>
#include <utility>
#include "../BuiltinFunctions/Add.h"

bool FunctionUtils::isInteger(antlrcpp::Any value)
{
    bool isShort = value.is<short>() || value.is<unsigned short>();
    bool isInt = value.is<int>() || value.is<unsigned int>();
    bool isLong = value.is<long>() || value.is<unsigned long>() || value.is<long long>() || value.is<unsigned long long>();
    return isShort || isInt || isLong;
}

bool FunctionUtils::isNumber(antlrcpp::Any value)
{
    return isInteger(value) || value.is<float>() || value.is<double>();
}

bool FunctionUtils::isShort(std::any value)
{
    return isOfType<short>(value) || isOfType<unsigned short>(value);
}

bool FunctionUtils::isInt32(std::any value)
{
    return isOfType<int>(value) || isOfType<unsigned int>(value);
}

bool FunctionUtils::isInt64(std::any value)
{
    return isOfType<long long int>(value) || isOfType<unsigned long long int>(value);
}

bool FunctionUtils::isFloat(std::any value)
{
    return isOfType<float>(value);
}

bool FunctionUtils::isDouble(std::any value)
{
    return isOfType<double>(value) || isOfType<long double>(value);
}

bool FunctionUtils::isInteger(std::any value)
{
    return isShort(value) || isInt32(value) || isInt64(value);
}

bool FunctionUtils::isNumber(std::any value)
{
    return isInteger(value) || isFloat(value) || isDouble(value);
}

ValueErrorTuple FunctionUtils::EvaluateChildren(Expression* expression, void* state, void* options, void* verify)
{
    std::vector<std::any> args;
    std::any value;
    std::string error;
    auto pos{ 0 };

    for (int i = 0; i < expression->getChildrenCount(); ++i)
    {
        Expression* child = expression->getChildAt(i);

        ValueErrorTuple valueAndError = child->TryEvaluate(state, options);
        error = valueAndError.second;
        if (!error.empty())
        {
            break;
        }

        if (verify != nullptr)
        {
            // error = verify(value, child, pos);
        }

        if (!error.empty())
        {
            break;
        }

        args.push_back(valueAndError.first);
        ++pos;
    }

    return ValueErrorTuple(args, error);
}

std::any FunctionUtils::ResolveValue(std::any value)
{
    // This should perform some conversion from JValue to regular values, if we use json we may skip this step or do other type of conversions
    return value;
}

std::string FunctionUtils::VerifyNumberOrStringOrNull(std::any value, Expression* expression, int number)
{
    std::string error;

    if (value.has_value() && !isNumber(value) && !(isOfType<std::string>(value)))
    {
        error = "{expression} is not string or number.";
    }

    return error;
}

ValueErrorTuple FunctionUtils::ReverseApplySequenceWithError(std::vector<std::any> args, void* verify)
{
    std::vector<std::any> binaryArgs(2);
    std::any sofar = args[0];
    for (auto i = 1; i < args.size(); ++i)
    {
        binaryArgs[0] = sofar;
        binaryArgs[1] = args[i];

        ValueErrorTuple resultAndError = AdaptiveExpressions_BuiltinFunctions::Add::ReverseEvaluatorInternal(binaryArgs);
        if (!resultAndError.second.empty())
        {
            return resultAndError;
        }
        else
        {
            sofar = resultAndError.first;
        }
    }

    return ValueErrorTuple(sofar, std::string());
}

ValueErrorTuple FunctionUtils::ReverseApplyWithError(Expression* expression, void* state, void* options)
{
    std::any value;
    std::string error;
    void* verify = nullptr;
    ValueErrorTuple argsAndError = EvaluateChildren(expression, state, options, verify);
    if (argsAndError.second.empty())
    {
        std::vector<std::any> args = std::any_cast<std::vector<std::any>>(argsAndError.first);

        try
        {
            ValueErrorTuple valueAndError = ReverseApplySequenceWithError(args, nullptr);
            value = valueAndError.first;
        }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return it in the error)
        catch (std::exception e)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            error = std::string(e.what());
        }
    }

    value = ResolveValue(value);

    return ValueErrorTuple(value, error);
}

EvaluateExpressionLambda FunctionUtils::ApplyWithError(std::function<ValueErrorTuple(std::vector<std::any>)> f, void* verify)
{
    return [&](Expression* expression, void* state, void* options)
    {
        std::any value;
        std::string error;
        std::vector<std::any> args;
        ValueErrorTuple argsAndError = EvaluateChildren(expression, state, options, verify);
        if (argsAndError.second.empty())
        {
            try
            {
                ValueErrorTuple valueAndError = f(args);
            }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return it in the error)
            catch (std::exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = std::string(e.what());
            }
        }

        value = ResolveValue(value);

        return ValueErrorTuple(value, error);
    };
}

EvaluateExpressionLambda FunctionUtils::ApplySequenceWithError(std::function<ValueErrorTuple(std::vector<std::any>)> f, void* verify)
{
    return ApplyWithError(
        [&](std::vector<std::any> args) {
        std::vector<std::any> binaryArgs(2);
        std::any sofar = args[0];
        for (auto i = 1; i < args.size(); ++i)
        {
            binaryArgs[0] = sofar;
            binaryArgs[1] = args[i];

            ValueErrorTuple resultAndError = f(binaryArgs);
            if (!resultAndError.second.empty())
            {
                return resultAndError;
            }
            else
            {
                sofar = resultAndError.first;
            }
        }

        return ValueErrorTuple(sofar, std::string());
        }, verify);
}

void FunctionUtils::ValidateArityAndAnyType(Expression* expression, int minArity, int maxArity, ReturnType returnType)
{
    if (expression->getChildrenCount() < minArity)
    {
        // throw new ArgumentException($"{expression} should have at least {minArity} children.");
    }

    if (expression->getChildrenCount() > maxArity)
    {
        // throw new ArgumentException($"{expression} can't have more than {maxArity} children.");
    }

    if ((returnType & ReturnType::Object) == 0)
    {
        for (int i = 0; i < expression->getChildrenCount(); ++i)
        {
            Expression* child = expression->getChildAt(i);
            if ((child->getReturnType() & ReturnType::Object) == 0 && (returnType & child->getReturnType()) == 0)
            {
                // throw new ArgumentException(BuildTypeValidatorError(returnType, child, expression));
            }
        }
    }
}
