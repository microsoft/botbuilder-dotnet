// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "ExpressionParser.h"
#include "Expression.h"
#include "ExpressionType.h"
#include "Constant.h"

#include "ExpressionAntlrLexer.h"
#include "ExpressionAntlrParser.h"

#include <iostream>
#include <string>

ExpressionParser::ExpressionTransformer::ExpressionTransformer(EvaluatorLookup lookup)
{
    m_lookupFunction = lookup;
}

Expression* ExpressionParser::ExpressionTransformer::Transform(antlr4::tree::ParseTree* context)
{
    return visit(context);
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitFile(ExpressionAntlrParser::FileContext* ctx)
{
    std::vector<Expression*> expressions;

    auto unaryOperationName = ctx->children[0]->getText();
    auto operand = visit(ctx->expression());
    if (unaryOperationName == ExpressionType::Subtract
        || unaryOperationName == ExpressionType::Add)
    {
        expressions.push_back((new Constant(0)));
        expressions.push_back(operand);

        return MakeExpression(unaryOperationName, expressions);
    }

    // expressions.push_back(new Expression(operand));
    return MakeExpression(unaryOperationName, expressions);
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitStringAtom(ExpressionAntlrParser::StringAtomContext* ctx)
{
    std::string text = ctx->getText();
    /*
    if (text.StartsWith("'", StringComparison.Ordinal) && text.EndsWith("'", StringComparison.Ordinal))
    {
        text = text.Substring(1, text.Length - 2).Replace("\\'", "'");
    }
    else if (text.StartsWith("\"", StringComparison.Ordinal) && text.EndsWith("\"", StringComparison.Ordinal))
    {
        text = text.Substring(1, text.Length - 2).Replace("\\\"", "\"");
    }
    else
    {
        throw new Exception($"Invalid string {text}");
    }
    */

    return Expression::ConstantExpression(text);
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitUnaryOpExp(ExpressionAntlrParser::UnaryOpExpContext* ctx)
{
    auto unaryOperationName = ctx->children.at(0)->getText();
    auto operand = visit(ctx->expression());
    if (unaryOperationName == ExpressionType::Subtract
        || unaryOperationName == ExpressionType::Add)
    {
        return MakeExpression(unaryOperationName, std::vector<Expression*>{new Constant(0), operand});
    }

    return MakeExpression(unaryOperationName, std::vector<Expression*>{operand});
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitBinaryOpExp(ExpressionAntlrParser::BinaryOpExpContext* ctx)
{
    auto binaryOperationName = ctx->children.at(1)->getText();
    antlrcpp::Any left = visit(ctx->expression(0));
    antlrcpp::Any right = visit(ctx->expression(1));

    Expression* leftExpression1 = left.as<Expression*>();

    std::vector<Expression*> children;
    children.push_back(leftExpression1);
    children.push_back(right.as<Expression*>());

    return MakeExpression(binaryOperationName, children);
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitFuncInvokeExp(ExpressionAntlrParser::FuncInvokeExpContext* context)
{
    ExpressionAntlrParser::ArgsListContext* argContext = context->argsList();
    std::vector<antlr4::tree::ParseTree*> argChildren = argContext->children;

    std::vector<Expression*> childExpressions;
    for (auto argChild : argChildren)
    {
        // TODO - in the C# version this part happens in a ProcessArgsList function that also checks for LambdaContexts
        if (dynamic_cast<ExpressionAntlrParser::ExpressionContext*>(argChild))
        {
            auto childExpression = visit(argChild);
            childExpressions.push_back(childExpression);
        }
    }

    std::string functionName = context->primaryExpression()->getText();
    if (context->NON() != nullptr)
    {
        functionName += context->NON()->getText();
    }

    return MakeExpression(functionName, childExpressions);
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitIdAtom(ExpressionAntlrParser::IdAtomContext* ctx)
{
    return antlrcpp::Any();
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitIndexAccessExp(ExpressionAntlrParser::IndexAccessExpContext* ctx)
{
    return antlrcpp::Any();
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitMemberAccessExp(ExpressionAntlrParser::MemberAccessExpContext* ctx)
{
    return antlrcpp::Any();
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitParenthesisExp(ExpressionAntlrParser::ParenthesisExpContext* ctx)
{
    return antlrcpp::Any();
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitArrayCreationExp(ExpressionAntlrParser::ArrayCreationExpContext* ctx)
{
    return antlrcpp::Any();
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitNumericAtom(ExpressionAntlrParser::NumericAtomContext* ctx)
{
    std::string numericString = ctx->getText();

    try
    {
        int integer = std::stoi(numericString);
        return Expression::ConstantExpression(integer);
    }
    catch (const std::bad_any_cast&)
    {
    }

    try
    {
        long long int longInteger = std::stol(numericString);
        return Expression::ConstantExpression(longInteger);
    }
    catch (const std::bad_any_cast&) {}

    try
    {
        double decimalValue = std::stod(numericString);
        return Expression::ConstantExpression(decimalValue);
    }
    catch (const std::bad_any_cast&)
    {
        // throw new Exception($"{context.GetText()} is not a number in expression '{context.GetText()}'");
    }


    return Expression::ConstantExpression(0);
}

/*
public override Expression VisitStringAtom([NotNull] ExpressionAntlrParser.StringAtomContext context)
{

    var text = context.GetText();
    if (text.StartsWith("'", StringComparison.Ordinal) && text.EndsWith("'", StringComparison.Ordinal))
    {
        text = text.Substring(1, text.Length - 2).Replace("\\'", "'");
    }
    else if (text.StartsWith("\"", StringComparison.Ordinal) && text.EndsWith("\"", StringComparison.Ordinal))
    {
        text = text.Substring(1, text.Length - 2).Replace("\\\"", "\"");
    }
    else
    {
        throw new Exception($"Invalid string {text}");
    }

    return Expression.ConstantExpression(EvalEscape(text));

    return nullptr;
}
*/

Expression* ExpressionParser::ExpressionTransformer::MakeExpression(std::string functionType, std::vector<Expression*> children)
{
    return Expression::MakeExpression(m_lookupFunction(functionType), children);
    // If lookup function fails, throw this: throw new SyntaxErrorException($"{functionType} does not have an evaluator, it's not a built-in function or a custom function.")
}

EvaluatorLookup ExpressionParser::getEvaluatorLookup()
{
    return m_evaluatorLookup;
}

/*
Expression* ExpressionParser::Parse(std::string expression)
{

    if (expression.size() == 0)
    {
        return Expression::ConstantExpression(std::string());
    }
    else
    {
        return new ExpressionTransformer(m_evaluatorLookup)->Transform(AntlrParse(expression));
    }

}
*/

ExpressionParser::ExpressionParser(EvaluatorLookup lookup)
{
    m_evaluatorLookup = lookup;
}

antlr4::tree::ParseTree* ExpressionParser::AntlrParse(std::string expression)
{
    /*
    // The expressionDict is a cache i think
    if (expressionDict.TryGetValue(expression, out var expressionParseTree))
    {
        return expressionParseTree;
    }
    */

    auto inputStream = new antlr4::ANTLRInputStream(expression);
    auto lexer = new ExpressionAntlrLexer(inputStream);
    lexer->removeErrorListeners();
    auto tokenStream = new antlr4::CommonTokenStream(lexer);
    auto parser = new ExpressionAntlrParser(tokenStream);
    parser->removeErrorListeners();
    // parser->addErrorListener(new ParserErrorListener());
    parser->setBuildParseTree(true);

    ExpressionAntlrParser::ExpressionContext* expressionContext = nullptr;
    ExpressionAntlrParser::FileContext* fileContext = parser->file();
    if (fileContext != nullptr)
    {
        expressionContext = fileContext->expression();
    }

    // expressionDict.TryAdd(expression, expressionContext);

    return expressionContext;
}

Expression* ExpressionParser::Parse(std::string expression)
{
    if (expression.empty())
    {
        return Expression::ConstantExpression(std::string());
    }
    else
    {
        ExpressionTransformer* transformer = new ExpressionTransformer(m_evaluatorLookup);
        return transformer->Transform(AntlrParse(expression));
    }

}
