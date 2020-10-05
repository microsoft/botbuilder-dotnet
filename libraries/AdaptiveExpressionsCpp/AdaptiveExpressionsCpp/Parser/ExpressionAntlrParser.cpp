#pragma warning disable 3021

// Generated from .\ExpressionAntlrParser.g4 by ANTLR 4.8


#include "ExpressionAntlrParserVisitor.h"

#include "ExpressionAntlrParser.h"


using namespace antlrcpp;
using namespace antlr4;

ExpressionAntlrParser::ExpressionAntlrParser(TokenStream *input) : Parser(input) {
  _interpreter = new atn::ParserATNSimulator(this, _atn, _decisionToDFA, _sharedContextCache);
}

ExpressionAntlrParser::~ExpressionAntlrParser() {
  delete _interpreter;
}

std::string ExpressionAntlrParser::getGrammarFileName() const {
  return "ExpressionAntlrParser.g4";
}

const std::vector<std::string>& ExpressionAntlrParser::getRuleNames() const {
  return _ruleNames;
}

dfa::Vocabulary& ExpressionAntlrParser::getVocabulary() const {
  return _vocabulary;
}


//----------------- FileContext ------------------------------------------------------------------

ExpressionAntlrParser::FileContext::FileContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::FileContext::expression() {
  return getRuleContext<ExpressionAntlrParser::ExpressionContext>(0);
}

tree::TerminalNode* ExpressionAntlrParser::FileContext::EOF() {
  return getToken(ExpressionAntlrParser::EOF, 0);
}


size_t ExpressionAntlrParser::FileContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleFile;
}


antlrcpp::Any ExpressionAntlrParser::FileContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitFile(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::FileContext* ExpressionAntlrParser::file() {
  FileContext *_localctx = _tracker.createInstance<FileContext>(_ctx, getState());
  enterRule(_localctx, 0, ExpressionAntlrParser::RuleFile);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(20);
    expression(0);
    setState(21);
    match(ExpressionAntlrParser::EOF);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- ExpressionContext ------------------------------------------------------------------

ExpressionAntlrParser::ExpressionContext::ExpressionContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}


size_t ExpressionAntlrParser::ExpressionContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleExpression;
}

void ExpressionAntlrParser::ExpressionContext::copyFrom(ExpressionContext *ctx) {
  ParserRuleContext::copyFrom(ctx);
}

//----------------- UnaryOpExpContext ------------------------------------------------------------------

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::UnaryOpExpContext::expression() {
  return getRuleContext<ExpressionAntlrParser::ExpressionContext>(0);
}

tree::TerminalNode* ExpressionAntlrParser::UnaryOpExpContext::NON() {
  return getToken(ExpressionAntlrParser::NON, 0);
}

tree::TerminalNode* ExpressionAntlrParser::UnaryOpExpContext::SUBSTRACT() {
  return getToken(ExpressionAntlrParser::SUBSTRACT, 0);
}

tree::TerminalNode* ExpressionAntlrParser::UnaryOpExpContext::PLUS() {
  return getToken(ExpressionAntlrParser::PLUS, 0);
}

ExpressionAntlrParser::UnaryOpExpContext::UnaryOpExpContext(ExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::UnaryOpExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitUnaryOpExp(this);
  else
    return visitor->visitChildren(this);
}
//----------------- BinaryOpExpContext ------------------------------------------------------------------

std::vector<ExpressionAntlrParser::ExpressionContext *> ExpressionAntlrParser::BinaryOpExpContext::expression() {
  return getRuleContexts<ExpressionAntlrParser::ExpressionContext>();
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::BinaryOpExpContext::expression(size_t i) {
  return getRuleContext<ExpressionAntlrParser::ExpressionContext>(i);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::XOR() {
  return getToken(ExpressionAntlrParser::XOR, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::ASTERISK() {
  return getToken(ExpressionAntlrParser::ASTERISK, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::SLASH() {
  return getToken(ExpressionAntlrParser::SLASH, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::PERCENT() {
  return getToken(ExpressionAntlrParser::PERCENT, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::PLUS() {
  return getToken(ExpressionAntlrParser::PLUS, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::SUBSTRACT() {
  return getToken(ExpressionAntlrParser::SUBSTRACT, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::DOUBLE_EQUAL() {
  return getToken(ExpressionAntlrParser::DOUBLE_EQUAL, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::NOT_EQUAL() {
  return getToken(ExpressionAntlrParser::NOT_EQUAL, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::SINGLE_AND() {
  return getToken(ExpressionAntlrParser::SINGLE_AND, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::LESS_THAN() {
  return getToken(ExpressionAntlrParser::LESS_THAN, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::LESS_OR_EQUAl() {
  return getToken(ExpressionAntlrParser::LESS_OR_EQUAl, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::MORE_THAN() {
  return getToken(ExpressionAntlrParser::MORE_THAN, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::MORE_OR_EQUAL() {
  return getToken(ExpressionAntlrParser::MORE_OR_EQUAL, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::DOUBLE_AND() {
  return getToken(ExpressionAntlrParser::DOUBLE_AND, 0);
}

tree::TerminalNode* ExpressionAntlrParser::BinaryOpExpContext::DOUBLE_VERTICAL_CYLINDER() {
  return getToken(ExpressionAntlrParser::DOUBLE_VERTICAL_CYLINDER, 0);
}

ExpressionAntlrParser::BinaryOpExpContext::BinaryOpExpContext(ExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::BinaryOpExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitBinaryOpExp(this);
  else
    return visitor->visitChildren(this);
}
//----------------- PrimaryExpContext ------------------------------------------------------------------

ExpressionAntlrParser::PrimaryExpressionContext* ExpressionAntlrParser::PrimaryExpContext::primaryExpression() {
  return getRuleContext<ExpressionAntlrParser::PrimaryExpressionContext>(0);
}

ExpressionAntlrParser::PrimaryExpContext::PrimaryExpContext(ExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::PrimaryExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitPrimaryExp(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::expression() {
   return expression(0);
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::expression(int precedence) {
  ParserRuleContext *parentContext = _ctx;
  size_t parentState = getState();
  ExpressionAntlrParser::ExpressionContext *_localctx = _tracker.createInstance<ExpressionContext>(_ctx, parentState);
  ExpressionAntlrParser::ExpressionContext *previousContext = _localctx;
  (void)previousContext; // Silence compiler, in case the context is not used by generated code.
  size_t startState = 2;
  enterRecursionRule(_localctx, 2, ExpressionAntlrParser::RuleExpression, precedence);

    size_t _la = 0;

  auto onExit = finally([=] {
    unrollRecursionContexts(parentContext);
  });
  try {
    size_t alt;
    enterOuterAlt(_localctx, 1);
    setState(27);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case ExpressionAntlrParser::PLUS:
      case ExpressionAntlrParser::SUBSTRACT:
      case ExpressionAntlrParser::NON: {
        _localctx = _tracker.createInstance<UnaryOpExpContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;

        setState(24);
        _la = _input->LA(1);
        if (!((((_la & ~ 0x3fULL) == 0) &&
          ((1ULL << _la) & ((1ULL << ExpressionAntlrParser::PLUS)
          | (1ULL << ExpressionAntlrParser::SUBSTRACT)
          | (1ULL << ExpressionAntlrParser::NON))) != 0))) {
        _errHandler->recoverInline(this);
        }
        else {
          _errHandler->reportMatch(this);
          consume();
        }
        setState(25);
        expression(10);
        break;
      }

      case ExpressionAntlrParser::STRING_INTERPOLATION_START:
      case ExpressionAntlrParser::OPEN_BRACKET:
      case ExpressionAntlrParser::OPEN_SQUARE_BRACKET:
      case ExpressionAntlrParser::OPEN_CURLY_BRACKET:
      case ExpressionAntlrParser::NUMBER:
      case ExpressionAntlrParser::IDENTIFIER:
      case ExpressionAntlrParser::STRING: {
        _localctx = _tracker.createInstance<PrimaryExpContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;
        setState(26);
        primaryExpression(0);
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    _ctx->stop = _input->LT(-1);
    setState(55);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 2, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        setState(53);
        _errHandler->sync(this);
        switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 1, _ctx)) {
        case 1: {
          auto newContext = _tracker.createInstance<BinaryOpExpContext>(_tracker.createInstance<ExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RuleExpression);
          setState(29);

          if (!(precpred(_ctx, 9))) throw FailedPredicateException(this, "precpred(_ctx, 9)");
          setState(30);
          match(ExpressionAntlrParser::XOR);
          setState(31);
          expression(9);
          break;
        }

        case 2: {
          auto newContext = _tracker.createInstance<BinaryOpExpContext>(_tracker.createInstance<ExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RuleExpression);
          setState(32);

          if (!(precpred(_ctx, 8))) throw FailedPredicateException(this, "precpred(_ctx, 8)");
          setState(33);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << ExpressionAntlrParser::ASTERISK)
            | (1ULL << ExpressionAntlrParser::SLASH)
            | (1ULL << ExpressionAntlrParser::PERCENT))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(34);
          expression(9);
          break;
        }

        case 3: {
          auto newContext = _tracker.createInstance<BinaryOpExpContext>(_tracker.createInstance<ExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RuleExpression);
          setState(35);

          if (!(precpred(_ctx, 7))) throw FailedPredicateException(this, "precpred(_ctx, 7)");
          setState(36);
          _la = _input->LA(1);
          if (!(_la == ExpressionAntlrParser::PLUS

          || _la == ExpressionAntlrParser::SUBSTRACT)) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(37);
          expression(8);
          break;
        }

        case 4: {
          auto newContext = _tracker.createInstance<BinaryOpExpContext>(_tracker.createInstance<ExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RuleExpression);
          setState(38);

          if (!(precpred(_ctx, 6))) throw FailedPredicateException(this, "precpred(_ctx, 6)");
          setState(39);
          _la = _input->LA(1);
          if (!(_la == ExpressionAntlrParser::DOUBLE_EQUAL

          || _la == ExpressionAntlrParser::NOT_EQUAL)) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(40);
          expression(7);
          break;
        }

        case 5: {
          auto newContext = _tracker.createInstance<BinaryOpExpContext>(_tracker.createInstance<ExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RuleExpression);
          setState(41);

          if (!(precpred(_ctx, 5))) throw FailedPredicateException(this, "precpred(_ctx, 5)");

          setState(42);
          match(ExpressionAntlrParser::SINGLE_AND);
          setState(43);
          expression(6);
          break;
        }

        case 6: {
          auto newContext = _tracker.createInstance<BinaryOpExpContext>(_tracker.createInstance<ExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RuleExpression);
          setState(44);

          if (!(precpred(_ctx, 4))) throw FailedPredicateException(this, "precpred(_ctx, 4)");
          setState(45);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << ExpressionAntlrParser::LESS_THAN)
            | (1ULL << ExpressionAntlrParser::MORE_THAN)
            | (1ULL << ExpressionAntlrParser::LESS_OR_EQUAl)
            | (1ULL << ExpressionAntlrParser::MORE_OR_EQUAL))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(46);
          expression(5);
          break;
        }

        case 7: {
          auto newContext = _tracker.createInstance<BinaryOpExpContext>(_tracker.createInstance<ExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RuleExpression);
          setState(47);

          if (!(precpred(_ctx, 3))) throw FailedPredicateException(this, "precpred(_ctx, 3)");
          setState(48);
          match(ExpressionAntlrParser::DOUBLE_AND);
          setState(49);
          expression(4);
          break;
        }

        case 8: {
          auto newContext = _tracker.createInstance<BinaryOpExpContext>(_tracker.createInstance<ExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RuleExpression);
          setState(50);

          if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
          setState(51);
          match(ExpressionAntlrParser::DOUBLE_VERTICAL_CYLINDER);
          setState(52);
          expression(3);
          break;
        }

        } 
      }
      setState(57);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 2, _ctx);
    }
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }
  return _localctx;
}

//----------------- PrimaryExpressionContext ------------------------------------------------------------------

ExpressionAntlrParser::PrimaryExpressionContext::PrimaryExpressionContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}


size_t ExpressionAntlrParser::PrimaryExpressionContext::getRuleIndex() const {
  return ExpressionAntlrParser::RulePrimaryExpression;
}

void ExpressionAntlrParser::PrimaryExpressionContext::copyFrom(PrimaryExpressionContext *ctx) {
  ParserRuleContext::copyFrom(ctx);
}

//----------------- FuncInvokeExpContext ------------------------------------------------------------------

ExpressionAntlrParser::PrimaryExpressionContext* ExpressionAntlrParser::FuncInvokeExpContext::primaryExpression() {
  return getRuleContext<ExpressionAntlrParser::PrimaryExpressionContext>(0);
}

tree::TerminalNode* ExpressionAntlrParser::FuncInvokeExpContext::OPEN_BRACKET() {
  return getToken(ExpressionAntlrParser::OPEN_BRACKET, 0);
}

tree::TerminalNode* ExpressionAntlrParser::FuncInvokeExpContext::CLOSE_BRACKET() {
  return getToken(ExpressionAntlrParser::CLOSE_BRACKET, 0);
}

tree::TerminalNode* ExpressionAntlrParser::FuncInvokeExpContext::NON() {
  return getToken(ExpressionAntlrParser::NON, 0);
}

ExpressionAntlrParser::ArgsListContext* ExpressionAntlrParser::FuncInvokeExpContext::argsList() {
  return getRuleContext<ExpressionAntlrParser::ArgsListContext>(0);
}

ExpressionAntlrParser::FuncInvokeExpContext::FuncInvokeExpContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::FuncInvokeExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitFuncInvokeExp(this);
  else
    return visitor->visitChildren(this);
}
//----------------- IdAtomContext ------------------------------------------------------------------

tree::TerminalNode* ExpressionAntlrParser::IdAtomContext::IDENTIFIER() {
  return getToken(ExpressionAntlrParser::IDENTIFIER, 0);
}

ExpressionAntlrParser::IdAtomContext::IdAtomContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::IdAtomContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitIdAtom(this);
  else
    return visitor->visitChildren(this);
}
//----------------- JsonCreationExpContext ------------------------------------------------------------------

tree::TerminalNode* ExpressionAntlrParser::JsonCreationExpContext::OPEN_CURLY_BRACKET() {
  return getToken(ExpressionAntlrParser::OPEN_CURLY_BRACKET, 0);
}

tree::TerminalNode* ExpressionAntlrParser::JsonCreationExpContext::CLOSE_CURLY_BRACKET() {
  return getToken(ExpressionAntlrParser::CLOSE_CURLY_BRACKET, 0);
}

ExpressionAntlrParser::KeyValuePairListContext* ExpressionAntlrParser::JsonCreationExpContext::keyValuePairList() {
  return getRuleContext<ExpressionAntlrParser::KeyValuePairListContext>(0);
}

ExpressionAntlrParser::JsonCreationExpContext::JsonCreationExpContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::JsonCreationExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitJsonCreationExp(this);
  else
    return visitor->visitChildren(this);
}
//----------------- StringAtomContext ------------------------------------------------------------------

tree::TerminalNode* ExpressionAntlrParser::StringAtomContext::STRING() {
  return getToken(ExpressionAntlrParser::STRING, 0);
}

ExpressionAntlrParser::StringAtomContext::StringAtomContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::StringAtomContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitStringAtom(this);
  else
    return visitor->visitChildren(this);
}
//----------------- IndexAccessExpContext ------------------------------------------------------------------

ExpressionAntlrParser::PrimaryExpressionContext* ExpressionAntlrParser::IndexAccessExpContext::primaryExpression() {
  return getRuleContext<ExpressionAntlrParser::PrimaryExpressionContext>(0);
}

tree::TerminalNode* ExpressionAntlrParser::IndexAccessExpContext::OPEN_SQUARE_BRACKET() {
  return getToken(ExpressionAntlrParser::OPEN_SQUARE_BRACKET, 0);
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::IndexAccessExpContext::expression() {
  return getRuleContext<ExpressionAntlrParser::ExpressionContext>(0);
}

tree::TerminalNode* ExpressionAntlrParser::IndexAccessExpContext::CLOSE_SQUARE_BRACKET() {
  return getToken(ExpressionAntlrParser::CLOSE_SQUARE_BRACKET, 0);
}

ExpressionAntlrParser::IndexAccessExpContext::IndexAccessExpContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::IndexAccessExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitIndexAccessExp(this);
  else
    return visitor->visitChildren(this);
}
//----------------- StringInterpolationAtomContext ------------------------------------------------------------------

ExpressionAntlrParser::StringInterpolationContext* ExpressionAntlrParser::StringInterpolationAtomContext::stringInterpolation() {
  return getRuleContext<ExpressionAntlrParser::StringInterpolationContext>(0);
}

ExpressionAntlrParser::StringInterpolationAtomContext::StringInterpolationAtomContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::StringInterpolationAtomContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitStringInterpolationAtom(this);
  else
    return visitor->visitChildren(this);
}
//----------------- MemberAccessExpContext ------------------------------------------------------------------

ExpressionAntlrParser::PrimaryExpressionContext* ExpressionAntlrParser::MemberAccessExpContext::primaryExpression() {
  return getRuleContext<ExpressionAntlrParser::PrimaryExpressionContext>(0);
}

tree::TerminalNode* ExpressionAntlrParser::MemberAccessExpContext::DOT() {
  return getToken(ExpressionAntlrParser::DOT, 0);
}

tree::TerminalNode* ExpressionAntlrParser::MemberAccessExpContext::IDENTIFIER() {
  return getToken(ExpressionAntlrParser::IDENTIFIER, 0);
}

ExpressionAntlrParser::MemberAccessExpContext::MemberAccessExpContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::MemberAccessExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitMemberAccessExp(this);
  else
    return visitor->visitChildren(this);
}
//----------------- ParenthesisExpContext ------------------------------------------------------------------

tree::TerminalNode* ExpressionAntlrParser::ParenthesisExpContext::OPEN_BRACKET() {
  return getToken(ExpressionAntlrParser::OPEN_BRACKET, 0);
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::ParenthesisExpContext::expression() {
  return getRuleContext<ExpressionAntlrParser::ExpressionContext>(0);
}

tree::TerminalNode* ExpressionAntlrParser::ParenthesisExpContext::CLOSE_BRACKET() {
  return getToken(ExpressionAntlrParser::CLOSE_BRACKET, 0);
}

ExpressionAntlrParser::ParenthesisExpContext::ParenthesisExpContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::ParenthesisExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitParenthesisExp(this);
  else
    return visitor->visitChildren(this);
}
//----------------- NumericAtomContext ------------------------------------------------------------------

tree::TerminalNode* ExpressionAntlrParser::NumericAtomContext::NUMBER() {
  return getToken(ExpressionAntlrParser::NUMBER, 0);
}

ExpressionAntlrParser::NumericAtomContext::NumericAtomContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::NumericAtomContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitNumericAtom(this);
  else
    return visitor->visitChildren(this);
}
//----------------- ArrayCreationExpContext ------------------------------------------------------------------

tree::TerminalNode* ExpressionAntlrParser::ArrayCreationExpContext::OPEN_SQUARE_BRACKET() {
  return getToken(ExpressionAntlrParser::OPEN_SQUARE_BRACKET, 0);
}

tree::TerminalNode* ExpressionAntlrParser::ArrayCreationExpContext::CLOSE_SQUARE_BRACKET() {
  return getToken(ExpressionAntlrParser::CLOSE_SQUARE_BRACKET, 0);
}

ExpressionAntlrParser::ArgsListContext* ExpressionAntlrParser::ArrayCreationExpContext::argsList() {
  return getRuleContext<ExpressionAntlrParser::ArgsListContext>(0);
}

ExpressionAntlrParser::ArrayCreationExpContext::ArrayCreationExpContext(PrimaryExpressionContext *ctx) { copyFrom(ctx); }


antlrcpp::Any ExpressionAntlrParser::ArrayCreationExpContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitArrayCreationExp(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::PrimaryExpressionContext* ExpressionAntlrParser::primaryExpression() {
   return primaryExpression(0);
}

ExpressionAntlrParser::PrimaryExpressionContext* ExpressionAntlrParser::primaryExpression(int precedence) {
  ParserRuleContext *parentContext = _ctx;
  size_t parentState = getState();
  ExpressionAntlrParser::PrimaryExpressionContext *_localctx = _tracker.createInstance<PrimaryExpressionContext>(_ctx, parentState);
  ExpressionAntlrParser::PrimaryExpressionContext *previousContext = _localctx;
  (void)previousContext; // Silence compiler, in case the context is not used by generated code.
  size_t startState = 4;
  enterRecursionRule(_localctx, 4, ExpressionAntlrParser::RulePrimaryExpression, precedence);

    size_t _la = 0;

  auto onExit = finally([=] {
    unrollRecursionContexts(parentContext);
  });
  try {
    size_t alt;
    enterOuterAlt(_localctx, 1);
    setState(77);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case ExpressionAntlrParser::OPEN_BRACKET: {
        _localctx = _tracker.createInstance<ParenthesisExpContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;

        setState(59);
        match(ExpressionAntlrParser::OPEN_BRACKET);
        setState(60);
        expression(0);
        setState(61);
        match(ExpressionAntlrParser::CLOSE_BRACKET);
        break;
      }

      case ExpressionAntlrParser::OPEN_SQUARE_BRACKET: {
        _localctx = _tracker.createInstance<ArrayCreationExpContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;
        setState(63);
        match(ExpressionAntlrParser::OPEN_SQUARE_BRACKET);
        setState(65);
        _errHandler->sync(this);

        _la = _input->LA(1);
        if ((((_la & ~ 0x3fULL) == 0) &&
          ((1ULL << _la) & ((1ULL << ExpressionAntlrParser::STRING_INTERPOLATION_START)
          | (1ULL << ExpressionAntlrParser::PLUS)
          | (1ULL << ExpressionAntlrParser::SUBSTRACT)
          | (1ULL << ExpressionAntlrParser::NON)
          | (1ULL << ExpressionAntlrParser::OPEN_BRACKET)
          | (1ULL << ExpressionAntlrParser::OPEN_SQUARE_BRACKET)
          | (1ULL << ExpressionAntlrParser::OPEN_CURLY_BRACKET)
          | (1ULL << ExpressionAntlrParser::NUMBER)
          | (1ULL << ExpressionAntlrParser::IDENTIFIER)
          | (1ULL << ExpressionAntlrParser::STRING))) != 0)) {
          setState(64);
          argsList();
        }
        setState(67);
        match(ExpressionAntlrParser::CLOSE_SQUARE_BRACKET);
        break;
      }

      case ExpressionAntlrParser::OPEN_CURLY_BRACKET: {
        _localctx = _tracker.createInstance<JsonCreationExpContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;
        setState(68);
        match(ExpressionAntlrParser::OPEN_CURLY_BRACKET);
        setState(70);
        _errHandler->sync(this);

        _la = _input->LA(1);
        if (_la == ExpressionAntlrParser::IDENTIFIER

        || _la == ExpressionAntlrParser::STRING) {
          setState(69);
          keyValuePairList();
        }
        setState(72);
        match(ExpressionAntlrParser::CLOSE_CURLY_BRACKET);
        break;
      }

      case ExpressionAntlrParser::NUMBER: {
        _localctx = _tracker.createInstance<NumericAtomContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;
        setState(73);
        match(ExpressionAntlrParser::NUMBER);
        break;
      }

      case ExpressionAntlrParser::STRING: {
        _localctx = _tracker.createInstance<StringAtomContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;
        setState(74);
        match(ExpressionAntlrParser::STRING);
        break;
      }

      case ExpressionAntlrParser::IDENTIFIER: {
        _localctx = _tracker.createInstance<IdAtomContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;
        setState(75);
        match(ExpressionAntlrParser::IDENTIFIER);
        break;
      }

      case ExpressionAntlrParser::STRING_INTERPOLATION_START: {
        _localctx = _tracker.createInstance<StringInterpolationAtomContext>(_localctx);
        _ctx = _localctx;
        previousContext = _localctx;
        setState(76);
        stringInterpolation();
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    _ctx->stop = _input->LT(-1);
    setState(98);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 9, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        setState(96);
        _errHandler->sync(this);
        switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 8, _ctx)) {
        case 1: {
          auto newContext = _tracker.createInstance<MemberAccessExpContext>(_tracker.createInstance<PrimaryExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RulePrimaryExpression);
          setState(79);

          if (!(precpred(_ctx, 3))) throw FailedPredicateException(this, "precpred(_ctx, 3)");
          setState(80);
          match(ExpressionAntlrParser::DOT);
          setState(81);
          match(ExpressionAntlrParser::IDENTIFIER);
          break;
        }

        case 2: {
          auto newContext = _tracker.createInstance<FuncInvokeExpContext>(_tracker.createInstance<PrimaryExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RulePrimaryExpression);
          setState(82);

          if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
          setState(84);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == ExpressionAntlrParser::NON) {
            setState(83);
            match(ExpressionAntlrParser::NON);
          }
          setState(86);
          match(ExpressionAntlrParser::OPEN_BRACKET);
          setState(88);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if ((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << ExpressionAntlrParser::STRING_INTERPOLATION_START)
            | (1ULL << ExpressionAntlrParser::PLUS)
            | (1ULL << ExpressionAntlrParser::SUBSTRACT)
            | (1ULL << ExpressionAntlrParser::NON)
            | (1ULL << ExpressionAntlrParser::OPEN_BRACKET)
            | (1ULL << ExpressionAntlrParser::OPEN_SQUARE_BRACKET)
            | (1ULL << ExpressionAntlrParser::OPEN_CURLY_BRACKET)
            | (1ULL << ExpressionAntlrParser::NUMBER)
            | (1ULL << ExpressionAntlrParser::IDENTIFIER)
            | (1ULL << ExpressionAntlrParser::STRING))) != 0)) {
            setState(87);
            argsList();
          }
          setState(90);
          match(ExpressionAntlrParser::CLOSE_BRACKET);
          break;
        }

        case 3: {
          auto newContext = _tracker.createInstance<IndexAccessExpContext>(_tracker.createInstance<PrimaryExpressionContext>(parentContext, parentState));
          _localctx = newContext;
          pushNewRecursionContext(newContext, startState, RulePrimaryExpression);
          setState(91);

          if (!(precpred(_ctx, 1))) throw FailedPredicateException(this, "precpred(_ctx, 1)");
          setState(92);
          match(ExpressionAntlrParser::OPEN_SQUARE_BRACKET);
          setState(93);
          expression(0);
          setState(94);
          match(ExpressionAntlrParser::CLOSE_SQUARE_BRACKET);
          break;
        }

        } 
      }
      setState(100);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 9, _ctx);
    }
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }
  return _localctx;
}

//----------------- StringInterpolationContext ------------------------------------------------------------------

ExpressionAntlrParser::StringInterpolationContext::StringInterpolationContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

std::vector<tree::TerminalNode *> ExpressionAntlrParser::StringInterpolationContext::STRING_INTERPOLATION_START() {
  return getTokens(ExpressionAntlrParser::STRING_INTERPOLATION_START);
}

tree::TerminalNode* ExpressionAntlrParser::StringInterpolationContext::STRING_INTERPOLATION_START(size_t i) {
  return getToken(ExpressionAntlrParser::STRING_INTERPOLATION_START, i);
}

std::vector<tree::TerminalNode *> ExpressionAntlrParser::StringInterpolationContext::ESCAPE_CHARACTER() {
  return getTokens(ExpressionAntlrParser::ESCAPE_CHARACTER);
}

tree::TerminalNode* ExpressionAntlrParser::StringInterpolationContext::ESCAPE_CHARACTER(size_t i) {
  return getToken(ExpressionAntlrParser::ESCAPE_CHARACTER, i);
}

std::vector<tree::TerminalNode *> ExpressionAntlrParser::StringInterpolationContext::TEMPLATE() {
  return getTokens(ExpressionAntlrParser::TEMPLATE);
}

tree::TerminalNode* ExpressionAntlrParser::StringInterpolationContext::TEMPLATE(size_t i) {
  return getToken(ExpressionAntlrParser::TEMPLATE, i);
}

std::vector<ExpressionAntlrParser::TextContentContext *> ExpressionAntlrParser::StringInterpolationContext::textContent() {
  return getRuleContexts<ExpressionAntlrParser::TextContentContext>();
}

ExpressionAntlrParser::TextContentContext* ExpressionAntlrParser::StringInterpolationContext::textContent(size_t i) {
  return getRuleContext<ExpressionAntlrParser::TextContentContext>(i);
}


size_t ExpressionAntlrParser::StringInterpolationContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleStringInterpolation;
}


antlrcpp::Any ExpressionAntlrParser::StringInterpolationContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitStringInterpolation(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::StringInterpolationContext* ExpressionAntlrParser::stringInterpolation() {
  StringInterpolationContext *_localctx = _tracker.createInstance<StringInterpolationContext>(_ctx, getState());
  enterRule(_localctx, 6, ExpressionAntlrParser::RuleStringInterpolation);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(101);
    match(ExpressionAntlrParser::STRING_INTERPOLATION_START);
    setState(107);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while ((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << ExpressionAntlrParser::TEMPLATE)
      | (1ULL << ExpressionAntlrParser::ESCAPE_CHARACTER)
      | (1ULL << ExpressionAntlrParser::TEXT_CONTENT))) != 0)) {
      setState(105);
      _errHandler->sync(this);
      switch (_input->LA(1)) {
        case ExpressionAntlrParser::ESCAPE_CHARACTER: {
          setState(102);
          match(ExpressionAntlrParser::ESCAPE_CHARACTER);
          break;
        }

        case ExpressionAntlrParser::TEMPLATE: {
          setState(103);
          match(ExpressionAntlrParser::TEMPLATE);
          break;
        }

        case ExpressionAntlrParser::TEXT_CONTENT: {
          setState(104);
          textContent();
          break;
        }

      default:
        throw NoViableAltException(this);
      }
      setState(109);
      _errHandler->sync(this);
      _la = _input->LA(1);
    }
    setState(110);
    match(ExpressionAntlrParser::STRING_INTERPOLATION_START);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- TextContentContext ------------------------------------------------------------------

ExpressionAntlrParser::TextContentContext::TextContentContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

std::vector<tree::TerminalNode *> ExpressionAntlrParser::TextContentContext::TEXT_CONTENT() {
  return getTokens(ExpressionAntlrParser::TEXT_CONTENT);
}

tree::TerminalNode* ExpressionAntlrParser::TextContentContext::TEXT_CONTENT(size_t i) {
  return getToken(ExpressionAntlrParser::TEXT_CONTENT, i);
}


size_t ExpressionAntlrParser::TextContentContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleTextContent;
}


antlrcpp::Any ExpressionAntlrParser::TextContentContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitTextContent(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::TextContentContext* ExpressionAntlrParser::textContent() {
  TextContentContext *_localctx = _tracker.createInstance<TextContentContext>(_ctx, getState());
  enterRule(_localctx, 8, ExpressionAntlrParser::RuleTextContent);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    size_t alt;
    enterOuterAlt(_localctx, 1);
    setState(113); 
    _errHandler->sync(this);
    alt = 1;
    do {
      switch (alt) {
        case 1: {
              setState(112);
              match(ExpressionAntlrParser::TEXT_CONTENT);
              break;
            }

      default:
        throw NoViableAltException(this);
      }
      setState(115); 
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 12, _ctx);
    } while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- ArgsListContext ------------------------------------------------------------------

ExpressionAntlrParser::ArgsListContext::ArgsListContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

std::vector<ExpressionAntlrParser::LambdaContext *> ExpressionAntlrParser::ArgsListContext::lambda() {
  return getRuleContexts<ExpressionAntlrParser::LambdaContext>();
}

ExpressionAntlrParser::LambdaContext* ExpressionAntlrParser::ArgsListContext::lambda(size_t i) {
  return getRuleContext<ExpressionAntlrParser::LambdaContext>(i);
}

std::vector<ExpressionAntlrParser::ExpressionContext *> ExpressionAntlrParser::ArgsListContext::expression() {
  return getRuleContexts<ExpressionAntlrParser::ExpressionContext>();
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::ArgsListContext::expression(size_t i) {
  return getRuleContext<ExpressionAntlrParser::ExpressionContext>(i);
}

std::vector<tree::TerminalNode *> ExpressionAntlrParser::ArgsListContext::COMMA() {
  return getTokens(ExpressionAntlrParser::COMMA);
}

tree::TerminalNode* ExpressionAntlrParser::ArgsListContext::COMMA(size_t i) {
  return getToken(ExpressionAntlrParser::COMMA, i);
}


size_t ExpressionAntlrParser::ArgsListContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleArgsList;
}


antlrcpp::Any ExpressionAntlrParser::ArgsListContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitArgsList(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::ArgsListContext* ExpressionAntlrParser::argsList() {
  ArgsListContext *_localctx = _tracker.createInstance<ArgsListContext>(_ctx, getState());
  enterRule(_localctx, 10, ExpressionAntlrParser::RuleArgsList);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(119);
    _errHandler->sync(this);
    switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 13, _ctx)) {
    case 1: {
      setState(117);
      lambda();
      break;
    }

    case 2: {
      setState(118);
      expression(0);
      break;
    }

    }
    setState(128);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == ExpressionAntlrParser::COMMA) {
      setState(121);
      match(ExpressionAntlrParser::COMMA);
      setState(124);
      _errHandler->sync(this);
      switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 14, _ctx)) {
      case 1: {
        setState(122);
        lambda();
        break;
      }

      case 2: {
        setState(123);
        expression(0);
        break;
      }

      }
      setState(130);
      _errHandler->sync(this);
      _la = _input->LA(1);
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- LambdaContext ------------------------------------------------------------------

ExpressionAntlrParser::LambdaContext::LambdaContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* ExpressionAntlrParser::LambdaContext::IDENTIFIER() {
  return getToken(ExpressionAntlrParser::IDENTIFIER, 0);
}

tree::TerminalNode* ExpressionAntlrParser::LambdaContext::ARROW() {
  return getToken(ExpressionAntlrParser::ARROW, 0);
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::LambdaContext::expression() {
  return getRuleContext<ExpressionAntlrParser::ExpressionContext>(0);
}


size_t ExpressionAntlrParser::LambdaContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleLambda;
}


antlrcpp::Any ExpressionAntlrParser::LambdaContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitLambda(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::LambdaContext* ExpressionAntlrParser::lambda() {
  LambdaContext *_localctx = _tracker.createInstance<LambdaContext>(_ctx, getState());
  enterRule(_localctx, 12, ExpressionAntlrParser::RuleLambda);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(131);
    match(ExpressionAntlrParser::IDENTIFIER);
    setState(132);
    match(ExpressionAntlrParser::ARROW);
    setState(133);
    expression(0);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- KeyValuePairListContext ------------------------------------------------------------------

ExpressionAntlrParser::KeyValuePairListContext::KeyValuePairListContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

std::vector<ExpressionAntlrParser::KeyValuePairContext *> ExpressionAntlrParser::KeyValuePairListContext::keyValuePair() {
  return getRuleContexts<ExpressionAntlrParser::KeyValuePairContext>();
}

ExpressionAntlrParser::KeyValuePairContext* ExpressionAntlrParser::KeyValuePairListContext::keyValuePair(size_t i) {
  return getRuleContext<ExpressionAntlrParser::KeyValuePairContext>(i);
}

std::vector<tree::TerminalNode *> ExpressionAntlrParser::KeyValuePairListContext::COMMA() {
  return getTokens(ExpressionAntlrParser::COMMA);
}

tree::TerminalNode* ExpressionAntlrParser::KeyValuePairListContext::COMMA(size_t i) {
  return getToken(ExpressionAntlrParser::COMMA, i);
}


size_t ExpressionAntlrParser::KeyValuePairListContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleKeyValuePairList;
}


antlrcpp::Any ExpressionAntlrParser::KeyValuePairListContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitKeyValuePairList(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::KeyValuePairListContext* ExpressionAntlrParser::keyValuePairList() {
  KeyValuePairListContext *_localctx = _tracker.createInstance<KeyValuePairListContext>(_ctx, getState());
  enterRule(_localctx, 14, ExpressionAntlrParser::RuleKeyValuePairList);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(135);
    keyValuePair();
    setState(140);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == ExpressionAntlrParser::COMMA) {
      setState(136);
      match(ExpressionAntlrParser::COMMA);
      setState(137);
      keyValuePair();
      setState(142);
      _errHandler->sync(this);
      _la = _input->LA(1);
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- KeyValuePairContext ------------------------------------------------------------------

ExpressionAntlrParser::KeyValuePairContext::KeyValuePairContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

ExpressionAntlrParser::KeyContext* ExpressionAntlrParser::KeyValuePairContext::key() {
  return getRuleContext<ExpressionAntlrParser::KeyContext>(0);
}

tree::TerminalNode* ExpressionAntlrParser::KeyValuePairContext::COLON() {
  return getToken(ExpressionAntlrParser::COLON, 0);
}

ExpressionAntlrParser::ExpressionContext* ExpressionAntlrParser::KeyValuePairContext::expression() {
  return getRuleContext<ExpressionAntlrParser::ExpressionContext>(0);
}


size_t ExpressionAntlrParser::KeyValuePairContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleKeyValuePair;
}


antlrcpp::Any ExpressionAntlrParser::KeyValuePairContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitKeyValuePair(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::KeyValuePairContext* ExpressionAntlrParser::keyValuePair() {
  KeyValuePairContext *_localctx = _tracker.createInstance<KeyValuePairContext>(_ctx, getState());
  enterRule(_localctx, 16, ExpressionAntlrParser::RuleKeyValuePair);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(143);
    key();
    setState(144);
    match(ExpressionAntlrParser::COLON);
    setState(145);
    expression(0);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- KeyContext ------------------------------------------------------------------

ExpressionAntlrParser::KeyContext::KeyContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* ExpressionAntlrParser::KeyContext::IDENTIFIER() {
  return getToken(ExpressionAntlrParser::IDENTIFIER, 0);
}

tree::TerminalNode* ExpressionAntlrParser::KeyContext::STRING() {
  return getToken(ExpressionAntlrParser::STRING, 0);
}


size_t ExpressionAntlrParser::KeyContext::getRuleIndex() const {
  return ExpressionAntlrParser::RuleKey;
}


antlrcpp::Any ExpressionAntlrParser::KeyContext::accept(tree::ParseTreeVisitor *visitor) {
  if (auto parserVisitor = dynamic_cast<ExpressionAntlrParserVisitor*>(visitor))
    return parserVisitor->visitKey(this);
  else
    return visitor->visitChildren(this);
}

ExpressionAntlrParser::KeyContext* ExpressionAntlrParser::key() {
  KeyContext *_localctx = _tracker.createInstance<KeyContext>(_ctx, getState());
  enterRule(_localctx, 18, ExpressionAntlrParser::RuleKey);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(147);
    _la = _input->LA(1);
    if (!(_la == ExpressionAntlrParser::IDENTIFIER

    || _la == ExpressionAntlrParser::STRING)) {
    _errHandler->recoverInline(this);
    }
    else {
      _errHandler->reportMatch(this);
      consume();
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

bool ExpressionAntlrParser::sempred(RuleContext *context, size_t ruleIndex, size_t predicateIndex) {
  switch (ruleIndex) {
    case 1: return expressionSempred(dynamic_cast<ExpressionContext *>(context), predicateIndex);
    case 2: return primaryExpressionSempred(dynamic_cast<PrimaryExpressionContext *>(context), predicateIndex);

  default:
    break;
  }
  return true;
}

bool ExpressionAntlrParser::expressionSempred(ExpressionContext *_localctx, size_t predicateIndex) {
  switch (predicateIndex) {
    case 0: return precpred(_ctx, 9);
    case 1: return precpred(_ctx, 8);
    case 2: return precpred(_ctx, 7);
    case 3: return precpred(_ctx, 6);
    case 4: return precpred(_ctx, 5);
    case 5: return precpred(_ctx, 4);
    case 6: return precpred(_ctx, 3);
    case 7: return precpred(_ctx, 2);

  default:
    break;
  }
  return true;
}

bool ExpressionAntlrParser::primaryExpressionSempred(PrimaryExpressionContext *_localctx, size_t predicateIndex) {
  switch (predicateIndex) {
    case 8: return precpred(_ctx, 3);
    case 9: return precpred(_ctx, 2);
    case 10: return precpred(_ctx, 1);

  default:
    break;
  }
  return true;
}

// Static vars and initialization.
std::vector<dfa::DFA> ExpressionAntlrParser::_decisionToDFA;
atn::PredictionContextCache ExpressionAntlrParser::_sharedContextCache;

// We own the ATN which in turn owns the ATN states.
atn::ATN ExpressionAntlrParser::_atn;
std::vector<uint16_t> ExpressionAntlrParser::_serializedATN;

std::vector<std::string> ExpressionAntlrParser::_ruleNames = {
  "file", "expression", "primaryExpression", "stringInterpolation", "textContent", 
  "argsList", "lambda", "keyValuePairList", "keyValuePair", "key"
};

std::vector<std::string> ExpressionAntlrParser::_literalNames = {
  "", "", "'+'", "'-'", "'!'", "'^'", "'*'", "'/'", "'%'", "'=='", "", "'&'", 
  "'&&'", "'||'", "'<'", "'>'", "'<='", "'>='", "'('", "')'", "'.'", "'['", 
  "']'", "'{'", "'}'", "','", "':'", "'=>'"
};

std::vector<std::string> ExpressionAntlrParser::_symbolicNames = {
  "", "STRING_INTERPOLATION_START", "PLUS", "SUBSTRACT", "NON", "XOR", "ASTERISK", 
  "SLASH", "PERCENT", "DOUBLE_EQUAL", "NOT_EQUAL", "SINGLE_AND", "DOUBLE_AND", 
  "DOUBLE_VERTICAL_CYLINDER", "LESS_THAN", "MORE_THAN", "LESS_OR_EQUAl", 
  "MORE_OR_EQUAL", "OPEN_BRACKET", "CLOSE_BRACKET", "DOT", "OPEN_SQUARE_BRACKET", 
  "CLOSE_SQUARE_BRACKET", "OPEN_CURLY_BRACKET", "CLOSE_CURLY_BRACKET", "COMMA", 
  "COLON", "ARROW", "NUMBER", "WHITESPACE", "IDENTIFIER", "NEWLINE", "STRING", 
  "INVALID_TOKEN_DEFAULT_MODE", "TEMPLATE", "ESCAPE_CHARACTER", "TEXT_CONTENT"
};

dfa::Vocabulary ExpressionAntlrParser::_vocabulary(_literalNames, _symbolicNames);

std::vector<std::string> ExpressionAntlrParser::_tokenNames;

ExpressionAntlrParser::Initializer::Initializer() {
	for (size_t i = 0; i < _symbolicNames.size(); ++i) {
		std::string name = _vocabulary.getLiteralName(i);
		if (name.empty()) {
			name = _vocabulary.getSymbolicName(i);
		}

		if (name.empty()) {
			_tokenNames.push_back("<INVALID>");
		} else {
      _tokenNames.push_back(name);
    }
	}

  _serializedATN = {
    0x3, 0x608b, 0xa72a, 0x8133, 0xb9ed, 0x417c, 0x3be7, 0x7786, 0x5964, 
    0x3, 0x26, 0x98, 0x4, 0x2, 0x9, 0x2, 0x4, 0x3, 0x9, 0x3, 0x4, 0x4, 0x9, 
    0x4, 0x4, 0x5, 0x9, 0x5, 0x4, 0x6, 0x9, 0x6, 0x4, 0x7, 0x9, 0x7, 0x4, 
    0x8, 0x9, 0x8, 0x4, 0x9, 0x9, 0x9, 0x4, 0xa, 0x9, 0xa, 0x4, 0xb, 0x9, 
    0xb, 0x3, 0x2, 0x3, 0x2, 0x3, 0x2, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 
    0x3, 0x5, 0x3, 0x1e, 0xa, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 
    0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 
    0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 
    0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x7, 0x3, 
    0x38, 0xa, 0x3, 0xc, 0x3, 0xe, 0x3, 0x3b, 0xb, 0x3, 0x3, 0x4, 0x3, 0x4, 
    0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x5, 0x4, 0x44, 0xa, 
    0x4, 0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x5, 0x4, 0x49, 0xa, 0x4, 0x3, 0x4, 
    0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x5, 0x4, 0x50, 0xa, 0x4, 0x3, 
    0x4, 0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x5, 0x4, 0x57, 0xa, 0x4, 
    0x3, 0x4, 0x3, 0x4, 0x5, 0x4, 0x5b, 0xa, 0x4, 0x3, 0x4, 0x3, 0x4, 0x3, 
    0x4, 0x3, 0x4, 0x3, 0x4, 0x3, 0x4, 0x7, 0x4, 0x63, 0xa, 0x4, 0xc, 0x4, 
    0xe, 0x4, 0x66, 0xb, 0x4, 0x3, 0x5, 0x3, 0x5, 0x3, 0x5, 0x3, 0x5, 0x7, 
    0x5, 0x6c, 0xa, 0x5, 0xc, 0x5, 0xe, 0x5, 0x6f, 0xb, 0x5, 0x3, 0x5, 0x3, 
    0x5, 0x3, 0x6, 0x6, 0x6, 0x74, 0xa, 0x6, 0xd, 0x6, 0xe, 0x6, 0x75, 0x3, 
    0x7, 0x3, 0x7, 0x5, 0x7, 0x7a, 0xa, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 
    0x5, 0x7, 0x7f, 0xa, 0x7, 0x7, 0x7, 0x81, 0xa, 0x7, 0xc, 0x7, 0xe, 0x7, 
    0x84, 0xb, 0x7, 0x3, 0x8, 0x3, 0x8, 0x3, 0x8, 0x3, 0x8, 0x3, 0x9, 0x3, 
    0x9, 0x3, 0x9, 0x7, 0x9, 0x8d, 0xa, 0x9, 0xc, 0x9, 0xe, 0x9, 0x90, 0xb, 
    0x9, 0x3, 0xa, 0x3, 0xa, 0x3, 0xa, 0x3, 0xa, 0x3, 0xb, 0x3, 0xb, 0x3, 
    0xb, 0x2, 0x4, 0x4, 0x6, 0xc, 0x2, 0x4, 0x6, 0x8, 0xa, 0xc, 0xe, 0x10, 
    0x12, 0x14, 0x2, 0x8, 0x3, 0x2, 0x4, 0x6, 0x3, 0x2, 0x8, 0xa, 0x3, 0x2, 
    0x4, 0x5, 0x3, 0x2, 0xb, 0xc, 0x3, 0x2, 0x10, 0x13, 0x4, 0x2, 0x20, 
    0x20, 0x22, 0x22, 0x2, 0xab, 0x2, 0x16, 0x3, 0x2, 0x2, 0x2, 0x4, 0x1d, 
    0x3, 0x2, 0x2, 0x2, 0x6, 0x4f, 0x3, 0x2, 0x2, 0x2, 0x8, 0x67, 0x3, 0x2, 
    0x2, 0x2, 0xa, 0x73, 0x3, 0x2, 0x2, 0x2, 0xc, 0x79, 0x3, 0x2, 0x2, 0x2, 
    0xe, 0x85, 0x3, 0x2, 0x2, 0x2, 0x10, 0x89, 0x3, 0x2, 0x2, 0x2, 0x12, 
    0x91, 0x3, 0x2, 0x2, 0x2, 0x14, 0x95, 0x3, 0x2, 0x2, 0x2, 0x16, 0x17, 
    0x5, 0x4, 0x3, 0x2, 0x17, 0x18, 0x7, 0x2, 0x2, 0x3, 0x18, 0x3, 0x3, 
    0x2, 0x2, 0x2, 0x19, 0x1a, 0x8, 0x3, 0x1, 0x2, 0x1a, 0x1b, 0x9, 0x2, 
    0x2, 0x2, 0x1b, 0x1e, 0x5, 0x4, 0x3, 0xc, 0x1c, 0x1e, 0x5, 0x6, 0x4, 
    0x2, 0x1d, 0x19, 0x3, 0x2, 0x2, 0x2, 0x1d, 0x1c, 0x3, 0x2, 0x2, 0x2, 
    0x1e, 0x39, 0x3, 0x2, 0x2, 0x2, 0x1f, 0x20, 0xc, 0xb, 0x2, 0x2, 0x20, 
    0x21, 0x7, 0x7, 0x2, 0x2, 0x21, 0x38, 0x5, 0x4, 0x3, 0xb, 0x22, 0x23, 
    0xc, 0xa, 0x2, 0x2, 0x23, 0x24, 0x9, 0x3, 0x2, 0x2, 0x24, 0x38, 0x5, 
    0x4, 0x3, 0xb, 0x25, 0x26, 0xc, 0x9, 0x2, 0x2, 0x26, 0x27, 0x9, 0x4, 
    0x2, 0x2, 0x27, 0x38, 0x5, 0x4, 0x3, 0xa, 0x28, 0x29, 0xc, 0x8, 0x2, 
    0x2, 0x29, 0x2a, 0x9, 0x5, 0x2, 0x2, 0x2a, 0x38, 0x5, 0x4, 0x3, 0x9, 
    0x2b, 0x2c, 0xc, 0x7, 0x2, 0x2, 0x2c, 0x2d, 0x7, 0xd, 0x2, 0x2, 0x2d, 
    0x38, 0x5, 0x4, 0x3, 0x8, 0x2e, 0x2f, 0xc, 0x6, 0x2, 0x2, 0x2f, 0x30, 
    0x9, 0x6, 0x2, 0x2, 0x30, 0x38, 0x5, 0x4, 0x3, 0x7, 0x31, 0x32, 0xc, 
    0x5, 0x2, 0x2, 0x32, 0x33, 0x7, 0xe, 0x2, 0x2, 0x33, 0x38, 0x5, 0x4, 
    0x3, 0x6, 0x34, 0x35, 0xc, 0x4, 0x2, 0x2, 0x35, 0x36, 0x7, 0xf, 0x2, 
    0x2, 0x36, 0x38, 0x5, 0x4, 0x3, 0x5, 0x37, 0x1f, 0x3, 0x2, 0x2, 0x2, 
    0x37, 0x22, 0x3, 0x2, 0x2, 0x2, 0x37, 0x25, 0x3, 0x2, 0x2, 0x2, 0x37, 
    0x28, 0x3, 0x2, 0x2, 0x2, 0x37, 0x2b, 0x3, 0x2, 0x2, 0x2, 0x37, 0x2e, 
    0x3, 0x2, 0x2, 0x2, 0x37, 0x31, 0x3, 0x2, 0x2, 0x2, 0x37, 0x34, 0x3, 
    0x2, 0x2, 0x2, 0x38, 0x3b, 0x3, 0x2, 0x2, 0x2, 0x39, 0x37, 0x3, 0x2, 
    0x2, 0x2, 0x39, 0x3a, 0x3, 0x2, 0x2, 0x2, 0x3a, 0x5, 0x3, 0x2, 0x2, 
    0x2, 0x3b, 0x39, 0x3, 0x2, 0x2, 0x2, 0x3c, 0x3d, 0x8, 0x4, 0x1, 0x2, 
    0x3d, 0x3e, 0x7, 0x14, 0x2, 0x2, 0x3e, 0x3f, 0x5, 0x4, 0x3, 0x2, 0x3f, 
    0x40, 0x7, 0x15, 0x2, 0x2, 0x40, 0x50, 0x3, 0x2, 0x2, 0x2, 0x41, 0x43, 
    0x7, 0x17, 0x2, 0x2, 0x42, 0x44, 0x5, 0xc, 0x7, 0x2, 0x43, 0x42, 0x3, 
    0x2, 0x2, 0x2, 0x43, 0x44, 0x3, 0x2, 0x2, 0x2, 0x44, 0x45, 0x3, 0x2, 
    0x2, 0x2, 0x45, 0x50, 0x7, 0x18, 0x2, 0x2, 0x46, 0x48, 0x7, 0x19, 0x2, 
    0x2, 0x47, 0x49, 0x5, 0x10, 0x9, 0x2, 0x48, 0x47, 0x3, 0x2, 0x2, 0x2, 
    0x48, 0x49, 0x3, 0x2, 0x2, 0x2, 0x49, 0x4a, 0x3, 0x2, 0x2, 0x2, 0x4a, 
    0x50, 0x7, 0x1a, 0x2, 0x2, 0x4b, 0x50, 0x7, 0x1e, 0x2, 0x2, 0x4c, 0x50, 
    0x7, 0x22, 0x2, 0x2, 0x4d, 0x50, 0x7, 0x20, 0x2, 0x2, 0x4e, 0x50, 0x5, 
    0x8, 0x5, 0x2, 0x4f, 0x3c, 0x3, 0x2, 0x2, 0x2, 0x4f, 0x41, 0x3, 0x2, 
    0x2, 0x2, 0x4f, 0x46, 0x3, 0x2, 0x2, 0x2, 0x4f, 0x4b, 0x3, 0x2, 0x2, 
    0x2, 0x4f, 0x4c, 0x3, 0x2, 0x2, 0x2, 0x4f, 0x4d, 0x3, 0x2, 0x2, 0x2, 
    0x4f, 0x4e, 0x3, 0x2, 0x2, 0x2, 0x50, 0x64, 0x3, 0x2, 0x2, 0x2, 0x51, 
    0x52, 0xc, 0x5, 0x2, 0x2, 0x52, 0x53, 0x7, 0x16, 0x2, 0x2, 0x53, 0x63, 
    0x7, 0x20, 0x2, 0x2, 0x54, 0x56, 0xc, 0x4, 0x2, 0x2, 0x55, 0x57, 0x7, 
    0x6, 0x2, 0x2, 0x56, 0x55, 0x3, 0x2, 0x2, 0x2, 0x56, 0x57, 0x3, 0x2, 
    0x2, 0x2, 0x57, 0x58, 0x3, 0x2, 0x2, 0x2, 0x58, 0x5a, 0x7, 0x14, 0x2, 
    0x2, 0x59, 0x5b, 0x5, 0xc, 0x7, 0x2, 0x5a, 0x59, 0x3, 0x2, 0x2, 0x2, 
    0x5a, 0x5b, 0x3, 0x2, 0x2, 0x2, 0x5b, 0x5c, 0x3, 0x2, 0x2, 0x2, 0x5c, 
    0x63, 0x7, 0x15, 0x2, 0x2, 0x5d, 0x5e, 0xc, 0x3, 0x2, 0x2, 0x5e, 0x5f, 
    0x7, 0x17, 0x2, 0x2, 0x5f, 0x60, 0x5, 0x4, 0x3, 0x2, 0x60, 0x61, 0x7, 
    0x18, 0x2, 0x2, 0x61, 0x63, 0x3, 0x2, 0x2, 0x2, 0x62, 0x51, 0x3, 0x2, 
    0x2, 0x2, 0x62, 0x54, 0x3, 0x2, 0x2, 0x2, 0x62, 0x5d, 0x3, 0x2, 0x2, 
    0x2, 0x63, 0x66, 0x3, 0x2, 0x2, 0x2, 0x64, 0x62, 0x3, 0x2, 0x2, 0x2, 
    0x64, 0x65, 0x3, 0x2, 0x2, 0x2, 0x65, 0x7, 0x3, 0x2, 0x2, 0x2, 0x66, 
    0x64, 0x3, 0x2, 0x2, 0x2, 0x67, 0x6d, 0x7, 0x3, 0x2, 0x2, 0x68, 0x6c, 
    0x7, 0x25, 0x2, 0x2, 0x69, 0x6c, 0x7, 0x24, 0x2, 0x2, 0x6a, 0x6c, 0x5, 
    0xa, 0x6, 0x2, 0x6b, 0x68, 0x3, 0x2, 0x2, 0x2, 0x6b, 0x69, 0x3, 0x2, 
    0x2, 0x2, 0x6b, 0x6a, 0x3, 0x2, 0x2, 0x2, 0x6c, 0x6f, 0x3, 0x2, 0x2, 
    0x2, 0x6d, 0x6b, 0x3, 0x2, 0x2, 0x2, 0x6d, 0x6e, 0x3, 0x2, 0x2, 0x2, 
    0x6e, 0x70, 0x3, 0x2, 0x2, 0x2, 0x6f, 0x6d, 0x3, 0x2, 0x2, 0x2, 0x70, 
    0x71, 0x7, 0x3, 0x2, 0x2, 0x71, 0x9, 0x3, 0x2, 0x2, 0x2, 0x72, 0x74, 
    0x7, 0x26, 0x2, 0x2, 0x73, 0x72, 0x3, 0x2, 0x2, 0x2, 0x74, 0x75, 0x3, 
    0x2, 0x2, 0x2, 0x75, 0x73, 0x3, 0x2, 0x2, 0x2, 0x75, 0x76, 0x3, 0x2, 
    0x2, 0x2, 0x76, 0xb, 0x3, 0x2, 0x2, 0x2, 0x77, 0x7a, 0x5, 0xe, 0x8, 
    0x2, 0x78, 0x7a, 0x5, 0x4, 0x3, 0x2, 0x79, 0x77, 0x3, 0x2, 0x2, 0x2, 
    0x79, 0x78, 0x3, 0x2, 0x2, 0x2, 0x7a, 0x82, 0x3, 0x2, 0x2, 0x2, 0x7b, 
    0x7e, 0x7, 0x1b, 0x2, 0x2, 0x7c, 0x7f, 0x5, 0xe, 0x8, 0x2, 0x7d, 0x7f, 
    0x5, 0x4, 0x3, 0x2, 0x7e, 0x7c, 0x3, 0x2, 0x2, 0x2, 0x7e, 0x7d, 0x3, 
    0x2, 0x2, 0x2, 0x7f, 0x81, 0x3, 0x2, 0x2, 0x2, 0x80, 0x7b, 0x3, 0x2, 
    0x2, 0x2, 0x81, 0x84, 0x3, 0x2, 0x2, 0x2, 0x82, 0x80, 0x3, 0x2, 0x2, 
    0x2, 0x82, 0x83, 0x3, 0x2, 0x2, 0x2, 0x83, 0xd, 0x3, 0x2, 0x2, 0x2, 
    0x84, 0x82, 0x3, 0x2, 0x2, 0x2, 0x85, 0x86, 0x7, 0x20, 0x2, 0x2, 0x86, 
    0x87, 0x7, 0x1d, 0x2, 0x2, 0x87, 0x88, 0x5, 0x4, 0x3, 0x2, 0x88, 0xf, 
    0x3, 0x2, 0x2, 0x2, 0x89, 0x8e, 0x5, 0x12, 0xa, 0x2, 0x8a, 0x8b, 0x7, 
    0x1b, 0x2, 0x2, 0x8b, 0x8d, 0x5, 0x12, 0xa, 0x2, 0x8c, 0x8a, 0x3, 0x2, 
    0x2, 0x2, 0x8d, 0x90, 0x3, 0x2, 0x2, 0x2, 0x8e, 0x8c, 0x3, 0x2, 0x2, 
    0x2, 0x8e, 0x8f, 0x3, 0x2, 0x2, 0x2, 0x8f, 0x11, 0x3, 0x2, 0x2, 0x2, 
    0x90, 0x8e, 0x3, 0x2, 0x2, 0x2, 0x91, 0x92, 0x5, 0x14, 0xb, 0x2, 0x92, 
    0x93, 0x7, 0x1c, 0x2, 0x2, 0x93, 0x94, 0x5, 0x4, 0x3, 0x2, 0x94, 0x13, 
    0x3, 0x2, 0x2, 0x2, 0x95, 0x96, 0x9, 0x7, 0x2, 0x2, 0x96, 0x15, 0x3, 
    0x2, 0x2, 0x2, 0x13, 0x1d, 0x37, 0x39, 0x43, 0x48, 0x4f, 0x56, 0x5a, 
    0x62, 0x64, 0x6b, 0x6d, 0x75, 0x79, 0x7e, 0x82, 0x8e, 
  };

  atn::ATNDeserializer deserializer;
  _atn = deserializer.deserialize(_serializedATN);

  size_t count = _atn.getNumberOfDecisions();
  _decisionToDFA.reserve(count);
  for (size_t i = 0; i < count; i++) { 
    _decisionToDFA.emplace_back(_atn.getDecisionState(i), i);
  }
}

ExpressionAntlrParser::Initializer ExpressionAntlrParser::_init;
