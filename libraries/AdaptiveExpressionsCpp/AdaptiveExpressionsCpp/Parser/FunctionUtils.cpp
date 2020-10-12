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

void FunctionUtils::ApplyWithErrorInternal()
{
    /*
    void* value = nullptr;
    std::string error();
    std::vector<void*> args;
    ValueErrorTuple argsAndError = EvaluateChildren(expression, state, options, verify);
    if (error == null)
    {
        try
        {
            (value, error) = function(args);
        }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return it in the error)
        catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            error = e.Message;
        }
    }

    value = ResolveValue(value);

    return (value, error);
    */

    return;
}

inline EvaluateExpressionFunction FunctionUtils::ApplySequenceWithError(ValueErrorTuple (*function)(std::vector<void*>), void* verify)
{
    // return ApplyWithError(anotherFunction, verify);
    return nullptr;
}

EvaluateExpressionFunction FunctionUtils::ApplyWithError(ValueErrorTuple (*function)(std::vector<void*>), void* verify)
{
    return EvaluateExpressionFunction();
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
