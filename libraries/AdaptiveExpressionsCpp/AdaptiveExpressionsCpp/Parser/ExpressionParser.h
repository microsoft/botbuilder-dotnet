// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once
#include "../Code/pch.h"

#include "ExpressionAntlrParserBaseVisitor.h"
#include "Expression.h"
#include <string>

class ExpressionParser // : antlr4::Parser // ?? IExpressionParser
{

// public:
    // Expression* Parse(std::string expression);
    //EvaluatorLookup getEvaluatorLookup();
        
public:
    static antlr4::tree::ParseTree* AntlrParse(std::string expression);


private:

    class ExpressionTransformer : ExpressionAntlrParserBaseVisitor
    {

    public:
        // ExpressionTransformer(EvaluatorLookup lookup);
        void Transform(antlr4::tree::ParseTree* context);
        antlrcpp::Any visitFile(ExpressionAntlrParser::FileContext* ctx) override;

    private:
        Expression* MakeExpression(std::string functionType, Expression* children, ...);

        const std::string escapeRegex = "\\[^\r\n]?";
        // EvaluatorLookup m_lookupFunction;
    };

    // this is a function, we could use a function pointer or a static class for this
    //EvaluatorLookup m_evaluatorLookup;
};