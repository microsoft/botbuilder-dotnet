#pragma once

#include "../Code/pch.h"

#include "Expression.h"
#include "ReturnType.h"
#include "Options.h"

class Expression;

class ExpressionEvaluator
{
public:
    ExpressionEvaluator(
        std::string type,
        ReturnType returnType = ReturnType::Object);

    virtual ValueErrorTuple TryEvaluate(Expression* expression, void* state, Options* options) = 0;
    virtual void ValidateExpression(Expression* expression) = 0;

    void setReturnType(ReturnType returnType);
    ReturnType getReturnType();

    std::string m_type;



    ReturnType m_returnType{};
};