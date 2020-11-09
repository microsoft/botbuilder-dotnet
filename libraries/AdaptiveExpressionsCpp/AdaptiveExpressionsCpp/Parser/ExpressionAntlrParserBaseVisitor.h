#pragma warning disable 3021

// Generated from .\ExpressionAntlrParser.g4 by ANTLR 4.8

#pragma once


#include "../antlr4-runtime/antlr4-runtime.h"
#include "ExpressionAntlrParserVisitor.h"


/**
 * This class provides an empty implementation of ExpressionAntlrParserVisitor, which can be
 * extended to create a visitor which only needs to handle a subset of the available methods.
 */
class  ExpressionAntlrParserBaseVisitor : public ExpressionAntlrParserVisitor {
public:

  virtual antlrcpp::Any visitFile(ExpressionAntlrParser::FileContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitUnaryOpExp(ExpressionAntlrParser::UnaryOpExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitBinaryOpExp(ExpressionAntlrParser::BinaryOpExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitPrimaryExp(ExpressionAntlrParser::PrimaryExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitFuncInvokeExp(ExpressionAntlrParser::FuncInvokeExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitIdAtom(ExpressionAntlrParser::IdAtomContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitJsonCreationExp(ExpressionAntlrParser::JsonCreationExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitStringAtom(ExpressionAntlrParser::StringAtomContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitIndexAccessExp(ExpressionAntlrParser::IndexAccessExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitStringInterpolationAtom(ExpressionAntlrParser::StringInterpolationAtomContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitMemberAccessExp(ExpressionAntlrParser::MemberAccessExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitParenthesisExp(ExpressionAntlrParser::ParenthesisExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitNumericAtom(ExpressionAntlrParser::NumericAtomContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitArrayCreationExp(ExpressionAntlrParser::ArrayCreationExpContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitStringInterpolation(ExpressionAntlrParser::StringInterpolationContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitTextContent(ExpressionAntlrParser::TextContentContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitArgsList(ExpressionAntlrParser::ArgsListContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitLambda(ExpressionAntlrParser::LambdaContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitKeyValuePairList(ExpressionAntlrParser::KeyValuePairListContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitKeyValuePair(ExpressionAntlrParser::KeyValuePairContext *ctx) override {
    return visitChildren(ctx);
  }

  virtual antlrcpp::Any visitKey(ExpressionAntlrParser::KeyContext *ctx) override {
    return visitChildren(ctx);
  }


};

