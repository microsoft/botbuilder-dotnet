// Generated from d:\project\botframework\botbuilder-dotnet\libraries\Microsoft.Bot.Expressions\parser\ExpressionParser.g4 by ANTLR 4.7.1
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class ExpressionParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.7.1", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		STRING_INTERPOLATION_START=1, PLUS=2, SUBSTRACT=3, NON=4, XOR=5, ASTERISK=6, 
		SLASH=7, PERCENT=8, DOUBLE_EQUAL=9, NOT_EQUAL=10, SINGLE_AND=11, DOUBLE_AND=12, 
		DOUBLE_VERTICAL_CYLINDER=13, LESS_THAN=14, MORE_THAN=15, LESS_OR_EQUAl=16, 
		MORE_OR_EQUAL=17, OPEN_BRACKET=18, CLOSE_BRACKET=19, DOT=20, OPEN_SQUARE_BRACKET=21, 
		CLOSE_SQUARE_BRACKET=22, COMMA=23, NUMBER=24, WHITESPACE=25, IDENTIFIER=26, 
		NEWLINE=27, STRING=28, CONSTANT=29, INVALID_TOKEN_DEFAULT_MODE=30, TEMPLATE=31, 
		ESCAPE_CHARACTER=32, TEXT_CONTENT=33;
	public static final int
		RULE_file = 0, RULE_expression = 1, RULE_primaryExpression = 2, RULE_stringInterpolation = 3, 
		RULE_argsList = 4;
	public static final String[] ruleNames = {
		"file", "expression", "primaryExpression", "stringInterpolation", "argsList"
	};

	private static final String[] _LITERAL_NAMES = {
		null, null, "'+'", "'-'", "'!'", "'^'", "'*'", "'/'", "'%'", "'=='", null, 
		"'&'", "'&&'", "'||'", "'<'", "'>'", "'<='", "'>='", "'('", "')'", "'.'", 
		"'['", "']'", "','"
	};
	private static final String[] _SYMBOLIC_NAMES = {
		null, "STRING_INTERPOLATION_START", "PLUS", "SUBSTRACT", "NON", "XOR", 
		"ASTERISK", "SLASH", "PERCENT", "DOUBLE_EQUAL", "NOT_EQUAL", "SINGLE_AND", 
		"DOUBLE_AND", "DOUBLE_VERTICAL_CYLINDER", "LESS_THAN", "MORE_THAN", "LESS_OR_EQUAl", 
		"MORE_OR_EQUAL", "OPEN_BRACKET", "CLOSE_BRACKET", "DOT", "OPEN_SQUARE_BRACKET", 
		"CLOSE_SQUARE_BRACKET", "COMMA", "NUMBER", "WHITESPACE", "IDENTIFIER", 
		"NEWLINE", "STRING", "CONSTANT", "INVALID_TOKEN_DEFAULT_MODE", "TEMPLATE", 
		"ESCAPE_CHARACTER", "TEXT_CONTENT"
	};
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}

	@Override
	public String getGrammarFileName() { return "ExpressionParser.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public ExpressionParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}
	public static class FileContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode EOF() { return getToken(ExpressionParser.EOF, 0); }
		public FileContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_file; }
	}

	public final FileContext file() throws RecognitionException {
		FileContext _localctx = new FileContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_file);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(10);
			expression(0);
			setState(11);
			match(EOF);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ExpressionContext extends ParserRuleContext {
		public ExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expression; }
	 
		public ExpressionContext() { }
		public void copyFrom(ExpressionContext ctx) {
			super.copyFrom(ctx);
		}
	}
	public static class UnaryOpExpContext extends ExpressionContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode NON() { return getToken(ExpressionParser.NON, 0); }
		public TerminalNode SUBSTRACT() { return getToken(ExpressionParser.SUBSTRACT, 0); }
		public TerminalNode PLUS() { return getToken(ExpressionParser.PLUS, 0); }
		public UnaryOpExpContext(ExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class BinaryOpExpContext extends ExpressionContext {
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public TerminalNode XOR() { return getToken(ExpressionParser.XOR, 0); }
		public TerminalNode ASTERISK() { return getToken(ExpressionParser.ASTERISK, 0); }
		public TerminalNode SLASH() { return getToken(ExpressionParser.SLASH, 0); }
		public TerminalNode PERCENT() { return getToken(ExpressionParser.PERCENT, 0); }
		public TerminalNode PLUS() { return getToken(ExpressionParser.PLUS, 0); }
		public TerminalNode SUBSTRACT() { return getToken(ExpressionParser.SUBSTRACT, 0); }
		public TerminalNode DOUBLE_EQUAL() { return getToken(ExpressionParser.DOUBLE_EQUAL, 0); }
		public TerminalNode NOT_EQUAL() { return getToken(ExpressionParser.NOT_EQUAL, 0); }
		public TerminalNode SINGLE_AND() { return getToken(ExpressionParser.SINGLE_AND, 0); }
		public TerminalNode LESS_THAN() { return getToken(ExpressionParser.LESS_THAN, 0); }
		public TerminalNode LESS_OR_EQUAl() { return getToken(ExpressionParser.LESS_OR_EQUAl, 0); }
		public TerminalNode MORE_THAN() { return getToken(ExpressionParser.MORE_THAN, 0); }
		public TerminalNode MORE_OR_EQUAL() { return getToken(ExpressionParser.MORE_OR_EQUAL, 0); }
		public TerminalNode DOUBLE_AND() { return getToken(ExpressionParser.DOUBLE_AND, 0); }
		public TerminalNode DOUBLE_VERTICAL_CYLINDER() { return getToken(ExpressionParser.DOUBLE_VERTICAL_CYLINDER, 0); }
		public BinaryOpExpContext(ExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class PrimaryExpContext extends ExpressionContext {
		public PrimaryExpressionContext primaryExpression() {
			return getRuleContext(PrimaryExpressionContext.class,0);
		}
		public PrimaryExpContext(ExpressionContext ctx) { copyFrom(ctx); }
	}

	public final ExpressionContext expression() throws RecognitionException {
		return expression(0);
	}

	private ExpressionContext expression(int _p) throws RecognitionException {
		ParserRuleContext _parentctx = _ctx;
		int _parentState = getState();
		ExpressionContext _localctx = new ExpressionContext(_ctx, _parentState);
		ExpressionContext _prevctx = _localctx;
		int _startState = 2;
		enterRecursionRule(_localctx, 2, RULE_expression, _p);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(17);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case PLUS:
			case SUBSTRACT:
			case NON:
				{
				_localctx = new UnaryOpExpContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;

				setState(14);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << PLUS) | (1L << SUBSTRACT) | (1L << NON))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(15);
				expression(10);
				}
				break;
			case STRING_INTERPOLATION_START:
			case OPEN_BRACKET:
			case NUMBER:
			case IDENTIFIER:
			case STRING:
			case CONSTANT:
				{
				_localctx = new PrimaryExpContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(16);
				primaryExpression(0);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(45);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,2,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(43);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
					case 1:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(19);
						if (!(precpred(_ctx, 9))) throw new FailedPredicateException(this, "precpred(_ctx, 9)");
						setState(20);
						match(XOR);
						setState(21);
						expression(9);
						}
						break;
					case 2:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(22);
						if (!(precpred(_ctx, 8))) throw new FailedPredicateException(this, "precpred(_ctx, 8)");
						setState(23);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << ASTERISK) | (1L << SLASH) | (1L << PERCENT))) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(24);
						expression(9);
						}
						break;
					case 3:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(25);
						if (!(precpred(_ctx, 7))) throw new FailedPredicateException(this, "precpred(_ctx, 7)");
						setState(26);
						_la = _input.LA(1);
						if ( !(_la==PLUS || _la==SUBSTRACT) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(27);
						expression(8);
						}
						break;
					case 4:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(28);
						if (!(precpred(_ctx, 6))) throw new FailedPredicateException(this, "precpred(_ctx, 6)");
						setState(29);
						_la = _input.LA(1);
						if ( !(_la==DOUBLE_EQUAL || _la==NOT_EQUAL) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(30);
						expression(7);
						}
						break;
					case 5:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(31);
						if (!(precpred(_ctx, 5))) throw new FailedPredicateException(this, "precpred(_ctx, 5)");
						{
						setState(32);
						match(SINGLE_AND);
						}
						setState(33);
						expression(6);
						}
						break;
					case 6:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(34);
						if (!(precpred(_ctx, 4))) throw new FailedPredicateException(this, "precpred(_ctx, 4)");
						setState(35);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << LESS_THAN) | (1L << MORE_THAN) | (1L << LESS_OR_EQUAl) | (1L << MORE_OR_EQUAL))) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(36);
						expression(5);
						}
						break;
					case 7:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(37);
						if (!(precpred(_ctx, 3))) throw new FailedPredicateException(this, "precpred(_ctx, 3)");
						setState(38);
						match(DOUBLE_AND);
						setState(39);
						expression(4);
						}
						break;
					case 8:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(40);
						if (!(precpred(_ctx, 2))) throw new FailedPredicateException(this, "precpred(_ctx, 2)");
						setState(41);
						match(DOUBLE_VERTICAL_CYLINDER);
						setState(42);
						expression(3);
						}
						break;
					}
					} 
				}
				setState(47);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,2,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			unrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	public static class PrimaryExpressionContext extends ParserRuleContext {
		public PrimaryExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_primaryExpression; }
	 
		public PrimaryExpressionContext() { }
		public void copyFrom(PrimaryExpressionContext ctx) {
			super.copyFrom(ctx);
		}
	}
	public static class FuncInvokeExpContext extends PrimaryExpressionContext {
		public PrimaryExpressionContext primaryExpression() {
			return getRuleContext(PrimaryExpressionContext.class,0);
		}
		public TerminalNode OPEN_BRACKET() { return getToken(ExpressionParser.OPEN_BRACKET, 0); }
		public TerminalNode CLOSE_BRACKET() { return getToken(ExpressionParser.CLOSE_BRACKET, 0); }
		public ArgsListContext argsList() {
			return getRuleContext(ArgsListContext.class,0);
		}
		public FuncInvokeExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class ConstantAtomContext extends PrimaryExpressionContext {
		public TerminalNode CONSTANT() { return getToken(ExpressionParser.CONSTANT, 0); }
		public ConstantAtomContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class IdAtomContext extends PrimaryExpressionContext {
		public TerminalNode IDENTIFIER() { return getToken(ExpressionParser.IDENTIFIER, 0); }
		public IdAtomContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class StringAtomContext extends PrimaryExpressionContext {
		public TerminalNode STRING() { return getToken(ExpressionParser.STRING, 0); }
		public StringAtomContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class IndexAccessExpContext extends PrimaryExpressionContext {
		public PrimaryExpressionContext primaryExpression() {
			return getRuleContext(PrimaryExpressionContext.class,0);
		}
		public TerminalNode OPEN_SQUARE_BRACKET() { return getToken(ExpressionParser.OPEN_SQUARE_BRACKET, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode CLOSE_SQUARE_BRACKET() { return getToken(ExpressionParser.CLOSE_SQUARE_BRACKET, 0); }
		public IndexAccessExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class StringInterpolationAtomContext extends PrimaryExpressionContext {
		public StringInterpolationContext stringInterpolation() {
			return getRuleContext(StringInterpolationContext.class,0);
		}
		public StringInterpolationAtomContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class MemberAccessExpContext extends PrimaryExpressionContext {
		public PrimaryExpressionContext primaryExpression() {
			return getRuleContext(PrimaryExpressionContext.class,0);
		}
		public TerminalNode DOT() { return getToken(ExpressionParser.DOT, 0); }
		public TerminalNode IDENTIFIER() { return getToken(ExpressionParser.IDENTIFIER, 0); }
		public MemberAccessExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class ParenthesisExpContext extends PrimaryExpressionContext {
		public TerminalNode OPEN_BRACKET() { return getToken(ExpressionParser.OPEN_BRACKET, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode CLOSE_BRACKET() { return getToken(ExpressionParser.CLOSE_BRACKET, 0); }
		public ParenthesisExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class NumericAtomContext extends PrimaryExpressionContext {
		public TerminalNode NUMBER() { return getToken(ExpressionParser.NUMBER, 0); }
		public NumericAtomContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}

	public final PrimaryExpressionContext primaryExpression() throws RecognitionException {
		return primaryExpression(0);
	}

	private PrimaryExpressionContext primaryExpression(int _p) throws RecognitionException {
		ParserRuleContext _parentctx = _ctx;
		int _parentState = getState();
		PrimaryExpressionContext _localctx = new PrimaryExpressionContext(_ctx, _parentState);
		PrimaryExpressionContext _prevctx = _localctx;
		int _startState = 4;
		enterRecursionRule(_localctx, 4, RULE_primaryExpression, _p);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(58);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case OPEN_BRACKET:
				{
				_localctx = new ParenthesisExpContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;

				setState(49);
				match(OPEN_BRACKET);
				setState(50);
				expression(0);
				setState(51);
				match(CLOSE_BRACKET);
				}
				break;
			case CONSTANT:
				{
				_localctx = new ConstantAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(53);
				match(CONSTANT);
				}
				break;
			case NUMBER:
				{
				_localctx = new NumericAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(54);
				match(NUMBER);
				}
				break;
			case STRING:
				{
				_localctx = new StringAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(55);
				match(STRING);
				}
				break;
			case IDENTIFIER:
				{
				_localctx = new IdAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(56);
				match(IDENTIFIER);
				}
				break;
			case STRING_INTERPOLATION_START:
				{
				_localctx = new StringInterpolationAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(57);
				stringInterpolation();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(76);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,6,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(74);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,5,_ctx) ) {
					case 1:
						{
						_localctx = new MemberAccessExpContext(new PrimaryExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_primaryExpression);
						setState(60);
						if (!(precpred(_ctx, 3))) throw new FailedPredicateException(this, "precpred(_ctx, 3)");
						setState(61);
						match(DOT);
						setState(62);
						match(IDENTIFIER);
						}
						break;
					case 2:
						{
						_localctx = new FuncInvokeExpContext(new PrimaryExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_primaryExpression);
						setState(63);
						if (!(precpred(_ctx, 2))) throw new FailedPredicateException(this, "precpred(_ctx, 2)");
						setState(64);
						match(OPEN_BRACKET);
						setState(66);
						_errHandler.sync(this);
						_la = _input.LA(1);
						if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << STRING_INTERPOLATION_START) | (1L << PLUS) | (1L << SUBSTRACT) | (1L << NON) | (1L << OPEN_BRACKET) | (1L << NUMBER) | (1L << IDENTIFIER) | (1L << STRING) | (1L << CONSTANT))) != 0)) {
							{
							setState(65);
							argsList();
							}
						}

						setState(68);
						match(CLOSE_BRACKET);
						}
						break;
					case 3:
						{
						_localctx = new IndexAccessExpContext(new PrimaryExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_primaryExpression);
						setState(69);
						if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
						setState(70);
						match(OPEN_SQUARE_BRACKET);
						setState(71);
						expression(0);
						setState(72);
						match(CLOSE_SQUARE_BRACKET);
						}
						break;
					}
					} 
				}
				setState(78);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,6,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			unrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	public static class StringInterpolationContext extends ParserRuleContext {
		public List<TerminalNode> STRING_INTERPOLATION_START() { return getTokens(ExpressionParser.STRING_INTERPOLATION_START); }
		public TerminalNode STRING_INTERPOLATION_START(int i) {
			return getToken(ExpressionParser.STRING_INTERPOLATION_START, i);
		}
		public List<TerminalNode> ESCAPE_CHARACTER() { return getTokens(ExpressionParser.ESCAPE_CHARACTER); }
		public TerminalNode ESCAPE_CHARACTER(int i) {
			return getToken(ExpressionParser.ESCAPE_CHARACTER, i);
		}
		public List<TerminalNode> TEMPLATE() { return getTokens(ExpressionParser.TEMPLATE); }
		public TerminalNode TEMPLATE(int i) {
			return getToken(ExpressionParser.TEMPLATE, i);
		}
		public List<TerminalNode> TEXT_CONTENT() { return getTokens(ExpressionParser.TEXT_CONTENT); }
		public TerminalNode TEXT_CONTENT(int i) {
			return getToken(ExpressionParser.TEXT_CONTENT, i);
		}
		public StringInterpolationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_stringInterpolation; }
	}

	public final StringInterpolationContext stringInterpolation() throws RecognitionException {
		StringInterpolationContext _localctx = new StringInterpolationContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_stringInterpolation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(79);
			match(STRING_INTERPOLATION_START);
			setState(81); 
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(80);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << TEMPLATE) | (1L << ESCAPE_CHARACTER) | (1L << TEXT_CONTENT))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				}
				}
				setState(83); 
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( (((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << TEMPLATE) | (1L << ESCAPE_CHARACTER) | (1L << TEXT_CONTENT))) != 0) );
			setState(85);
			match(STRING_INTERPOLATION_START);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ArgsListContext extends ParserRuleContext {
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(ExpressionParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(ExpressionParser.COMMA, i);
		}
		public ArgsListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argsList; }
	}

	public final ArgsListContext argsList() throws RecognitionException {
		ArgsListContext _localctx = new ArgsListContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_argsList);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(87);
			expression(0);
			setState(92);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(88);
				match(COMMA);
				setState(89);
				expression(0);
				}
				}
				setState(94);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 1:
			return expression_sempred((ExpressionContext)_localctx, predIndex);
		case 2:
			return primaryExpression_sempred((PrimaryExpressionContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean expression_sempred(ExpressionContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return precpred(_ctx, 9);
		case 1:
			return precpred(_ctx, 8);
		case 2:
			return precpred(_ctx, 7);
		case 3:
			return precpred(_ctx, 6);
		case 4:
			return precpred(_ctx, 5);
		case 5:
			return precpred(_ctx, 4);
		case 6:
			return precpred(_ctx, 3);
		case 7:
			return precpred(_ctx, 2);
		}
		return true;
	}
	private boolean primaryExpression_sempred(PrimaryExpressionContext _localctx, int predIndex) {
		switch (predIndex) {
		case 8:
			return precpred(_ctx, 3);
		case 9:
			return precpred(_ctx, 2);
		case 10:
			return precpred(_ctx, 1);
		}
		return true;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3#b\4\2\t\2\4\3\t\3"+
		"\4\4\t\4\4\5\t\5\4\6\t\6\3\2\3\2\3\2\3\3\3\3\3\3\3\3\5\3\24\n\3\3\3\3"+
		"\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3"+
		"\3\3\3\3\3\3\3\3\3\3\7\3.\n\3\f\3\16\3\61\13\3\3\4\3\4\3\4\3\4\3\4\3\4"+
		"\3\4\3\4\3\4\3\4\5\4=\n\4\3\4\3\4\3\4\3\4\3\4\3\4\5\4E\n\4\3\4\3\4\3\4"+
		"\3\4\3\4\3\4\7\4M\n\4\f\4\16\4P\13\4\3\5\3\5\6\5T\n\5\r\5\16\5U\3\5\3"+
		"\5\3\6\3\6\3\6\7\6]\n\6\f\6\16\6`\13\6\3\6\2\4\4\6\7\2\4\6\b\n\2\b\3\2"+
		"\4\6\3\2\b\n\3\2\4\5\3\2\13\f\3\2\20\23\3\2!#\2p\2\f\3\2\2\2\4\23\3\2"+
		"\2\2\6<\3\2\2\2\bQ\3\2\2\2\nY\3\2\2\2\f\r\5\4\3\2\r\16\7\2\2\3\16\3\3"+
		"\2\2\2\17\20\b\3\1\2\20\21\t\2\2\2\21\24\5\4\3\f\22\24\5\6\4\2\23\17\3"+
		"\2\2\2\23\22\3\2\2\2\24/\3\2\2\2\25\26\f\13\2\2\26\27\7\7\2\2\27.\5\4"+
		"\3\13\30\31\f\n\2\2\31\32\t\3\2\2\32.\5\4\3\13\33\34\f\t\2\2\34\35\t\4"+
		"\2\2\35.\5\4\3\n\36\37\f\b\2\2\37 \t\5\2\2 .\5\4\3\t!\"\f\7\2\2\"#\7\r"+
		"\2\2#.\5\4\3\b$%\f\6\2\2%&\t\6\2\2&.\5\4\3\7\'(\f\5\2\2()\7\16\2\2).\5"+
		"\4\3\6*+\f\4\2\2+,\7\17\2\2,.\5\4\3\5-\25\3\2\2\2-\30\3\2\2\2-\33\3\2"+
		"\2\2-\36\3\2\2\2-!\3\2\2\2-$\3\2\2\2-\'\3\2\2\2-*\3\2\2\2.\61\3\2\2\2"+
		"/-\3\2\2\2/\60\3\2\2\2\60\5\3\2\2\2\61/\3\2\2\2\62\63\b\4\1\2\63\64\7"+
		"\24\2\2\64\65\5\4\3\2\65\66\7\25\2\2\66=\3\2\2\2\67=\7\37\2\28=\7\32\2"+
		"\29=\7\36\2\2:=\7\34\2\2;=\5\b\5\2<\62\3\2\2\2<\67\3\2\2\2<8\3\2\2\2<"+
		"9\3\2\2\2<:\3\2\2\2<;\3\2\2\2=N\3\2\2\2>?\f\5\2\2?@\7\26\2\2@M\7\34\2"+
		"\2AB\f\4\2\2BD\7\24\2\2CE\5\n\6\2DC\3\2\2\2DE\3\2\2\2EF\3\2\2\2FM\7\25"+
		"\2\2GH\f\3\2\2HI\7\27\2\2IJ\5\4\3\2JK\7\30\2\2KM\3\2\2\2L>\3\2\2\2LA\3"+
		"\2\2\2LG\3\2\2\2MP\3\2\2\2NL\3\2\2\2NO\3\2\2\2O\7\3\2\2\2PN\3\2\2\2QS"+
		"\7\3\2\2RT\t\7\2\2SR\3\2\2\2TU\3\2\2\2US\3\2\2\2UV\3\2\2\2VW\3\2\2\2W"+
		"X\7\3\2\2X\t\3\2\2\2Y^\5\4\3\2Z[\7\31\2\2[]\5\4\3\2\\Z\3\2\2\2]`\3\2\2"+
		"\2^\\\3\2\2\2^_\3\2\2\2_\13\3\2\2\2`^\3\2\2\2\13\23-/<DLNU^";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}