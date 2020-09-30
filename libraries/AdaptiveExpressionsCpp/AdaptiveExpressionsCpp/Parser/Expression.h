#pragma once

#include "../Code/pch.h"

#include <string>
#include "ExpressionEvaluator.h"

class ExpressionEvaluator;

class Expression : antlrcpp::Any
{

public:
    Expression();
    Expression(std::string type, Expression* children);
    Expression(ExpressionEvaluator* evaluator, Expression* children);

    // static Expression Parse(std::string expression, EvaluatorLookup lookup = nullptr);
    static Expression* MakeExpression(ExpressionEvaluator* evaluator, Expression* children);

    void Validate();
    ExpressionEvaluator* getEvaluator();

private:
    ExpressionEvaluator* m_evaluator;
};