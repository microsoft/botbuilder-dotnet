// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once
#include "../Code/pch.h"

#include "ExpressionAntlrParserBaseVisitor.h"
#include <string>

class ExpressionParser
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
        
        antlrcpp::Any visitUnaryOpExp(ExpressionAntlrParser::UnaryOpExpContext* ctx) override;
        antlrcpp::Any visitBinaryOpExp(ExpressionAntlrParser::BinaryOpExpContext* ctx) override;
        antlrcpp::Any visitFuncInvokeExp(ExpressionAntlrParser::FuncInvokeExpContext* ctx) override;
        antlrcpp::Any visitIdAtom(ExpressionAntlrParser::IdAtomContext* ctx) override;
        antlrcpp::Any visitIndexAccessExp(ExpressionAntlrParser::IndexAccessExpContext* ctx) override;
        antlrcpp::Any visitMemberAccessExp(ExpressionAntlrParser::MemberAccessExpContext* ctx) override;
        antlrcpp::Any visitParenthesisExp(ExpressionAntlrParser::ParenthesisExpContext* ctx) override;
        antlrcpp::Any visitArrayCreationExp(ExpressionAntlrParser::ArrayCreationExpContext* ctx) override;

        antlrcpp::Any visitStringAtom(ExpressionAntlrParser::StringAtomContext* ctx) override;
        antlrcpp::Any visitNumericAtom(ExpressionAntlrParser::NumericAtomContext* ctx) override;

    private:
        Expression* MakeExpression(std::string functionType, size_t childrenCount, std::vector<Expression*> children);

        const std::string escapeRegex = "\\[^\r\n]?";
        EvaluatorLookup m_lookupFunction;
    };

    // this is a function, we could use a function pointer or a static class for this
    EvaluatorLookup m_evaluatorLookup;
};