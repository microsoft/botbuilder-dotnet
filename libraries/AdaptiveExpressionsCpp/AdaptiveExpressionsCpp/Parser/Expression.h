#pragma once

#include "../Code/pch.h"

#include <string>
#include "ExpressionParser.h"
#include "FunctionTable.h"

class Expression
{

public:
    Expression();
    Expression(antlrcpp::Any value);
    Expression(std::string type, Expression* children);
    Expression(ExpressionEvaluator* evaluator, Expression* children);
    
    static Expression* ConstantExpression(antlrcpp::Any value);
    static Expression* Parse(std::string expression, EvaluatorLookup lookup = nullptr);
    static Expression* MakeExpression(ExpressionEvaluator* evaluator, Expression* children);
    static ExpressionEvaluator* Lookup(std::string functionName);

    void Validate();
    ExpressionEvaluator* getEvaluator();

    ValueErrorTuple TryEvaluate(void* state, void* options = nullptr);

    static const FunctionTable* Functions;

    Expression* getChildren();

private:
    antlrcpp::Any m_expressionValue;
    ExpressionEvaluator* m_evaluator;
    EvaluatorLookup m_evaluatorLookup;
    Expression* m_children = nullptr;
};


