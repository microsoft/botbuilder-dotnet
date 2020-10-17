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

    static ValueErrorTuple ReverseApplySequenceWithError(std::vector<std::any> args, void* verify);
    static ValueErrorTuple ReverseApplyWithError(Expression* expression, void* state, void* options);

    static void ValidateArityAndAnyType(Expression* expression, int minArity, int maxArity, ReturnType returnType = ReturnType::Object);

    static bool isInteger(antlrcpp::Any value);
    static bool isNumber(antlrcpp::Any value);


    template<class T>
    static bool isOfType(std::any value);

    template<class T>
    static T castToType(std::any value, bool& castSuccessful);

    template<class T>
    static T castToType(antlrcpp::Any value, bool& castSuccessful);

    static bool isShort(std::any value);
    static bool isInt32(std::any value);
    static bool isInt64(std::any value);
    static bool isFloat(std::any value);
    static bool isDouble(std::any value);

    static bool isInteger(std::any value);
    static bool isNumber(std::any value);

    static ValueErrorTuple EvaluateChildren(Expression* expression, void* state, void* options, void* verify = nullptr);

    static std::any ResolveValue(std::any value);

    static std::string VerifyNumberOrStringOrNull(std::any value, Expression* expression, int number);

private:
    // static ValueErrorTuple ApplySequenceWithErrorInternal(ValueErrorTuple(*function)(std::vector<std::any>), Expression* expression, void* state, void* options, void* verify = nullptr);
    // static ValueErrorTuple ApplyWithErrorInternal(ValueErrorTuple(*function)(std::vector<std::any>), Expression* expression, void* state, void* options, void* verify);
    // static ValueErrorTuple ApplyWithErrorInternal(ValueErrorTuple(*function)(std::vector<std::any>), void* verify = nullptr);
};

template<class T>
inline T FunctionUtils::castToType(std::any value, bool& castSuccessful)
{
    T returnValue{};
    try
    {
        returnValue = std::any_cast<T>(value);
        castSuccessful = true;
    }
    catch (const std::bad_any_cast&)
    {
        try
        {
            antlrcpp::Any antlrValue = std::any_cast<antlrcpp::Any>(value);
            return castToType<T>(antlrValue, castSuccessful);
        }
        catch (const std::bad_any_cast&)
        {
            castSuccessful = false;
        }
    }

    return returnValue;
}

template<class T>
inline T FunctionUtils::castToType(antlrcpp::Any value, bool& castSuccessful)
{
    T returnValue{};
    try
    {
        returnValue = value.as<T>();
        castSuccessful = true;
    }
    catch (const std::bad_any_cast&)
    {
        /*
        try
        {
            antlrcpp::Any antlrValue = std::any_cast<antlrcpp::Any>(value);
            return castToType(antlrValue, castSuccessful);
        }
        catch (const std::bad_any_cast&)
        {
            castSuccessful = false;
        }
        */
    }

    return returnValue;
}