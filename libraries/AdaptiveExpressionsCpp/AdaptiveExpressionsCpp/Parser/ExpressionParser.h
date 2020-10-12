// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once
#include "../Code/pch.h"

#include "ExpressionAntlrParserBaseVisitor.h"
#include <string>

class ExpressionParser // : antlr4::Parser // ?? IExpressionParser
{
public:
    ExpressionParser(EvaluatorLookup lookup);

    static antlr4::tree::ParseTree* AntlrParse(std::string expression);
    Expression* Parse(std::string expression);
    EvaluatorLookup getEvaluatorLookup();

private:

    class ExpressionTransformer : ExpressionAntlrParserBaseVisitor
    {

    public:
        ExpressionTransformer(EvaluatorLookup lookup);
        Expression* Transform(antlr4::tree::ParseTree* context);
        antlrcpp::Any visitFile(ExpressionAntlrParser::FileContext* ctx) override;
        antlrcpp::Any visitStringAtom(ExpressionAntlrParser::StringAtomContext* ctx) override;
        antlrcpp::Any visitBinaryOpExp(ExpressionAntlrParser::BinaryOpExpContext* ctx) override;

    private:
        Expression* MakeExpression(std::string functionType, Expression* children, ...);

        const std::string escapeRegex = "\\[^\r\n]?";
        EvaluatorLookup m_lookupFunction;
    };

    // this is a function, we could use a function pointer or a static class for this
    EvaluatorLookup m_evaluatorLookup;
};