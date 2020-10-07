// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "Constant.h"

Constant::Constant(antlrcpp::Any value) : Expression()
{
}

antlrcpp::Any Constant::getValue()
{
    return m_value;
}

void Constant::setValue(antlrcpp::Any value)
{
    // m_evaluator->returnType = value.is<std::string>() ? 
    m_value = value;
}
