// Generated from c:\Repos\TemplatingLib\botbuilder-dotnet\libraries\AdaptiveExpressionsCpp\AdaptiveExpressionsCpp\Parser\ExpressionAntlrParser.g4 by ANTLR 4.8
#pragma warning disable 3021
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class ExpressionAntlrParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.8", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		STRING_INTERPOLATION_START=1, PLUS=2, SUBSTRACT=3, NON=4, XOR=5, ASTERISK=6, 
		SLASH=7, PERCENT=8, DOUBLE_EQUAL=9, NOT_EQUAL=10, SINGLE_AND=11, DOUBLE_AND=12, 
		DOUBLE_VERTICAL_CYLINDER=13, LESS_THAN=14, MORE_THAN=15, LESS_OR_EQUAl=16, 
		MORE_OR_EQUAL=17, OPEN_BRACKET=18, CLOSE_BRACKET=19, DOT=20, OPEN_SQUARE_BRACKET=21, 
		CLOSE_SQUARE_BRACKET=22, OPEN_CURLY_BRACKET=23, CLOSE_CURLY_BRACKET=24, 
		COMMA=25, COLON=26, ARROW=27, NUMBER=28, WHITESPACE=29, IDENTIFIER=30, 
		NEWLINE=31, STRING=32, INVALID_TOKEN_DEFAULT_MODE=33, TEMPLATE=34, ESCAPE_CHARACTER=35, 
		TEXT_CONTENT=36;
	public static final int
		RULE_file = 0, RULE_expression = 1, RULE_primaryExpression = 2, RULE_stringInterpolation = 3, 
		RULE_textContent = 4, RULE_argsList = 5, RULE_lambda = 6, RULE_keyValuePairList = 7, 
		RULE_keyValuePair = 8, RULE_key = 9;
	private static String[] makeRuleNames() {
		return new String[] {
			"file", "expression", "primaryExpression", "stringInterpolation", "textContent", 
			"argsList", "lambda", "keyValuePairList", "keyValuePair", "key"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, "'+'", "'-'", "'!'", "'^'", "'*'", "'/'", "'%'", "'=='", 
			null, "'&'", "'&&'", "'||'", "'<'", "'>'", "'<='", "'>='", "'('", "')'", 
			"'.'", "'['", "']'", "'{'", "'}'", "','", "':'", "'=>'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "STRING_INTERPOLATION_START", "PLUS", "SUBSTRACT", "NON", "XOR", 
			"ASTERISK", "SLASH", "PERCENT", "DOUBLE_EQUAL", "NOT_EQUAL", "SINGLE_AND", 
			"DOUBLE_AND", "DOUBLE_VERTICAL_CYLINDER", "LESS_THAN", "MORE_THAN", "LESS_OR_EQUAl", 
			"MORE_OR_EQUAL", "OPEN_BRACKET", "CLOSE_BRACKET", "DOT", "OPEN_SQUARE_BRACKET", 
			"CLOSE_SQUARE_BRACKET", "OPEN_CURLY_BRACKET", "CLOSE_CURLY_BRACKET", 
			"COMMA", "COLON", "ARROW", "NUMBER", "WHITESPACE", "IDENTIFIER", "NEWLINE", 
			"STRING", "INVALID_TOKEN_DEFAULT_MODE", "TEMPLATE", "ESCAPE_CHARACTER", 
			"TEXT_CONTENT"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
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
	public String getGrammarFileName() { return "ExpressionAntlrParser.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public ExpressionAntlrParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	public static class FileContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode EOF() { return getToken(ExpressionAntlrParser.EOF, 0); }
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
			setState(20);
			expression(0);
			setState(21);
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
		public TerminalNode NON() { return getToken(ExpressionAntlrParser.NON, 0); }
		public TerminalNode SUBSTRACT() { return getToken(ExpressionAntlrParser.SUBSTRACT, 0); }
		public TerminalNode PLUS() { return getToken(ExpressionAntlrParser.PLUS, 0); }
		public UnaryOpExpContext(ExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class BinaryOpExpContext extends ExpressionContext {
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public TerminalNode XOR() { return getToken(ExpressionAntlrParser.XOR, 0); }
		public TerminalNode ASTERISK() { return getToken(ExpressionAntlrParser.ASTERISK, 0); }
		public TerminalNode SLASH() { return getToken(ExpressionAntlrParser.SLASH, 0); }
		public TerminalNode PERCENT() { return getToken(ExpressionAntlrParser.PERCENT, 0); }
		public TerminalNode PLUS() { return getToken(ExpressionAntlrParser.PLUS, 0); }
		public TerminalNode SUBSTRACT() { return getToken(ExpressionAntlrParser.SUBSTRACT, 0); }
		public TerminalNode DOUBLE_EQUAL() { return getToken(ExpressionAntlrParser.DOUBLE_EQUAL, 0); }
		public TerminalNode NOT_EQUAL() { return getToken(ExpressionAntlrParser.NOT_EQUAL, 0); }
		public TerminalNode SINGLE_AND() { return getToken(ExpressionAntlrParser.SINGLE_AND, 0); }
		public TerminalNode LESS_THAN() { return getToken(ExpressionAntlrParser.LESS_THAN, 0); }
		public TerminalNode LESS_OR_EQUAl() { return getToken(ExpressionAntlrParser.LESS_OR_EQUAl, 0); }
		public TerminalNode MORE_THAN() { return getToken(ExpressionAntlrParser.MORE_THAN, 0); }
		public TerminalNode MORE_OR_EQUAL() { return getToken(ExpressionAntlrParser.MORE_OR_EQUAL, 0); }
		public TerminalNode DOUBLE_AND() { return getToken(ExpressionAntlrParser.DOUBLE_AND, 0); }
		public TerminalNode DOUBLE_VERTICAL_CYLINDER() { return getToken(ExpressionAntlrParser.DOUBLE_VERTICAL_CYLINDER, 0); }
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
			setState(27);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case PLUS:
			case SUBSTRACT:
			case NON:
				{
				_localctx = new UnaryOpExpContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;

				setState(24);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << PLUS) | (1L << SUBSTRACT) | (1L << NON))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(25);
				expression(10);
				}
				break;
			case STRING_INTERPOLATION_START:
			case OPEN_BRACKET:
			case OPEN_SQUARE_BRACKET:
			case OPEN_CURLY_BRACKET:
			case NUMBER:
			case IDENTIFIER:
			case STRING:
				{
				_localctx = new PrimaryExpContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(26);
				primaryExpression(0);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(55);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,2,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(53);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
					case 1:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(29);
						if (!(precpred(_ctx, 9))) throw new FailedPredicateException(this, "precpred(_ctx, 9)");
						setState(30);
						match(XOR);
						setState(31);
						expression(9);
						}
						break;
					case 2:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(32);
						if (!(precpred(_ctx, 8))) throw new FailedPredicateException(this, "precpred(_ctx, 8)");
						setState(33);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << ASTERISK) | (1L << SLASH) | (1L << PERCENT))) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(34);
						expression(9);
						}
						break;
					case 3:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(35);
						if (!(precpred(_ctx, 7))) throw new FailedPredicateException(this, "precpred(_ctx, 7)");
						setState(36);
						_la = _input.LA(1);
						if ( !(_la==PLUS || _la==SUBSTRACT) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(37);
						expression(8);
						}
						break;
					case 4:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(38);
						if (!(precpred(_ctx, 6))) throw new FailedPredicateException(this, "precpred(_ctx, 6)");
						setState(39);
						_la = _input.LA(1);
						if ( !(_la==DOUBLE_EQUAL || _la==NOT_EQUAL) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(40);
						expression(7);
						}
						break;
					case 5:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(41);
						if (!(precpred(_ctx, 5))) throw new FailedPredicateException(this, "precpred(_ctx, 5)");
						{
						setState(42);
						match(SINGLE_AND);
						}
						setState(43);
						expression(6);
						}
						break;
					case 6:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(44);
						if (!(precpred(_ctx, 4))) throw new FailedPredicateException(this, "precpred(_ctx, 4)");
						setState(45);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << LESS_THAN) | (1L << MORE_THAN) | (1L << LESS_OR_EQUAl) | (1L << MORE_OR_EQUAL))) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(46);
						expression(5);
						}
						break;
					case 7:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(47);
						if (!(precpred(_ctx, 3))) throw new FailedPredicateException(this, "precpred(_ctx, 3)");
						setState(48);
						match(DOUBLE_AND);
						setState(49);
						expression(4);
						}
						break;
					case 8:
						{
						_localctx = new BinaryOpExpContext(new ExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_expression);
						setState(50);
						if (!(precpred(_ctx, 2))) throw new FailedPredicateException(this, "precpred(_ctx, 2)");
						setState(51);
						match(DOUBLE_VERTICAL_CYLINDER);
						setState(52);
						expression(3);
						}
						break;
					}
					} 
				}
				setState(57);
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
		public TerminalNode OPEN_BRACKET() { return getToken(ExpressionAntlrParser.OPEN_BRACKET, 0); }
		public TerminalNode CLOSE_BRACKET() { return getToken(ExpressionAntlrParser.CLOSE_BRACKET, 0); }
		public TerminalNode NON() { return getToken(ExpressionAntlrParser.NON, 0); }
		public ArgsListContext argsList() {
			return getRuleContext(ArgsListContext.class,0);
		}
		public FuncInvokeExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class IdAtomContext extends PrimaryExpressionContext {
		public TerminalNode IDENTIFIER() { return getToken(ExpressionAntlrParser.IDENTIFIER, 0); }
		public IdAtomContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class JsonCreationExpContext extends PrimaryExpressionContext {
		public TerminalNode OPEN_CURLY_BRACKET() { return getToken(ExpressionAntlrParser.OPEN_CURLY_BRACKET, 0); }
		public TerminalNode CLOSE_CURLY_BRACKET() { return getToken(ExpressionAntlrParser.CLOSE_CURLY_BRACKET, 0); }
		public KeyValuePairListContext keyValuePairList() {
			return getRuleContext(KeyValuePairListContext.class,0);
		}
		public JsonCreationExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class StringAtomContext extends PrimaryExpressionContext {
		public TerminalNode STRING() { return getToken(ExpressionAntlrParser.STRING, 0); }
		public StringAtomContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class IndexAccessExpContext extends PrimaryExpressionContext {
		public PrimaryExpressionContext primaryExpression() {
			return getRuleContext(PrimaryExpressionContext.class,0);
		}
		public TerminalNode OPEN_SQUARE_BRACKET() { return getToken(ExpressionAntlrParser.OPEN_SQUARE_BRACKET, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode CLOSE_SQUARE_BRACKET() { return getToken(ExpressionAntlrParser.CLOSE_SQUARE_BRACKET, 0); }
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
		public TerminalNode DOT() { return getToken(ExpressionAntlrParser.DOT, 0); }
		public TerminalNode IDENTIFIER() { return getToken(ExpressionAntlrParser.IDENTIFIER, 0); }
		public MemberAccessExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class ParenthesisExpContext extends PrimaryExpressionContext {
		public TerminalNode OPEN_BRACKET() { return getToken(ExpressionAntlrParser.OPEN_BRACKET, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode CLOSE_BRACKET() { return getToken(ExpressionAntlrParser.CLOSE_BRACKET, 0); }
		public ParenthesisExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class NumericAtomContext extends PrimaryExpressionContext {
		public TerminalNode NUMBER() { return getToken(ExpressionAntlrParser.NUMBER, 0); }
		public NumericAtomContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
	}
	public static class ArrayCreationExpContext extends PrimaryExpressionContext {
		public TerminalNode OPEN_SQUARE_BRACKET() { return getToken(ExpressionAntlrParser.OPEN_SQUARE_BRACKET, 0); }
		public TerminalNode CLOSE_SQUARE_BRACKET() { return getToken(ExpressionAntlrParser.CLOSE_SQUARE_BRACKET, 0); }
		public ArgsListContext argsList() {
			return getRuleContext(ArgsListContext.class,0);
		}
		public ArrayCreationExpContext(PrimaryExpressionContext ctx) { copyFrom(ctx); }
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
			setState(77);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case OPEN_BRACKET:
				{
				_localctx = new ParenthesisExpContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;

				setState(59);
				match(OPEN_BRACKET);
				setState(60);
				expression(0);
				setState(61);
				match(CLOSE_BRACKET);
				}
				break;
			case OPEN_SQUARE_BRACKET:
				{
				_localctx = new ArrayCreationExpContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(63);
				match(OPEN_SQUARE_BRACKET);
				setState(65);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << STRING_INTERPOLATION_START) | (1L << PLUS) | (1L << SUBSTRACT) | (1L << NON) | (1L << OPEN_BRACKET) | (1L << OPEN_SQUARE_BRACKET) | (1L << OPEN_CURLY_BRACKET) | (1L << NUMBER) | (1L << IDENTIFIER) | (1L << STRING))) != 0)) {
					{
					setState(64);
					argsList();
					}
				}

				setState(67);
				match(CLOSE_SQUARE_BRACKET);
				}
				break;
			case OPEN_CURLY_BRACKET:
				{
				_localctx = new JsonCreationExpContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(68);
				match(OPEN_CURLY_BRACKET);
				setState(70);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==IDENTIFIER || _la==STRING) {
					{
					setState(69);
					keyValuePairList();
					}
				}

				setState(72);
				match(CLOSE_CURLY_BRACKET);
				}
				break;
			case NUMBER:
				{
				_localctx = new NumericAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(73);
				match(NUMBER);
				}
				break;
			case STRING:
				{
				_localctx = new StringAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(74);
				match(STRING);
				}
				break;
			case IDENTIFIER:
				{
				_localctx = new IdAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(75);
				match(IDENTIFIER);
				}
				break;
			case STRING_INTERPOLATION_START:
				{
				_localctx = new StringInterpolationAtomContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(76);
				stringInterpolation();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(98);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,9,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(96);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,8,_ctx) ) {
					case 1:
						{
						_localctx = new MemberAccessExpContext(new PrimaryExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_primaryExpression);
						setState(79);
						if (!(precpred(_ctx, 3))) throw new FailedPredicateException(this, "precpred(_ctx, 3)");
						setState(80);
						match(DOT);
						setState(81);
						match(IDENTIFIER);
						}
						break;
					case 2:
						{
						_localctx = new FuncInvokeExpContext(new PrimaryExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_primaryExpression);
						setState(82);
						if (!(precpred(_ctx, 2))) throw new FailedPredicateException(this, "precpred(_ctx, 2)");
						setState(84);
						_errHandler.sync(this);
						_la = _input.LA(1);
						if (_la==NON) {
							{
							setState(83);
							match(NON);
							}
						}

						setState(86);
						match(OPEN_BRACKET);
						setState(88);
						_errHandler.sync(this);
						_la = _input.LA(1);
						if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << STRING_INTERPOLATION_START) | (1L << PLUS) | (1L << SUBSTRACT) | (1L << NON) | (1L << OPEN_BRACKET) | (1L << OPEN_SQUARE_BRACKET) | (1L << OPEN_CURLY_BRACKET) | (1L << NUMBER) | (1L << IDENTIFIER) | (1L << STRING))) != 0)) {
							{
							setState(87);
							argsList();
							}
						}

						setState(90);
						match(CLOSE_BRACKET);
						}
						break;
					case 3:
						{
						_localctx = new IndexAccessExpContext(new PrimaryExpressionContext(_parentctx, _parentState));
						pushNewRecursionContext(_localctx, _startState, RULE_primaryExpression);
						setState(91);
						if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
						setState(92);
						match(OPEN_SQUARE_BRACKET);
						setState(93);
						expression(0);
						setState(94);
						match(CLOSE_SQUARE_BRACKET);
						}
						break;
					}
					} 
				}
				setState(100);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,9,_ctx);
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
		public List<TerminalNode> STRING_INTERPOLATION_START() { return getTokens(ExpressionAntlrParser.STRING_INTERPOLATION_START); }
		public TerminalNode STRING_INTERPOLATION_START(int i) {
			return getToken(ExpressionAntlrParser.STRING_INTERPOLATION_START, i);
		}
		public List<TerminalNode> ESCAPE_CHARACTER() { return getTokens(ExpressionAntlrParser.ESCAPE_CHARACTER); }
		public TerminalNode ESCAPE_CHARACTER(int i) {
			return getToken(ExpressionAntlrParser.ESCAPE_CHARACTER, i);
		}
		public List<TerminalNode> TEMPLATE() { return getTokens(ExpressionAntlrParser.TEMPLATE); }
		public TerminalNode TEMPLATE(int i) {
			return getToken(ExpressionAntlrParser.TEMPLATE, i);
		}
		public List<TextContentContext> textContent() {
			return getRuleContexts(TextContentContext.class);
		}
		public TextContentContext textContent(int i) {
			return getRuleContext(TextContentContext.class,i);
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
			setState(101);
			match(STRING_INTERPOLATION_START);
			setState(107);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << TEMPLATE) | (1L << ESCAPE_CHARACTER) | (1L << TEXT_CONTENT))) != 0)) {
				{
				setState(105);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ESCAPE_CHARACTER:
					{
					setState(102);
					match(ESCAPE_CHARACTER);
					}
					break;
				case TEMPLATE:
					{
					setState(103);
					match(TEMPLATE);
					}
					break;
				case TEXT_CONTENT:
					{
					setState(104);
					textContent();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(109);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(110);
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

	public static class TextContentContext extends ParserRuleContext {
		public List<TerminalNode> TEXT_CONTENT() { return getTokens(ExpressionAntlrParser.TEXT_CONTENT); }
		public TerminalNode TEXT_CONTENT(int i) {
			return getToken(ExpressionAntlrParser.TEXT_CONTENT, i);
		}
		public TextContentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_textContent; }
	}

	public final TextContentContext textContent() throws RecognitionException {
		TextContentContext _localctx = new TextContentContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_textContent);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(113); 
			_errHandler.sync(this);
			_alt = 1;
			do {
				switch (_alt) {
				case 1:
					{
					{
					setState(112);
					match(TEXT_CONTENT);
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(115); 
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,12,_ctx);
			} while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER );
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
		public List<LambdaContext> lambda() {
			return getRuleContexts(LambdaContext.class);
		}
		public LambdaContext lambda(int i) {
			return getRuleContext(LambdaContext.class,i);
		}
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(ExpressionAntlrParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(ExpressionAntlrParser.COMMA, i);
		}
		public ArgsListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argsList; }
	}

	public final ArgsListContext argsList() throws RecognitionException {
		ArgsListContext _localctx = new ArgsListContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_argsList);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(119);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,13,_ctx) ) {
			case 1:
				{
				setState(117);
				lambda();
				}
				break;
			case 2:
				{
				setState(118);
				expression(0);
				}
				break;
			}
			setState(128);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(121);
				match(COMMA);
				setState(124);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,14,_ctx) ) {
				case 1:
					{
					setState(122);
					lambda();
					}
					break;
				case 2:
					{
					setState(123);
					expression(0);
					}
					break;
				}
				}
				}
				setState(130);
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

	public static class LambdaContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(ExpressionAntlrParser.IDENTIFIER, 0); }
		public TerminalNode ARROW() { return getToken(ExpressionAntlrParser.ARROW, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public LambdaContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_lambda; }
	}

	public final LambdaContext lambda() throws RecognitionException {
		LambdaContext _localctx = new LambdaContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_lambda);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(131);
			match(IDENTIFIER);
			setState(132);
			match(ARROW);
			setState(133);
			expression(0);
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

	public static class KeyValuePairListContext extends ParserRuleContext {
		public List<KeyValuePairContext> keyValuePair() {
			return getRuleContexts(KeyValuePairContext.class);
		}
		public KeyValuePairContext keyValuePair(int i) {
			return getRuleContext(KeyValuePairContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(ExpressionAntlrParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(ExpressionAntlrParser.COMMA, i);
		}
		public KeyValuePairListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_keyValuePairList; }
	}

	public final KeyValuePairListContext keyValuePairList() throws RecognitionException {
		KeyValuePairListContext _localctx = new KeyValuePairListContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_keyValuePairList);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(135);
			keyValuePair();
			setState(140);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(136);
				match(COMMA);
				setState(137);
				keyValuePair();
				}
				}
				setState(142);
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

	public static class KeyValuePairContext extends ParserRuleContext {
		public KeyContext key() {
			return getRuleContext(KeyContext.class,0);
		}
		public TerminalNode COLON() { return getToken(ExpressionAntlrParser.COLON, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public KeyValuePairContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_keyValuePair; }
	}

	public final KeyValuePairContext keyValuePair() throws RecognitionException {
		KeyValuePairContext _localctx = new KeyValuePairContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_keyValuePair);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(143);
			key();
			setState(144);
			match(COLON);
			setState(145);
			expression(0);
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

	public static class KeyContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(ExpressionAntlrParser.IDENTIFIER, 0); }
		public TerminalNode STRING() { return getToken(ExpressionAntlrParser.STRING, 0); }
		public KeyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_key; }
	}

	public final KeyContext key() throws RecognitionException {
		KeyContext _localctx = new KeyContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_key);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(147);
			_la = _input.LA(1);
			if ( !(_la==IDENTIFIER || _la==STRING) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3&\u0098\4\2\t\2\4"+
		"\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13\t"+
		"\13\3\2\3\2\3\2\3\3\3\3\3\3\3\3\5\3\36\n\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3"+
		"\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\7"+
		"\38\n\3\f\3\16\3;\13\3\3\4\3\4\3\4\3\4\3\4\3\4\3\4\5\4D\n\4\3\4\3\4\3"+
		"\4\5\4I\n\4\3\4\3\4\3\4\3\4\3\4\5\4P\n\4\3\4\3\4\3\4\3\4\3\4\5\4W\n\4"+
		"\3\4\3\4\5\4[\n\4\3\4\3\4\3\4\3\4\3\4\3\4\7\4c\n\4\f\4\16\4f\13\4\3\5"+
		"\3\5\3\5\3\5\7\5l\n\5\f\5\16\5o\13\5\3\5\3\5\3\6\6\6t\n\6\r\6\16\6u\3"+
		"\7\3\7\5\7z\n\7\3\7\3\7\3\7\5\7\177\n\7\7\7\u0081\n\7\f\7\16\7\u0084\13"+
		"\7\3\b\3\b\3\b\3\b\3\t\3\t\3\t\7\t\u008d\n\t\f\t\16\t\u0090\13\t\3\n\3"+
		"\n\3\n\3\n\3\13\3\13\3\13\2\4\4\6\f\2\4\6\b\n\f\16\20\22\24\2\b\3\2\4"+
		"\6\3\2\b\n\3\2\4\5\3\2\13\f\3\2\20\23\4\2  \"\"\2\u00ab\2\26\3\2\2\2\4"+
		"\35\3\2\2\2\6O\3\2\2\2\bg\3\2\2\2\ns\3\2\2\2\fy\3\2\2\2\16\u0085\3\2\2"+
		"\2\20\u0089\3\2\2\2\22\u0091\3\2\2\2\24\u0095\3\2\2\2\26\27\5\4\3\2\27"+
		"\30\7\2\2\3\30\3\3\2\2\2\31\32\b\3\1\2\32\33\t\2\2\2\33\36\5\4\3\f\34"+
		"\36\5\6\4\2\35\31\3\2\2\2\35\34\3\2\2\2\369\3\2\2\2\37 \f\13\2\2 !\7\7"+
		"\2\2!8\5\4\3\13\"#\f\n\2\2#$\t\3\2\2$8\5\4\3\13%&\f\t\2\2&\'\t\4\2\2\'"+
		"8\5\4\3\n()\f\b\2\2)*\t\5\2\2*8\5\4\3\t+,\f\7\2\2,-\7\r\2\2-8\5\4\3\b"+
		"./\f\6\2\2/\60\t\6\2\2\608\5\4\3\7\61\62\f\5\2\2\62\63\7\16\2\2\638\5"+
		"\4\3\6\64\65\f\4\2\2\65\66\7\17\2\2\668\5\4\3\5\67\37\3\2\2\2\67\"\3\2"+
		"\2\2\67%\3\2\2\2\67(\3\2\2\2\67+\3\2\2\2\67.\3\2\2\2\67\61\3\2\2\2\67"+
		"\64\3\2\2\28;\3\2\2\29\67\3\2\2\29:\3\2\2\2:\5\3\2\2\2;9\3\2\2\2<=\b\4"+
		"\1\2=>\7\24\2\2>?\5\4\3\2?@\7\25\2\2@P\3\2\2\2AC\7\27\2\2BD\5\f\7\2CB"+
		"\3\2\2\2CD\3\2\2\2DE\3\2\2\2EP\7\30\2\2FH\7\31\2\2GI\5\20\t\2HG\3\2\2"+
		"\2HI\3\2\2\2IJ\3\2\2\2JP\7\32\2\2KP\7\36\2\2LP\7\"\2\2MP\7 \2\2NP\5\b"+
		"\5\2O<\3\2\2\2OA\3\2\2\2OF\3\2\2\2OK\3\2\2\2OL\3\2\2\2OM\3\2\2\2ON\3\2"+
		"\2\2Pd\3\2\2\2QR\f\5\2\2RS\7\26\2\2Sc\7 \2\2TV\f\4\2\2UW\7\6\2\2VU\3\2"+
		"\2\2VW\3\2\2\2WX\3\2\2\2XZ\7\24\2\2Y[\5\f\7\2ZY\3\2\2\2Z[\3\2\2\2[\\\3"+
		"\2\2\2\\c\7\25\2\2]^\f\3\2\2^_\7\27\2\2_`\5\4\3\2`a\7\30\2\2ac\3\2\2\2"+
		"bQ\3\2\2\2bT\3\2\2\2b]\3\2\2\2cf\3\2\2\2db\3\2\2\2de\3\2\2\2e\7\3\2\2"+
		"\2fd\3\2\2\2gm\7\3\2\2hl\7%\2\2il\7$\2\2jl\5\n\6\2kh\3\2\2\2ki\3\2\2\2"+
		"kj\3\2\2\2lo\3\2\2\2mk\3\2\2\2mn\3\2\2\2np\3\2\2\2om\3\2\2\2pq\7\3\2\2"+
		"q\t\3\2\2\2rt\7&\2\2sr\3\2\2\2tu\3\2\2\2us\3\2\2\2uv\3\2\2\2v\13\3\2\2"+
		"\2wz\5\16\b\2xz\5\4\3\2yw\3\2\2\2yx\3\2\2\2z\u0082\3\2\2\2{~\7\33\2\2"+
		"|\177\5\16\b\2}\177\5\4\3\2~|\3\2\2\2~}\3\2\2\2\177\u0081\3\2\2\2\u0080"+
		"{\3\2\2\2\u0081\u0084\3\2\2\2\u0082\u0080\3\2\2\2\u0082\u0083\3\2\2\2"+
		"\u0083\r\3\2\2\2\u0084\u0082\3\2\2\2\u0085\u0086\7 \2\2\u0086\u0087\7"+
		"\35\2\2\u0087\u0088\5\4\3\2\u0088\17\3\2\2\2\u0089\u008e\5\22\n\2\u008a"+
		"\u008b\7\33\2\2\u008b\u008d\5\22\n\2\u008c\u008a\3\2\2\2\u008d\u0090\3"+
		"\2\2\2\u008e\u008c\3\2\2\2\u008e\u008f\3\2\2\2\u008f\21\3\2\2\2\u0090"+
		"\u008e\3\2\2\2\u0091\u0092\5\24\13\2\u0092\u0093\7\34\2\2\u0093\u0094"+
		"\5\4\3\2\u0094\23\3\2\2\2\u0095\u0096\t\7\2\2\u0096\25\3\2\2\2\23\35\67"+
		"9CHOVZbdkmuy~\u0082\u008e";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}