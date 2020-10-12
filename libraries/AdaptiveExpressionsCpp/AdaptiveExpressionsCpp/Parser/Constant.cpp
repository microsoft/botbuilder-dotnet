// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "Constant.h"
#include "FunctionUtils.h"

Constant::Constant(antlrcpp::Any value) : Expression()
{
}

antlrcpp::Any Constant::getValue()
{
    return m_value;
}

void Constant::setValue(antlrcpp::Any value)
{
    // Have to figure out how to check if it's an array
    getEvaluator()->m_returnType =
        value.is<std::string>() ? ReturnType::String :
        FunctionUtils::isNumber(value) ? ReturnType::Number :
        value.is<bool>() ? ReturnType::String :
        ReturnType::Object;

    m_value = value;
}