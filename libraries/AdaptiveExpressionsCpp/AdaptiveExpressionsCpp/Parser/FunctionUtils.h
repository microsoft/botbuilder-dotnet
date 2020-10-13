#pragma once

#include "../Code/pch.h"

static class FunctionUtils
{
public:
    // public static EvaluateExpressionDelegate ApplyWithError(Func<IReadOnlyList<object>, (object, string)> function, VerifyExpression verify = null)
    // public static EvaluateExpressionDelegate ApplySequenceWithError(Func<IReadOnlyList<object>, (object, string)> function, VerifyExpression verify = null)

    // public delegate (object value, string error) EvaluateExpressionDelegate(Expression expression, IMemory state, Options options);

    static EvaluateExpressionLambda ApplySequenceWithError(std::function<ValueErrorTuple (std::vector<std::any>)> f, void* verify = nullptr);
    static EvaluateExpressionLambda ApplyWithError(std::function<ValueErrorTuple(std::vector<std::any>)> f, void* verify = nullptr);

    static void ValidateArityAndAnyType(Expression* expression, int minArity, int maxArity, ReturnType returnType = ReturnType::Object);

    static bool isInteger(antlrcpp::Any value);
    static bool isNumber(antlrcpp::Any value);

    static ValueErrorTuple EvaluateChildren(Expression* expression, void* state, void* options, void* verify = nullptr);

    static std::any ResolveValue(std::any value);

    

private:
    // static ValueErrorTuple ApplySequenceWithErrorInternal(ValueErrorTuple(*function)(std::vector<std::any>), Expression* expression, void* state, void* options, void* verify = nullptr);
    // static ValueErrorTuple ApplyWithErrorInternal(ValueErrorTuple(*function)(std::vector<std::any>), Expression* expression, void* state, void* options, void* verify);
    // static ValueErrorTuple ApplyWithErrorInternal(ValueErrorTuple(*function)(std::vector<std::any>), void* verify = nullptr);
};