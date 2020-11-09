#pragma warning disable 3021

// Generated from .\ExpressionAntlrLexer.g4 by ANTLR 4.8

#pragma once


#include "../antlr4-runtime/antlr4-runtime.h"




class  ExpressionAntlrLexer : public antlr4::Lexer {
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
    STRING_INTERPOLATION_MODE = 1
  };

  ExpressionAntlrLexer(antlr4::CharStream *input);
  ~ExpressionAntlrLexer();


    bool ignoreWS = true;      // usually we ignore whitespace, but inside stringInterpolation, whitespace is significant

  virtual std::string getGrammarFileName() const override;
  virtual const std::vector<std::string>& getRuleNames() const override;

  virtual const std::vector<std::string>& getChannelNames() const override;
  virtual const std::vector<std::string>& getModeNames() const override;
  virtual const std::vector<std::string>& getTokenNames() const override; // deprecated, use vocabulary instead
  virtual antlr4::dfa::Vocabulary& getVocabulary() const override;

  virtual const std::vector<uint16_t> getSerializedATN() const override;
  virtual const antlr4::atn::ATN& getATN() const override;

  virtual void action(antlr4::RuleContext *context, size_t ruleIndex, size_t actionIndex) override;
  virtual bool sempred(antlr4::RuleContext *_localctx, size_t ruleIndex, size_t predicateIndex) override;

private:
  static std::vector<antlr4::dfa::DFA> _decisionToDFA;
  static antlr4::atn::PredictionContextCache _sharedContextCache;
  static std::vector<std::string> _ruleNames;
  static std::vector<std::string> _tokenNames;
  static std::vector<std::string> _channelNames;
  static std::vector<std::string> _modeNames;

  static std::vector<std::string> _literalNames;
  static std::vector<std::string> _symbolicNames;
  static antlr4::dfa::Vocabulary _vocabulary;
  static antlr4::atn::ATN _atn;
  static std::vector<uint16_t> _serializedATN;


  // Individual action functions triggered by action() above.
  void STRING_INTERPOLATION_STARTAction(antlr4::RuleContext *context, size_t actionIndex);
  void STRING_INTERPOLATION_ENDAction(antlr4::RuleContext *context, size_t actionIndex);

  // Individual semantic predicate functions triggered by sempred() above.
  bool WHITESPACESempred(antlr4::RuleContext *_localctx, size_t predicateIndex);

  struct Initializer {
    Initializer();
  };
  static Initializer _init;
};

