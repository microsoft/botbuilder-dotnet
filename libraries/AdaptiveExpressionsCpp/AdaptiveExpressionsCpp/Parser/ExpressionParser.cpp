// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "ExpressionParser.h"
#include "Expression.h"
#include "ExpressionType.h"
#include "Constant.h"

#include "ExpressionAntlrLexer.h"
#include "ExpressionAntlrParser.h"

/*
ExpressionTransformer::ExpressionTransformer(EvaluatorLookup lookup)
{
    m_lookupFunction = lookup;
}
*/

void ExpressionParser::ExpressionTransformer::Transform(antlr4::tree::ParseTree* context)
{
    visit(context);
}

antlrcpp::Any ExpressionParser::ExpressionTransformer::visitFile(ExpressionAntlrParser::FileContext* ctx)
{
    auto unaryOperationName = ctx->children[0]->getText();
    auto operand = visit(ctx->expression());
    if (unaryOperationName == ExpressionType::Subtract
        || unaryOperationName == ExpressionType::Add)
    {
        return MakeExpression(unaryOperationName, new Constant(0), operand);
    }

    return MakeExpression(unaryOperationName, operand);
}



Expression* ExpressionParser::ExpressionTransformer::MakeExpression(std::string functionType, Expression* children, ...)
{
    return nullptr;
    // Expression::MakeExpression(_lookupFunction(functionType) ? ? throw new SyntaxErrorException($"{functionType} does not have an evaluator, it's not a built-in function or a custom function."), children);
}


/*
void ExpressionParser::getEvaluatorLookup()
{
    return m_evaluatorLookup;
}
*/

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

antlr4::tree::ParseTree* ExpressionParser::AntlrParse(std::string expression)
{
    /*
    if (expressionDict.TryGetValue(expression, out var expressionParseTree))
    {
        return expressionParseTree;
    }
    */
    /*

    auto inputStream = new antlr4::ANTLRInputStream("a string");
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
    */

    return nullptr;
}