#pragma once

#include "../Code/pch.h"

static class FunctionUtils
{
public:
    static EvaluateExpressionFunction ApplySequenceWithError(ValueErrorTuple (*function)(std::vector<void*>), void* verify = nullptr);
    static EvaluateExpressionFunction ApplyWithError(ValueErrorTuple (*function)(std::vector<void*>), void* verify = nullptr);

    static void ValidateArityAndAnyType(Expression* expression, int minArity, int maxArity, ReturnType returnType = ReturnType::Object);

    static bool isInteger(antlrcpp::Any value);
    static bool isNumber(antlrcpp::Any value);

    static ValueErrorTuple EvaluateChildren(Expression* expression, void* state, void* options, void* verify = nullptr);

private:
    static void ApplyWithErrorInternal();
};