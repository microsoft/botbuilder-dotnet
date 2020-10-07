#pragma once

#include "../Code/pch.h"

#include <string>
#include "ExpressionParser.h"

class Expression : antlrcpp::Any
{

public:
    Expression();
    Expression(std::string type, Expression* children);
    Expression(ExpressionEvaluator* evaluator, Expression* children);
    
    static Expression* ConstantExpression(antlrcpp::Any value);
    static Expression* Parse(std::string expression, EvaluatorLookup lookup = nullptr);
    static Expression* MakeExpression(ExpressionEvaluator* evaluator, Expression* children);
    static ExpressionEvaluator* Lookup(std::string functionName);

    void Validate();
    ExpressionEvaluator* getEvaluator();

private:
    ExpressionEvaluator* m_evaluator;
    EvaluatorLookup m_evaluatorLookup;
};