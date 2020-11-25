#pragma warning disable 3021

// Generated from .\ExpressionAntlrParser.g4 by ANTLR 4.8

#pragma once


#include "../antlr4-runtime/antlr4-runtime.h"
#include "ExpressionAntlrParser.h"



/**
 * This class defines an abstract visitor for a parse tree
 * produced by ExpressionAntlrParser.
 */
class  ExpressionAntlrParserVisitor : public antlr4::tree::AbstractParseTreeVisitor {
public:

  /**
   * Visit parse trees produced by ExpressionAntlrParser.
   */
    virtual antlrcpp::Any visitFile(ExpressionAntlrParser::FileContext *context) = 0;

    virtual antlrcpp::Any visitUnaryOpExp(ExpressionAntlrParser::UnaryOpExpContext *context) = 0;

    virtual antlrcpp::Any visitBinaryOpExp(ExpressionAntlrParser::BinaryOpExpContext *context) = 0;

    virtual antlrcpp::Any visitPrimaryExp(ExpressionAntlrParser::PrimaryExpContext *context) = 0;

    virtual antlrcpp::Any visitFuncInvokeExp(ExpressionAntlrParser::FuncInvokeExpContext *context) = 0;

    virtual antlrcpp::Any visitIdAtom(ExpressionAntlrParser::IdAtomContext *context) = 0;

    virtual antlrcpp::Any visitJsonCreationExp(ExpressionAntlrParser::JsonCreationExpContext *context) = 0;

    virtual antlrcpp::Any visitStringAtom(ExpressionAntlrParser::StringAtomContext *context) = 0;

    virtual antlrcpp::Any visitIndexAccessExp(ExpressionAntlrParser::IndexAccessExpContext *context) = 0;

    virtual antlrcpp::Any visitStringInterpolationAtom(ExpressionAntlrParser::StringInterpolationAtomContext *context) = 0;

    virtual antlrcpp::Any visitMemberAccessExp(ExpressionAntlrParser::MemberAccessExpContext *context) = 0;

    virtual antlrcpp::Any visitParenthesisExp(ExpressionAntlrParser::ParenthesisExpContext *context) = 0;

    virtual antlrcpp::Any visitNumericAtom(ExpressionAntlrParser::NumericAtomContext *context) = 0;

    virtual antlrcpp::Any visitArrayCreationExp(ExpressionAntlrParser::ArrayCreationExpContext *context) = 0;

    virtual antlrcpp::Any visitStringInterpolation(ExpressionAntlrParser::StringInterpolationContext *context) = 0;

    virtual antlrcpp::Any visitTextContent(ExpressionAntlrParser::TextContentContext *context) = 0;

    virtual antlrcpp::Any visitArgsList(ExpressionAntlrParser::ArgsListContext *context) = 0;

    virtual antlrcpp::Any visitLambda(ExpressionAntlrParser::LambdaContext *context) = 0;

    virtual antlrcpp::Any visitKeyValuePairList(ExpressionAntlrParser::KeyValuePairListContext *context) = 0;

    virtual antlrcpp::Any visitKeyValuePair(ExpressionAntlrParser::KeyValuePairContext *context) = 0;

    virtual antlrcpp::Any visitKey(ExpressionAntlrParser::KeyContext *context) = 0;


};

