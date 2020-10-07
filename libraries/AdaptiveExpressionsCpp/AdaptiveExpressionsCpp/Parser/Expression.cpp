#include "Expression.h"
#include "Constant.h"

Expression::Expression()
{

}

Expression::Expression(std::string type, Expression* children)
{

}

Expression::Expression(ExpressionEvaluator* evaluator, Expression* children)
{

}

std::string trimStart(const std::string& s, std::string chars)
{
    int i = 0;
    for (const auto& c : s)
    {
        bool isTrimmable{ false };
        for (const auto& t : chars)
        {
            if (c == t)
            {
                isTrimmable = true;
            }
        }

        if (!isTrimmable)
        {
            return s.substr(i);
        }

        i++;
    }

    return std::string();
}

Expression* Expression::ConstantExpression(antlrcpp::Any value)
{
    return new Constant(value);
}

Expression* Expression::Parse(std::string expression, EvaluatorLookup lookup)
{
    ExpressionParser* parser;
    if (lookup == nullptr)
    {
        parser = new ExpressionParser(Expression::Lookup);
    }
    else
    {
        parser = new ExpressionParser(lookup);
    }

    return parser->Parse(!(expression.empty()) ? trimStart(expression, "=") : std::string());
}

Expression* Expression::MakeExpression(ExpressionEvaluator* evaluator, Expression* children)
{
    auto expr = new Expression(evaluator, children);
    expr->Validate();
    return expr;
}

ExpressionEvaluator* Expression::Lookup(std::string functionName)
{
    return nullptr;
}

void Expression::Validate()
{
    m_evaluator->ValidateExpression(this);
}

ExpressionEvaluator* Expression::getEvaluator()
{
    return m_evaluator;
}