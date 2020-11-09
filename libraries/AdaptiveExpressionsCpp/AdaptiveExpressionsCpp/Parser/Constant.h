// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

#include "../Code/pch.h"

class Constant : public Expression
{

public:
    Constant(std::any value);
   // ExpressionEvaluator* getEvaluator() override { return Expression::getEvaluator(); }

    static EvaluateExpressionLambda Evaluator();

    std::any getValue();
    void setValue(std::any value);

private:
    std::any m_value;
};