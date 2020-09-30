#pragma warning disable 3021

// Generated from ExpressionAntlrParser.g4 by ANTLR 4.8

#pragma once


#include "../Code/pch.h"




class  ExpressionAntlrParser : public antlr4::Parser {
public:
  enum {
    STRING_INTERPOLATION_START = 1, PLUS = 2, SUBSTRACT = 3, NON = 4, XOR = 5, 
    ASTERISK = 6, SLASH = 7, PERCENT = 8, DOUBLE_EQUAL = 9, NOT_EQUAL = 10, 
    SINGLE_AND = 11, DOUBLE_AND = 12, DOUBLE_VERTICAL_CYLINDER = 13, LESS_THAN = 14, 
    MORE_THAN = 15, LESS_OR_EQUAl = 16, MORE_OR_EQUAL = 17, OPEN_BRACKET = 18, 
    CLOSE_BRACKET = 19, DOT = 20, OPEN_SQUARE_BRACKET = 21, CLOSE_SQUARE_BRACKET = 22, 
    OPEN_CURLY_BRACKET = 23, CLOSE_CURLY_BRACKET = 24, COMMA = 25, COLON = 26, 
    ARROW = 27, NUMBER = 28, WHITESPACE = 29, IDENTIFIER = 30, NEWLINE = 31, 
    STRING = 32, INVALID_TOKEN_DEFAULT_MODE = 33, TEMPLATE = 34, ESCAPE_CHARACTER = 35, 
    TEXT_CONTENT = 36
  };

  enum {
    RuleFile = 0, RuleExpression = 1, RulePrimaryExpression = 2, RuleStringInterpolation = 3, 
    RuleTextContent = 4, RuleArgsList = 5, RuleLambda = 6, RuleKeyValuePairList = 7, 
    RuleKeyValuePair = 8, RuleKey = 9
  };

  ExpressionAntlrParser(antlr4::TokenStream *input);
  ~ExpressionAntlrParser();

  virtual std::string getGrammarFileName() const override;
  virtual const antlr4::atn::ATN& getATN() const override { return _atn; };
  virtual const std::vector<std::string>& getTokenNames() const override { return _tokenNames; }; // deprecated: use vocabulary instead.
  virtual const std::vector<std::string>& getRuleNames() const override;
  virtual antlr4::dfa::Vocabulary& getVocabulary() const override;


  class FileContext;
  class ExpressionContext;
  class PrimaryExpressionContext;
  class StringInterpolationContext;
  class TextContentContext;
  class ArgsListContext;
  class LambdaContext;
  class KeyValuePairListContext;
  class KeyValuePairContext;
  class KeyContext; 

  class  FileContext : public antlr4::ParserRuleContext {
  public:
    FileContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    ExpressionContext *expression();
    antlr4::tree::TerminalNode *EOF();


    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
   
  };

  FileContext* file();

  class  ExpressionContext : public antlr4::ParserRuleContext {
  public:
    ExpressionContext(antlr4::ParserRuleContext *parent, size_t invokingState);
   
    ExpressionContext() = default;
    void copyFrom(ExpressionContext *context);
    using antlr4::ParserRuleContext::copyFrom;

    virtual size_t getRuleIndex() const override;

   
  };

  class  UnaryOpExpContext : public ExpressionContext {
  public:
    UnaryOpExpContext(ExpressionContext *ctx);

    ExpressionContext *expression();
    antlr4::tree::TerminalNode *NON();
    antlr4::tree::TerminalNode *SUBSTRACT();
    antlr4::tree::TerminalNode *PLUS();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  BinaryOpExpContext : public ExpressionContext {
  public:
    BinaryOpExpContext(ExpressionContext *ctx);

    std::vector<ExpressionContext *> expression();
    ExpressionContext* expression(size_t i);
    antlr4::tree::TerminalNode *XOR();
    antlr4::tree::TerminalNode *ASTERISK();
    antlr4::tree::TerminalNode *SLASH();
    antlr4::tree::TerminalNode *PERCENT();
    antlr4::tree::TerminalNode *PLUS();
    antlr4::tree::TerminalNode *SUBSTRACT();
    antlr4::tree::TerminalNode *DOUBLE_EQUAL();
    antlr4::tree::TerminalNode *NOT_EQUAL();
    antlr4::tree::TerminalNode *SINGLE_AND();
    antlr4::tree::TerminalNode *LESS_THAN();
    antlr4::tree::TerminalNode *LESS_OR_EQUAl();
    antlr4::tree::TerminalNode *MORE_THAN();
    antlr4::tree::TerminalNode *MORE_OR_EQUAL();
    antlr4::tree::TerminalNode *DOUBLE_AND();
    antlr4::tree::TerminalNode *DOUBLE_VERTICAL_CYLINDER();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  PrimaryExpContext : public ExpressionContext {
  public:
    PrimaryExpContext(ExpressionContext *ctx);

    PrimaryExpressionContext *primaryExpression();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  ExpressionContext* expression();
  ExpressionContext* expression(int precedence);
  class  PrimaryExpressionContext : public antlr4::ParserRuleContext {
  public:
    PrimaryExpressionContext(antlr4::ParserRuleContext *parent, size_t invokingState);
   
    PrimaryExpressionContext() = default;
    void copyFrom(PrimaryExpressionContext *context);
    using antlr4::ParserRuleContext::copyFrom;

    virtual size_t getRuleIndex() const override;

   
  };

  class  FuncInvokeExpContext : public PrimaryExpressionContext {
  public:
    FuncInvokeExpContext(PrimaryExpressionContext *ctx);

    PrimaryExpressionContext *primaryExpression();
    antlr4::tree::TerminalNode *OPEN_BRACKET();
    antlr4::tree::TerminalNode *CLOSE_BRACKET();
    antlr4::tree::TerminalNode *NON();
    ArgsListContext *argsList();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  IdAtomContext : public PrimaryExpressionContext {
  public:
    IdAtomContext(PrimaryExpressionContext *ctx);

    antlr4::tree::TerminalNode *IDENTIFIER();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  JsonCreationExpContext : public PrimaryExpressionContext {
  public:
    JsonCreationExpContext(PrimaryExpressionContext *ctx);

    antlr4::tree::TerminalNode *OPEN_CURLY_BRACKET();
    antlr4::tree::TerminalNode *CLOSE_CURLY_BRACKET();
    KeyValuePairListContext *keyValuePairList();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  StringAtomContext : public PrimaryExpressionContext {
  public:
    StringAtomContext(PrimaryExpressionContext *ctx);

    antlr4::tree::TerminalNode *STRING();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  IndexAccessExpContext : public PrimaryExpressionContext {
  public:
    IndexAccessExpContext(PrimaryExpressionContext *ctx);

    PrimaryExpressionContext *primaryExpression();
    antlr4::tree::TerminalNode *OPEN_SQUARE_BRACKET();
    ExpressionContext *expression();
    antlr4::tree::TerminalNode *CLOSE_SQUARE_BRACKET();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  StringInterpolationAtomContext : public PrimaryExpressionContext {
  public:
    StringInterpolationAtomContext(PrimaryExpressionContext *ctx);

    StringInterpolationContext *stringInterpolation();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  MemberAccessExpContext : public PrimaryExpressionContext {
  public:
    MemberAccessExpContext(PrimaryExpressionContext *ctx);

    PrimaryExpressionContext *primaryExpression();
    antlr4::tree::TerminalNode *DOT();
    antlr4::tree::TerminalNode *IDENTIFIER();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  ParenthesisExpContext : public PrimaryExpressionContext {
  public:
    ParenthesisExpContext(PrimaryExpressionContext *ctx);

    antlr4::tree::TerminalNode *OPEN_BRACKET();
    ExpressionContext *expression();
    antlr4::tree::TerminalNode *CLOSE_BRACKET();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  NumericAtomContext : public PrimaryExpressionContext {
  public:
    NumericAtomContext(PrimaryExpressionContext *ctx);

    antlr4::tree::TerminalNode *NUMBER();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  class  ArrayCreationExpContext : public PrimaryExpressionContext {
  public:
    ArrayCreationExpContext(PrimaryExpressionContext *ctx);

    antlr4::tree::TerminalNode *OPEN_SQUARE_BRACKET();
    antlr4::tree::TerminalNode *CLOSE_SQUARE_BRACKET();
    ArgsListContext *argsList();

    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
  };

  PrimaryExpressionContext* primaryExpression();
  PrimaryExpressionContext* primaryExpression(int precedence);
  class  StringInterpolationContext : public antlr4::ParserRuleContext {
  public:
    StringInterpolationContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    std::vector<antlr4::tree::TerminalNode *> STRING_INTERPOLATION_START();
    antlr4::tree::TerminalNode* STRING_INTERPOLATION_START(size_t i);
    std::vector<antlr4::tree::TerminalNode *> ESCAPE_CHARACTER();
    antlr4::tree::TerminalNode* ESCAPE_CHARACTER(size_t i);
    std::vector<antlr4::tree::TerminalNode *> TEMPLATE();
    antlr4::tree::TerminalNode* TEMPLATE(size_t i);
    std::vector<TextContentContext *> textContent();
    TextContentContext* textContent(size_t i);


    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
   
  };

  StringInterpolationContext* stringInterpolation();

  class  TextContentContext : public antlr4::ParserRuleContext {
  public:
    TextContentContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    std::vector<antlr4::tree::TerminalNode *> TEXT_CONTENT();
    antlr4::tree::TerminalNode* TEXT_CONTENT(size_t i);


    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
   
  };

  TextContentContext* textContent();

  class  ArgsListContext : public antlr4::ParserRuleContext {
  public:
    ArgsListContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    std::vector<LambdaContext *> lambda();
    LambdaContext* lambda(size_t i);
    std::vector<ExpressionContext *> expression();
    ExpressionContext* expression(size_t i);
    std::vector<antlr4::tree::TerminalNode *> COMMA();
    antlr4::tree::TerminalNode* COMMA(size_t i);


    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
   
  };

  ArgsListContext* argsList();

  class  LambdaContext : public antlr4::ParserRuleContext {
  public:
    LambdaContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *IDENTIFIER();
    antlr4::tree::TerminalNode *ARROW();
    ExpressionContext *expression();


    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
   
  };

  LambdaContext* lambda();

  class  KeyValuePairListContext : public antlr4::ParserRuleContext {
  public:
    KeyValuePairListContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    std::vector<KeyValuePairContext *> keyValuePair();
    KeyValuePairContext* keyValuePair(size_t i);
    std::vector<antlr4::tree::TerminalNode *> COMMA();
    antlr4::tree::TerminalNode* COMMA(size_t i);


    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
   
  };

  KeyValuePairListContext* keyValuePairList();

  class  KeyValuePairContext : public antlr4::ParserRuleContext {
  public:
    KeyValuePairContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    KeyContext *key();
    antlr4::tree::TerminalNode *COLON();
    ExpressionContext *expression();


    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
   
  };

  KeyValuePairContext* keyValuePair();

  class  KeyContext : public antlr4::ParserRuleContext {
  public:
    KeyContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *IDENTIFIER();
    antlr4::tree::TerminalNode *STRING();


    virtual antlrcpp::Any accept(antlr4::tree::ParseTreeVisitor *visitor) override;
   
  };

  KeyContext* key();


  virtual bool sempred(antlr4::RuleContext *_localctx, size_t ruleIndex, size_t predicateIndex) override;
  bool expressionSempred(ExpressionContext *_localctx, size_t predicateIndex);
  bool primaryExpressionSempred(PrimaryExpressionContext *_localctx, size_t predicateIndex);

private:
  static std::vector<antlr4::dfa::DFA> _decisionToDFA;
  static antlr4::atn::PredictionContextCache _sharedContextCache;
  static std::vector<std::string> _ruleNames;
  static std::vector<std::string> _tokenNames;

  static std::vector<std::string> _literalNames;
  static std::vector<std::string> _symbolicNames;
  static antlr4::dfa::Vocabulary _vocabulary;
  static antlr4::atn::ATN _atn;
  static std::vector<uint16_t> _serializedATN;


  struct Initializer {
    Initializer();
  };
  static Initializer _init;
};

