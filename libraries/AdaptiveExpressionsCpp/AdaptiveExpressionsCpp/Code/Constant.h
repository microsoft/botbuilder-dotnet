// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

#include "../Code/pch.h"

class Constant : public Expression
{

public:
    Constant(std::any value);

    static EvaluateExpressionLambda Evaluator();

    std::any getValue();
    void setValue(std::any value);

private:
    std::any m_value;
};

class ConstantExpressionEvaluator : public ExpressionEvaluator
{
public:
    ConstantExpressionEvaluator();

    virtual ValueErrorTuple TryEvaluate(Expression* expression, void* state, Options* options) override;
    virtual void ValidateExpression(Expression* expression) override;
};