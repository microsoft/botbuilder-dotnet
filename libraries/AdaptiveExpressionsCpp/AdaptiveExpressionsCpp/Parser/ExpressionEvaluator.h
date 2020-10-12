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
        EvaluateExpressionFunction evaluator,
        ReturnType returnType = ReturnType::Object,
        EvaluateExpressionValidatorFunction validator = nullptr);

    ValueErrorTuple TryEvaluate(Expression* expression, void* state, void* options);
    void ValidateExpression(Expression* expression);
    
    std::string m_type;
    ReturnType m_returnType{};
    EvaluateExpressionFunction m_evaluator;
    EvaluateExpressionValidatorFunction m_validator;
};