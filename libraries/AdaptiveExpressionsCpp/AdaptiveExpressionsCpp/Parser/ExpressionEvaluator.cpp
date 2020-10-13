// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "ExpressionEvaluator.h"

ExpressionEvaluator::ExpressionEvaluator(std::string type, EvaluateExpressionFunction evaluator, ReturnType returnType, EvaluateExpressionValidatorFunction validator)
{
    m_type = type;
    m_evaluator = evaluator;
    m_returnType = returnType;
    m_validator = validator;
}

ValueErrorTuple ExpressionEvaluator::TryEvaluate(Expression* expression, void* state, void* options)
{
    m_evaluator(expression, state, options);
    return std::make_pair(nullptr, std::string());
}

void ExpressionEvaluator::ValidateExpression(Expression* expression)
{
    m_validator(expression);
}