// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

#include "../Code/pch.h"

class Constant : public Expression
{

public:
    Constant(antlrcpp::Any value);
   // ExpressionEvaluator* getEvaluator() override { return Expression::getEvaluator(); }

    static EvaluateExpressionLambda Evaluator();

    antlrcpp::Any getValue();
    void setValue(antlrcpp::Any value);

private:
    antlrcpp::Any m_value;
};