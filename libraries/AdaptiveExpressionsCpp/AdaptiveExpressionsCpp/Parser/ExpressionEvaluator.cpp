// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "ExpressionEvaluator.h"

ExpressionEvaluator::ExpressionEvaluator(std::string type, EvaluateExpressionLambda evaluator, ReturnType returnType, EvaluateExpressionValidatorFunction validator)
{
    m_type = type;
    m_evaluator = evaluator;
    m_returnType = returnType;
    m_validator = validator;
}

ValueErrorTuple ExpressionEvaluator::TryEvaluate(Expression* expression, void* state, void* options)
{ 
    ValueErrorTuple valueAndError = m_evaluator(expression, state, options);
    return valueAndError;
}

void ExpressionEvaluator::ValidateExpression(Expression* expression)
{
    m_validator(expression);
}

void ExpressionEvaluator::setReturnType(ReturnType returnType)
{
    m_returnType = returnType;
}

ReturnType ExpressionEvaluator::getReturnType()
{
    return m_returnType;
}
