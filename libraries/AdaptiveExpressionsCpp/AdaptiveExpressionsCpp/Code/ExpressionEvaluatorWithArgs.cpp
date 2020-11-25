#pragma once
#include "../Code/ExpressionEvaluatorWithArgs.h"
#include "../Code/FunctionUtils.h"

namespace AdaptiveExpressions_BuiltinFunctions
{
    ValueErrorTuple ExpressionEvaluatorWithArgs::TryEvaluate(Expression* expression, void* state, Options* options)
    {
        return ApplyWithError(expression, state, options);
    }


    ValueErrorTuple ExpressionEvaluatorWithArgs::ApplyWithError(Expression* expression, void* state, Options* options)
    {
        std::any value;
        std::string error;
        void* verify = nullptr;
        ValueErrorTuple argsAndError = FunctionUtils::EvaluateChildren(expression, state, options, verify);
        if (argsAndError.second.empty())
        {
            std::vector<std::any> args = std::any_cast<std::vector<std::any>>(argsAndError.first);

            try
            {
                ValueErrorTuple valueAndError = ApplySequenceWithError(args, nullptr);
                value = valueAndError.first;
            }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return it in the error)
            catch (std::exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = std::string(e.what());
            }
        }

        value = FunctionUtils::ResolveValue(value);

        return ValueErrorTuple(value, error);
    }

    ValueErrorTuple ExpressionEvaluatorWithArgs::ApplySequenceWithError(std::vector<std::any> args, void* verify)
    {
        std::vector<std::any> binaryArgs(2);
        std::any sofar = args[0];
        for (auto i = 1; i < args.size(); ++i)
        {
            binaryArgs[0] = sofar;
            binaryArgs[1] = args[i];

            ValueErrorTuple resultAndError = EvaluateOperator(binaryArgs);
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
}