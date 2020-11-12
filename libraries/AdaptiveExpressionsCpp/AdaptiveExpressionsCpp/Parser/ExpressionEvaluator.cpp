// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "ExpressionEvaluator.h"
#include "../Parser/FunctionUtils.h"
#include "../BuiltinFunctions/Subtract.h"

ExpressionEvaluator::ExpressionEvaluator(std::string type, ReturnType returnType)
{
    m_type = type;
    m_returnType = returnType;
}

void ExpressionEvaluator::setReturnType(ReturnType returnType)
{
    m_returnType = returnType;
}

ReturnType ExpressionEvaluator::getReturnType()
{
    return m_returnType;
}

