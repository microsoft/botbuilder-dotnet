#pragma once

#include "../Code/pch.h"

#include <string>
#include "ExpressionParser.h"
#include "FunctionTable.h"

class Expression
{

public:
    Expression();
    Expression(std::string type, size_t childrenCount, std::vector<Expression*> children);


    Expression(ExpressionEvaluator* evaluator);
    Expression(ExpressionEvaluator* evaluator, size_t childrenCount, std::vector<Expression*> children);
    
    static Expression* ConstantExpression(antlrcpp::Any value);
    static Expression* Parse(std::string expression, EvaluatorLookup lookup = nullptr);
    static Expression* MakeExpression(ExpressionEvaluator* evaluator, size_t childrenCount, std::vector<Expression*> children);
    static ExpressionEvaluator* Lookup(std::string functionName);

    void Validate();
    ExpressionEvaluator* getEvaluator();

    ValueErrorTuple TryEvaluate(void* state, void* options = nullptr);

    static const FunctionTable* Functions;

    size_t getChildrenCount();
    Expression* getChildAt(size_t pos);

    /*
    antlrcpp::Any getValue();
    */

    // Expression* getChildren();

private:
    // antlrcpp::Any m_expressionValue{};
    ExpressionEvaluator* m_evaluator{};
    EvaluatorLookup m_evaluatorLookup{};
    std::vector<Expression*> m_children{};
    size_t m_childrenCount{};
};


