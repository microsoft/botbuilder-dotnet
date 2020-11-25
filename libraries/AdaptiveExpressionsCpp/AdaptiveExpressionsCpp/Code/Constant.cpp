// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "Constant.h"
#include "FunctionUtils.h"
#include "ExpressionType.h"

Constant::Constant(std::any value) :
    Expression(new ConstantExpressionEvaluator())
{
    setValue(value);
}

std::any Constant::getValue()
{
    return m_value;
}

EvaluateExpressionLambda Constant::Evaluator()
{
    return [](Expression* expression, void* state, void* options)
    {
        Constant* constant = (Constant*)expression;
        return ValueErrorTuple(constant->getValue(), std::string());
    };;
}

void Constant::setValue(std::any value)
{
    // Have to figure out how to check if it's an array
    m_evaluator->setReturnType(FunctionUtils::isOfType<std::string>(value) ? ReturnType::String :
        FunctionUtils::isNumber(value) ? ReturnType::Number :
        FunctionUtils::isOfType<bool>(value) ? ReturnType::String :
        // FunctionUtils::isOfType<std::vector<template T>> ? ReturnType::Array :
        ReturnType::Object);

    m_value = value;
}

ConstantExpressionEvaluator::ConstantExpressionEvaluator() :
    ExpressionEvaluator(ExpressionType::Constant, ReturnType::Object)
{
}

ValueErrorTuple ConstantExpressionEvaluator::TryEvaluate(Expression* expression, void* state, Options* options)
{
    Constant* constant = (Constant*)expression;
    return ValueErrorTuple(constant->getValue(), std::string());
}

void ConstantExpressionEvaluator::ValidateExpression(Expression* expression)
{
}
