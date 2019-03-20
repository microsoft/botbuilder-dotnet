// Generated from d:\projects\BotFramework\myfork\botbuilder-dotnet\libraries\Microsoft.Bot.Builder.AI.LanguageGeneration\LGFileParser.g4 by ANTLR 4.7.1
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class LGFileParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.7.1", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		COMMENTS=1, WS=2, NEWLINE=3, HASH=4, DASH=5, WS_IN_NAME=6, IDENTIFIER=7, 
		DOT=8, OPEN_PARENTHESIS=9, CLOSE_PARENTHESIS=10, COMMA=11, WS_IN_BODY_IGNORED=12, 
		CASE=13, DEFAULT=14, MULTI_LINE_TEXT=15, ESCAPE_CHARACTER=16, INVALID_ESCAPE=17, 
		EXPRESSION=18, TEMPLATE_REF=19, TEXT_SEPARATOR=20, TEXT=21;
	public static final int
		RULE_file = 0, RULE_paragraph = 1, RULE_newline = 2, RULE_templateDefinition = 3, 
		RULE_templateNameLine = 4, RULE_templateName = 5, RULE_parameters = 6, 
		RULE_templateBody = 7, RULE_normalTemplateBody = 8, RULE_normalTemplateString = 9, 
		RULE_conditionalTemplateBody = 10, RULE_caseRule = 11, RULE_defaultRule = 12, 
		RULE_caseCondition = 13, RULE_defaultCondition = 14;
	public static final String[] ruleNames = {
		"file", "paragraph", "newline", "templateDefinition", "templateNameLine", 
		"templateName", "parameters", "templateBody", "normalTemplateBody", "normalTemplateString", 
		"conditionalTemplateBody", "caseRule", "defaultRule", "caseCondition", 
		"defaultCondition"
	};

	private static final String[] _LITERAL_NAMES = {
		null, null, null, null, "'#'", null, null, null, "'.'", "'('", "')'", 
		"','"
	};
	private static final String[] _SYMBOLIC_NAMES = {
		null, "COMMENTS", "WS", "NEWLINE", "HASH", "DASH", "WS_IN_NAME", "IDENTIFIER", 
		"DOT", "OPEN_PARENTHESIS", "CLOSE_PARENTHESIS", "COMMA", "WS_IN_BODY_IGNORED", 
		"CASE", "DEFAULT", "MULTI_LINE_TEXT", "ESCAPE_CHARACTER", "INVALID_ESCAPE", 
		"EXPRESSION", "TEMPLATE_REF", "TEXT_SEPARATOR", "TEXT"
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
	public String getGrammarFileName() { return "LGFileParser.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public LGFileParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}
	public static class FileContext extends ParserRuleContext {
		public TerminalNode EOF() { return getToken(LGFileParser.EOF, 0); }
		public List<ParagraphContext> paragraph() {
			return getRuleContexts(ParagraphContext.class);
		}
		public ParagraphContext paragraph(int i) {
			return getRuleContext(ParagraphContext.class,i);
		}
		public FileContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_file; }
	}

	public final FileContext file() throws RecognitionException {
		FileContext _localctx = new FileContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_file);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(31); 
			_errHandler.sync(this);
			_alt = 1+1;
			do {
				switch (_alt) {
				case 1+1:
					{
					{
					setState(30);
					paragraph();
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(33); 
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,0,_ctx);
			} while ( _alt!=1 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER );
			setState(35);
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

	public static class ParagraphContext extends ParserRuleContext {
		public NewlineContext newline() {
			return getRuleContext(NewlineContext.class,0);
		}
		public TemplateDefinitionContext templateDefinition() {
			return getRuleContext(TemplateDefinitionContext.class,0);
		}
		public ParagraphContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_paragraph; }
	}

	public final ParagraphContext paragraph() throws RecognitionException {
		ParagraphContext _localctx = new ParagraphContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_paragraph);
		try {
			setState(39);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EOF:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(37);
				newline();
				}
				break;
			case HASH:
				enterOuterAlt(_localctx, 2);
				{
				setState(38);
				templateDefinition();
				}
				break;
			default:
				throw new NoViableAltException(this);
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

	public static class NewlineContext extends ParserRuleContext {
		public TerminalNode NEWLINE() { return getToken(LGFileParser.NEWLINE, 0); }
		public TerminalNode EOF() { return getToken(LGFileParser.EOF, 0); }
		public NewlineContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_newline; }
	}

	public final NewlineContext newline() throws RecognitionException {
		NewlineContext _localctx = new NewlineContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_newline);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(41);
			_la = _input.LA(1);
			if ( !(_la==EOF || _la==NEWLINE) ) {
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

	public static class TemplateDefinitionContext extends ParserRuleContext {
		public TemplateNameLineContext templateNameLine() {
			return getRuleContext(TemplateNameLineContext.class,0);
		}
		public NewlineContext newline() {
			return getRuleContext(NewlineContext.class,0);
		}
		public TemplateBodyContext templateBody() {
			return getRuleContext(TemplateBodyContext.class,0);
		}
		public TemplateDefinitionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_templateDefinition; }
	}

	public final TemplateDefinitionContext templateDefinition() throws RecognitionException {
		TemplateDefinitionContext _localctx = new TemplateDefinitionContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_templateDefinition);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(43);
			templateNameLine();
			setState(44);
			newline();
			setState(46);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==DASH) {
				{
				setState(45);
				templateBody();
				}
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

	public static class TemplateNameLineContext extends ParserRuleContext {
		public TerminalNode HASH() { return getToken(LGFileParser.HASH, 0); }
		public TemplateNameContext templateName() {
			return getRuleContext(TemplateNameContext.class,0);
		}
		public ParametersContext parameters() {
			return getRuleContext(ParametersContext.class,0);
		}
		public TemplateNameLineContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_templateNameLine; }
	}

	public final TemplateNameLineContext templateNameLine() throws RecognitionException {
		TemplateNameLineContext _localctx = new TemplateNameLineContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_templateNameLine);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(48);
			match(HASH);
			setState(49);
			templateName();
			setState(51);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==IDENTIFIER || _la==OPEN_PARENTHESIS) {
				{
				setState(50);
				parameters();
				}
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

	public static class TemplateNameContext extends ParserRuleContext {
		public List<TerminalNode> IDENTIFIER() { return getTokens(LGFileParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(LGFileParser.IDENTIFIER, i);
		}
		public List<TerminalNode> DOT() { return getTokens(LGFileParser.DOT); }
		public TerminalNode DOT(int i) {
			return getToken(LGFileParser.DOT, i);
		}
		public TemplateNameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_templateName; }
	}

	public final TemplateNameContext templateName() throws RecognitionException {
		TemplateNameContext _localctx = new TemplateNameContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_templateName);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(53);
			match(IDENTIFIER);
			setState(58);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DOT) {
				{
				{
				setState(54);
				match(DOT);
				setState(55);
				match(IDENTIFIER);
				}
				}
				setState(60);
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

	public static class ParametersContext extends ParserRuleContext {
		public List<TerminalNode> IDENTIFIER() { return getTokens(LGFileParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(LGFileParser.IDENTIFIER, i);
		}
		public TerminalNode OPEN_PARENTHESIS() { return getToken(LGFileParser.OPEN_PARENTHESIS, 0); }
		public List<TerminalNode> COMMA() { return getTokens(LGFileParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(LGFileParser.COMMA, i);
		}
		public TerminalNode CLOSE_PARENTHESIS() { return getToken(LGFileParser.CLOSE_PARENTHESIS, 0); }
		public ParametersContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_parameters; }
	}

	public final ParametersContext parameters() throws RecognitionException {
		ParametersContext _localctx = new ParametersContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_parameters);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(62);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==OPEN_PARENTHESIS) {
				{
				setState(61);
				match(OPEN_PARENTHESIS);
				}
			}

			setState(64);
			match(IDENTIFIER);
			setState(69);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(65);
				match(COMMA);
				setState(66);
				match(IDENTIFIER);
				}
				}
				setState(71);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(73);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==CLOSE_PARENTHESIS) {
				{
				setState(72);
				match(CLOSE_PARENTHESIS);
				}
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

	public static class TemplateBodyContext extends ParserRuleContext {
		public TemplateBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_templateBody; }
	 
		public TemplateBodyContext() { }
		public void copyFrom(TemplateBodyContext ctx) {
			super.copyFrom(ctx);
		}
	}
	public static class NormalBodyContext extends TemplateBodyContext {
		public NormalTemplateBodyContext normalTemplateBody() {
			return getRuleContext(NormalTemplateBodyContext.class,0);
		}
		public NormalBodyContext(TemplateBodyContext ctx) { copyFrom(ctx); }
	}
	public static class ConditionalBodyContext extends TemplateBodyContext {
		public ConditionalTemplateBodyContext conditionalTemplateBody() {
			return getRuleContext(ConditionalTemplateBodyContext.class,0);
		}
		public ConditionalBodyContext(TemplateBodyContext ctx) { copyFrom(ctx); }
	}

	public final TemplateBodyContext templateBody() throws RecognitionException {
		TemplateBodyContext _localctx = new TemplateBodyContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_templateBody);
		try {
			setState(77);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,8,_ctx) ) {
			case 1:
				_localctx = new NormalBodyContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(75);
				normalTemplateBody();
				}
				break;
			case 2:
				_localctx = new ConditionalBodyContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(76);
				conditionalTemplateBody();
				}
				break;
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

	public static class NormalTemplateBodyContext extends ParserRuleContext {
		public List<NormalTemplateStringContext> normalTemplateString() {
			return getRuleContexts(NormalTemplateStringContext.class);
		}
		public NormalTemplateStringContext normalTemplateString(int i) {
			return getRuleContext(NormalTemplateStringContext.class,i);
		}
		public List<NewlineContext> newline() {
			return getRuleContexts(NewlineContext.class);
		}
		public NewlineContext newline(int i) {
			return getRuleContext(NewlineContext.class,i);
		}
		public NormalTemplateBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_normalTemplateBody; }
	}

	public final NormalTemplateBodyContext normalTemplateBody() throws RecognitionException {
		NormalTemplateBodyContext _localctx = new NormalTemplateBodyContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_normalTemplateBody);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(82); 
			_errHandler.sync(this);
			_alt = 1;
			do {
				switch (_alt) {
				case 1:
					{
					{
					setState(79);
					normalTemplateString();
					setState(80);
					newline();
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(84); 
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,9,_ctx);
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

	public static class NormalTemplateStringContext extends ParserRuleContext {
		public TerminalNode DASH() { return getToken(LGFileParser.DASH, 0); }
		public List<TerminalNode> WS() { return getTokens(LGFileParser.WS); }
		public TerminalNode WS(int i) {
			return getToken(LGFileParser.WS, i);
		}
		public List<TerminalNode> TEXT() { return getTokens(LGFileParser.TEXT); }
		public TerminalNode TEXT(int i) {
			return getToken(LGFileParser.TEXT, i);
		}
		public List<TerminalNode> EXPRESSION() { return getTokens(LGFileParser.EXPRESSION); }
		public TerminalNode EXPRESSION(int i) {
			return getToken(LGFileParser.EXPRESSION, i);
		}
		public List<TerminalNode> TEMPLATE_REF() { return getTokens(LGFileParser.TEMPLATE_REF); }
		public TerminalNode TEMPLATE_REF(int i) {
			return getToken(LGFileParser.TEMPLATE_REF, i);
		}
		public List<TerminalNode> TEXT_SEPARATOR() { return getTokens(LGFileParser.TEXT_SEPARATOR); }
		public TerminalNode TEXT_SEPARATOR(int i) {
			return getToken(LGFileParser.TEXT_SEPARATOR, i);
		}
		public List<TerminalNode> MULTI_LINE_TEXT() { return getTokens(LGFileParser.MULTI_LINE_TEXT); }
		public TerminalNode MULTI_LINE_TEXT(int i) {
			return getToken(LGFileParser.MULTI_LINE_TEXT, i);
		}
		public List<TerminalNode> ESCAPE_CHARACTER() { return getTokens(LGFileParser.ESCAPE_CHARACTER); }
		public TerminalNode ESCAPE_CHARACTER(int i) {
			return getToken(LGFileParser.ESCAPE_CHARACTER, i);
		}
		public List<TerminalNode> INVALID_ESCAPE() { return getTokens(LGFileParser.INVALID_ESCAPE); }
		public TerminalNode INVALID_ESCAPE(int i) {
			return getToken(LGFileParser.INVALID_ESCAPE, i);
		}
		public NormalTemplateStringContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_normalTemplateString; }
	}

	public final NormalTemplateStringContext normalTemplateString() throws RecognitionException {
		NormalTemplateStringContext _localctx = new NormalTemplateStringContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_normalTemplateString);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(86);
			match(DASH);
			setState(90);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << MULTI_LINE_TEXT) | (1L << ESCAPE_CHARACTER) | (1L << INVALID_ESCAPE) | (1L << EXPRESSION) | (1L << TEMPLATE_REF) | (1L << TEXT_SEPARATOR) | (1L << TEXT))) != 0)) {
				{
				{
				setState(87);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << MULTI_LINE_TEXT) | (1L << ESCAPE_CHARACTER) | (1L << INVALID_ESCAPE) | (1L << EXPRESSION) | (1L << TEMPLATE_REF) | (1L << TEXT_SEPARATOR) | (1L << TEXT))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				}
				}
				setState(92);
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

	public static class ConditionalTemplateBodyContext extends ParserRuleContext {
		public List<CaseRuleContext> caseRule() {
			return getRuleContexts(CaseRuleContext.class);
		}
		public CaseRuleContext caseRule(int i) {
			return getRuleContext(CaseRuleContext.class,i);
		}
		public DefaultRuleContext defaultRule() {
			return getRuleContext(DefaultRuleContext.class,0);
		}
		public ConditionalTemplateBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_conditionalTemplateBody; }
	}

	public final ConditionalTemplateBodyContext conditionalTemplateBody() throws RecognitionException {
		ConditionalTemplateBodyContext _localctx = new ConditionalTemplateBodyContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_conditionalTemplateBody);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(94); 
			_errHandler.sync(this);
			_alt = 1;
			do {
				switch (_alt) {
				case 1:
					{
					{
					setState(93);
					caseRule();
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(96); 
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,11,_ctx);
			} while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER );
			setState(99);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==DASH) {
				{
				setState(98);
				defaultRule();
				}
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

	public static class CaseRuleContext extends ParserRuleContext {
		public CaseConditionContext caseCondition() {
			return getRuleContext(CaseConditionContext.class,0);
		}
		public NewlineContext newline() {
			return getRuleContext(NewlineContext.class,0);
		}
		public NormalTemplateBodyContext normalTemplateBody() {
			return getRuleContext(NormalTemplateBodyContext.class,0);
		}
		public CaseRuleContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_caseRule; }
	}

	public final CaseRuleContext caseRule() throws RecognitionException {
		CaseRuleContext _localctx = new CaseRuleContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_caseRule);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(101);
			caseCondition();
			setState(102);
			newline();
			setState(104);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,13,_ctx) ) {
			case 1:
				{
				setState(103);
				normalTemplateBody();
				}
				break;
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

	public static class DefaultRuleContext extends ParserRuleContext {
		public DefaultConditionContext defaultCondition() {
			return getRuleContext(DefaultConditionContext.class,0);
		}
		public NewlineContext newline() {
			return getRuleContext(NewlineContext.class,0);
		}
		public NormalTemplateBodyContext normalTemplateBody() {
			return getRuleContext(NormalTemplateBodyContext.class,0);
		}
		public DefaultRuleContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_defaultRule; }
	}

	public final DefaultRuleContext defaultRule() throws RecognitionException {
		DefaultRuleContext _localctx = new DefaultRuleContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_defaultRule);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(106);
			defaultCondition();
			setState(107);
			newline();
			setState(109);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==DASH) {
				{
				setState(108);
				normalTemplateBody();
				}
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

	public static class CaseConditionContext extends ParserRuleContext {
		public TerminalNode DASH() { return getToken(LGFileParser.DASH, 0); }
		public TerminalNode CASE() { return getToken(LGFileParser.CASE, 0); }
		public List<TerminalNode> WS() { return getTokens(LGFileParser.WS); }
		public TerminalNode WS(int i) {
			return getToken(LGFileParser.WS, i);
		}
		public List<TerminalNode> TEXT() { return getTokens(LGFileParser.TEXT); }
		public TerminalNode TEXT(int i) {
			return getToken(LGFileParser.TEXT, i);
		}
		public List<TerminalNode> EXPRESSION() { return getTokens(LGFileParser.EXPRESSION); }
		public TerminalNode EXPRESSION(int i) {
			return getToken(LGFileParser.EXPRESSION, i);
		}
		public CaseConditionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_caseCondition; }
	}

	public final CaseConditionContext caseCondition() throws RecognitionException {
		CaseConditionContext _localctx = new CaseConditionContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_caseCondition);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(111);
			match(DASH);
			setState(112);
			match(CASE);
			setState(116);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << EXPRESSION) | (1L << TEXT))) != 0)) {
				{
				{
				setState(113);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << EXPRESSION) | (1L << TEXT))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				}
				}
				setState(118);
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

	public static class DefaultConditionContext extends ParserRuleContext {
		public TerminalNode DASH() { return getToken(LGFileParser.DASH, 0); }
		public TerminalNode DEFAULT() { return getToken(LGFileParser.DEFAULT, 0); }
		public DefaultConditionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_defaultCondition; }
	}

	public final DefaultConditionContext defaultCondition() throws RecognitionException {
		DefaultConditionContext _localctx = new DefaultConditionContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_defaultCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(119);
			match(DASH);
			setState(120);
			match(DEFAULT);
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

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3\27}\4\2\t\2\4\3\t"+
		"\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13\t\13\4"+
		"\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\3\2\6\2\"\n\2\r\2\16\2#\3"+
		"\2\3\2\3\3\3\3\5\3*\n\3\3\4\3\4\3\5\3\5\3\5\5\5\61\n\5\3\6\3\6\3\6\5\6"+
		"\66\n\6\3\7\3\7\3\7\7\7;\n\7\f\7\16\7>\13\7\3\b\5\bA\n\b\3\b\3\b\3\b\7"+
		"\bF\n\b\f\b\16\bI\13\b\3\b\5\bL\n\b\3\t\3\t\5\tP\n\t\3\n\3\n\3\n\6\nU"+
		"\n\n\r\n\16\nV\3\13\3\13\7\13[\n\13\f\13\16\13^\13\13\3\f\6\fa\n\f\r\f"+
		"\16\fb\3\f\5\ff\n\f\3\r\3\r\3\r\5\rk\n\r\3\16\3\16\3\16\5\16p\n\16\3\17"+
		"\3\17\3\17\7\17u\n\17\f\17\16\17x\13\17\3\20\3\20\3\20\3\20\3#\2\21\2"+
		"\4\6\b\n\f\16\20\22\24\26\30\32\34\36\2\5\3\3\5\5\4\2\4\4\21\27\5\2\4"+
		"\4\24\24\27\27\2}\2!\3\2\2\2\4)\3\2\2\2\6+\3\2\2\2\b-\3\2\2\2\n\62\3\2"+
		"\2\2\f\67\3\2\2\2\16@\3\2\2\2\20O\3\2\2\2\22T\3\2\2\2\24X\3\2\2\2\26`"+
		"\3\2\2\2\30g\3\2\2\2\32l\3\2\2\2\34q\3\2\2\2\36y\3\2\2\2 \"\5\4\3\2! "+
		"\3\2\2\2\"#\3\2\2\2#$\3\2\2\2#!\3\2\2\2$%\3\2\2\2%&\7\2\2\3&\3\3\2\2\2"+
		"\'*\5\6\4\2(*\5\b\5\2)\'\3\2\2\2)(\3\2\2\2*\5\3\2\2\2+,\t\2\2\2,\7\3\2"+
		"\2\2-.\5\n\6\2.\60\5\6\4\2/\61\5\20\t\2\60/\3\2\2\2\60\61\3\2\2\2\61\t"+
		"\3\2\2\2\62\63\7\6\2\2\63\65\5\f\7\2\64\66\5\16\b\2\65\64\3\2\2\2\65\66"+
		"\3\2\2\2\66\13\3\2\2\2\67<\7\t\2\289\7\n\2\29;\7\t\2\2:8\3\2\2\2;>\3\2"+
		"\2\2<:\3\2\2\2<=\3\2\2\2=\r\3\2\2\2><\3\2\2\2?A\7\13\2\2@?\3\2\2\2@A\3"+
		"\2\2\2AB\3\2\2\2BG\7\t\2\2CD\7\r\2\2DF\7\t\2\2EC\3\2\2\2FI\3\2\2\2GE\3"+
		"\2\2\2GH\3\2\2\2HK\3\2\2\2IG\3\2\2\2JL\7\f\2\2KJ\3\2\2\2KL\3\2\2\2L\17"+
		"\3\2\2\2MP\5\22\n\2NP\5\26\f\2OM\3\2\2\2ON\3\2\2\2P\21\3\2\2\2QR\5\24"+
		"\13\2RS\5\6\4\2SU\3\2\2\2TQ\3\2\2\2UV\3\2\2\2VT\3\2\2\2VW\3\2\2\2W\23"+
		"\3\2\2\2X\\\7\7\2\2Y[\t\3\2\2ZY\3\2\2\2[^\3\2\2\2\\Z\3\2\2\2\\]\3\2\2"+
		"\2]\25\3\2\2\2^\\\3\2\2\2_a\5\30\r\2`_\3\2\2\2ab\3\2\2\2b`\3\2\2\2bc\3"+
		"\2\2\2ce\3\2\2\2df\5\32\16\2ed\3\2\2\2ef\3\2\2\2f\27\3\2\2\2gh\5\34\17"+
		"\2hj\5\6\4\2ik\5\22\n\2ji\3\2\2\2jk\3\2\2\2k\31\3\2\2\2lm\5\36\20\2mo"+
		"\5\6\4\2np\5\22\n\2on\3\2\2\2op\3\2\2\2p\33\3\2\2\2qr\7\7\2\2rv\7\17\2"+
		"\2su\t\4\2\2ts\3\2\2\2ux\3\2\2\2vt\3\2\2\2vw\3\2\2\2w\35\3\2\2\2xv\3\2"+
		"\2\2yz\7\7\2\2z{\7\20\2\2{\37\3\2\2\2\22#)\60\65<@GKOV\\bejov";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}