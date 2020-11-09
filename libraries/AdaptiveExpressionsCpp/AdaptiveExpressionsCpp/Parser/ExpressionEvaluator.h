#pragma once

#include "../Code/pch.h"

#include "Expression.h"
#include "ReturnType.h"

class Expression;

class ExpressionEvaluator
{
public:
    ExpressionEvaluator(
        std::string type,
        EvaluateExpressionLambda evaluator,
        ReturnType returnType = ReturnType::Object,
        EvaluateExpressionValidatorFunction validator = nullptr);

    ValueErrorTuple TryEvaluate(Expression* expression, void* state, void* options);
    void ValidateExpression(Expression* expression);

    void setReturnType(ReturnType returnType);
    ReturnType getReturnType();
    
    std::string m_type;
    
    EvaluateExpressionLambda m_evaluator;
    EvaluateExpressionValidatorFunction m_validator;

protected:
    ReturnType m_returnType{};
};