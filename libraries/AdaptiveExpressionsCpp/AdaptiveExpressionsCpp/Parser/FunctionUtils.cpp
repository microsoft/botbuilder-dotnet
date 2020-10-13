#include "FunctionUtils.h"

bool FunctionUtils::isInteger(antlrcpp::Any value)
{
    return value.is<short>() || value.is<unsigned short>() || value.is<int>() || value.is<unsigned int>() || value.is<long>() || value.is<unsigned long>();
}

bool FunctionUtils::isNumber(antlrcpp::Any value)
{
    return isInteger(value) || value.is<float>() || value.is<double>();
}

ValueErrorTuple FunctionUtils::EvaluateChildren(Expression* expression, void* state, void* options, void* verify)
{
    std::vector<void*>* args = new std::vector<void*>();
    void* value;
    std::string error;
    auto pos{ 0 };

    for (int i = 0; i < sizeof(expression->getChildren()); ++i)
    {
        Expression child = expression->getChildren()[i];

        ValueErrorTuple valueAndError = child.TryEvaluate(state, options);
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

        args->push_back(value);
        ++pos;
    }

    return ValueErrorTuple(args, error);
}

std::any FunctionUtils::ResolveValue(std::any value)
{
    // This should perform some conversion from JValue to regular values, if we use json we may skip this step or do other type of conversions
    return value;
}

EvaluateExpressionLambda FunctionUtils::ApplyWithError(std::function<ValueErrorTuple(std::vector<std::any>)> f, void* verify)
{
    EvaluateExpressionLambda otherFunction = [&](Expression* expression, void* state, void* options)
    { 
        std::any value;
        std::string error;
        std::vector<std::any> args;
        ValueErrorTuple argsAndError = EvaluateChildren(expression, state, options, verify);
        if (!argsAndError.second.empty())
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

    return otherFunction;
}

EvaluateExpressionLambda FunctionUtils::ApplySequenceWithError(std::function<ValueErrorTuple(std::vector<std::any>)> f, void* verify)
{
    std::function<ValueErrorTuple(std::vector<std::any>)> otherFunction = [&](std::vector<std::any> args) {
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
    };


    return ApplyWithError(otherFunction, verify);
}

void FunctionUtils::ValidateArityAndAnyType(Expression* expression, int minArity, int maxArity, ReturnType returnType)
{
    /*
    if (expression->Children.Length < minArity)
    {
        throw new ArgumentException($"{expression} should have at least {minArity} children.");
    }

    if (expression.Children.Length > maxArity)
    {
        throw new ArgumentException($"{expression} can't have more than {maxArity} children.");
    }

    if ((returnType & ReturnType.Object) == 0)
    {
        foreach(var child in expression.Children)
        {
            if ((child.ReturnType & ReturnType.Object) == 0 && (returnType & child.ReturnType) == 0)
            {
                throw new ArgumentException(BuildTypeValidatorError(returnType, child, expression));
            }
        }
    }
    */
}
