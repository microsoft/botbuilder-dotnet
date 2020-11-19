#include "Expression.h"
#include "Constant.h"

const FunctionTable* Expression::Functions = new FunctionTable();

Expression::Expression()
{
}

Expression::Expression(std::string type, std::vector<Expression*> children)
{
    // m_evaluator = Functions[type]; // ? ? throw new SyntaxErrorException($"{type} does not have an evaluator, it's not a built-in function or a custom function.");
    m_children = children;
}

Expression::Expression(ExpressionEvaluator* evaluator) : Expression(evaluator, std::vector<Expression*>())
{
}

Expression::Expression(ExpressionEvaluator* evaluator, std::vector<Expression*> children)
{
    if (evaluator == nullptr)
    {
        throw std::invalid_argument("Null evaluator constructing Expression");
    }

    m_evaluator = evaluator;
    m_children = children;
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

Expression* Expression::ConstantExpression(std::any value)
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

Expression* Expression::MakeExpression(ExpressionEvaluator* evaluator, std::vector<Expression*> children)
{
    auto expr = new Expression(evaluator, children);
    expr->Validate();
    return expr;
}

ExpressionEvaluator* Expression::Lookup(std::string functionName)
{
    if (Expression::Functions->find(functionName) == Expression::Functions->end())
    {
        return nullptr;
    }

    return Expression::Functions->at(functionName);
}

void Expression::Validate()
{
    m_evaluator->ValidateExpression(this);
}

ExpressionEvaluator* Expression::getEvaluator()
{
    return m_evaluator;
}

ReturnType Expression::getReturnType()
{
    return m_evaluator->getReturnType();
}

size_t Expression::getChildrenCount()
{
    return m_children.size();
}

Expression* Expression::getChildAt(size_t pos)
{
    return m_children[pos];
}
/*
antlrcpp::Any Expression::getValue()
{
    return m_expressionValue;
}
*/

/*
Expression* Expression::getChildren()
{
    return m_children;
}
*/

ValueErrorTuple Expression::TryEvaluate(void* state, void* options)
{
    return m_evaluator->TryEvaluate(this, state, options);
}