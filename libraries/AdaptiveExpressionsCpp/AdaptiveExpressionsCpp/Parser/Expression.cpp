#include "Expression.h"

Expression::Expression()
{

}

Expression::Expression(std::string type, Expression* children)
{

}

Expression::Expression(ExpressionEvaluator* evaluator, Expression* children)
{

}

/*
Expression Expression::Parse(std::string expression, EvaluatorLookup lookup = nullptr)
{

}
*/

Expression* Expression::MakeExpression(ExpressionEvaluator* evaluator, Expression* children)
{
    auto expr = new Expression(evaluator, children);
    expr->Validate();
    return expr;
}

void Expression::Validate()
{
    m_evaluator->ValidateExpression(this);
}

ExpressionEvaluator* Expression::getEvaluator()
{
    return m_evaluator;
}