// Generated from d:\project\botframework\botbuilder-dotnet\libraries\Microsoft.Bot.Builder.LanguageGeneration\LGFileParser.g4 by ANTLR 4.7.1
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
		NEWLINE=1, OPTION=2, COMMENT=3, IMPORT=4, TEMPLATE_NAME_LINE=5, TEMPLATE_BODY_LINE=6, 
		INVALID_LINE=7;
	public static final int
		RULE_file = 0, RULE_paragraph = 1, RULE_commentDefinition = 2, RULE_importDefinition = 3, 
		RULE_optionDefinition = 4, RULE_errorDefinition = 5, RULE_templateDefinition = 6, 
		RULE_templateNameLine = 7, RULE_templateBody = 8, RULE_templateBodyLine = 9;
	public static final String[] ruleNames = {
		"file", "paragraph", "commentDefinition", "importDefinition", "optionDefinition", 
		"errorDefinition", "templateDefinition", "templateNameLine", "templateBody", 
		"templateBodyLine"
	};

	private static final String[] _LITERAL_NAMES = {
	};
	private static final String[] _SYMBOLIC_NAMES = {
		null, "NEWLINE", "OPTION", "COMMENT", "IMPORT", "TEMPLATE_NAME_LINE", 
		"TEMPLATE_BODY_LINE", "INVALID_LINE"
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
			setState(21); 
			_errHandler.sync(this);
			_alt = 1+1;
			do {
				switch (_alt) {
				case 1+1:
					{
					{
					setState(20);
					paragraph();
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(23); 
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,0,_ctx);
			} while ( _alt!=1 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER );
			setState(25);
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
		public TemplateDefinitionContext templateDefinition() {
			return getRuleContext(TemplateDefinitionContext.class,0);
		}
		public ImportDefinitionContext importDefinition() {
			return getRuleContext(ImportDefinitionContext.class,0);
		}
		public OptionDefinitionContext optionDefinition() {
			return getRuleContext(OptionDefinitionContext.class,0);
		}
		public ErrorDefinitionContext errorDefinition() {
			return getRuleContext(ErrorDefinitionContext.class,0);
		}
		public CommentDefinitionContext commentDefinition() {
			return getRuleContext(CommentDefinitionContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(LGFileParser.NEWLINE, 0); }
		public TerminalNode EOF() { return getToken(LGFileParser.EOF, 0); }
		public ParagraphContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_paragraph; }
	}

	public final ParagraphContext paragraph() throws RecognitionException {
		ParagraphContext _localctx = new ParagraphContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_paragraph);
		try {
			setState(34);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case TEMPLATE_NAME_LINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(27);
				templateDefinition();
				}
				break;
			case IMPORT:
				enterOuterAlt(_localctx, 2);
				{
				setState(28);
				importDefinition();
				}
				break;
			case OPTION:
				enterOuterAlt(_localctx, 3);
				{
				setState(29);
				optionDefinition();
				}
				break;
			case INVALID_LINE:
				enterOuterAlt(_localctx, 4);
				{
				setState(30);
				errorDefinition();
				}
				break;
			case COMMENT:
				enterOuterAlt(_localctx, 5);
				{
				setState(31);
				commentDefinition();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 6);
				{
				setState(32);
				match(NEWLINE);
				}
				break;
			case EOF:
				enterOuterAlt(_localctx, 7);
				{
				setState(33);
				match(EOF);
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

	public static class CommentDefinitionContext extends ParserRuleContext {
		public TerminalNode COMMENT() { return getToken(LGFileParser.COMMENT, 0); }
		public TerminalNode NEWLINE() { return getToken(LGFileParser.NEWLINE, 0); }
		public CommentDefinitionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_commentDefinition; }
	}

	public final CommentDefinitionContext commentDefinition() throws RecognitionException {
		CommentDefinitionContext _localctx = new CommentDefinitionContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_commentDefinition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(36);
			match(COMMENT);
			setState(38);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,2,_ctx) ) {
			case 1:
				{
				setState(37);
				match(NEWLINE);
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

	public static class ImportDefinitionContext extends ParserRuleContext {
		public TerminalNode IMPORT() { return getToken(LGFileParser.IMPORT, 0); }
		public TerminalNode NEWLINE() { return getToken(LGFileParser.NEWLINE, 0); }
		public ImportDefinitionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_importDefinition; }
	}

	public final ImportDefinitionContext importDefinition() throws RecognitionException {
		ImportDefinitionContext _localctx = new ImportDefinitionContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_importDefinition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(40);
			match(IMPORT);
			setState(42);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,3,_ctx) ) {
			case 1:
				{
				setState(41);
				match(NEWLINE);
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

	public static class OptionDefinitionContext extends ParserRuleContext {
		public TerminalNode OPTION() { return getToken(LGFileParser.OPTION, 0); }
		public TerminalNode NEWLINE() { return getToken(LGFileParser.NEWLINE, 0); }
		public OptionDefinitionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_optionDefinition; }
	}

	public final OptionDefinitionContext optionDefinition() throws RecognitionException {
		OptionDefinitionContext _localctx = new OptionDefinitionContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_optionDefinition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(44);
			match(OPTION);
			setState(46);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,4,_ctx) ) {
			case 1:
				{
				setState(45);
				match(NEWLINE);
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

	public static class ErrorDefinitionContext extends ParserRuleContext {
		public TerminalNode INVALID_LINE() { return getToken(LGFileParser.INVALID_LINE, 0); }
		public TerminalNode NEWLINE() { return getToken(LGFileParser.NEWLINE, 0); }
		public ErrorDefinitionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_errorDefinition; }
	}

	public final ErrorDefinitionContext errorDefinition() throws RecognitionException {
		ErrorDefinitionContext _localctx = new ErrorDefinitionContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_errorDefinition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(48);
			match(INVALID_LINE);
			setState(50);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,5,_ctx) ) {
			case 1:
				{
				setState(49);
				match(NEWLINE);
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

	public static class TemplateDefinitionContext extends ParserRuleContext {
		public TemplateNameLineContext templateNameLine() {
			return getRuleContext(TemplateNameLineContext.class,0);
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
		enterRule(_localctx, 12, RULE_templateDefinition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(52);
			templateNameLine();
			setState(53);
			templateBody();
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
		public TerminalNode TEMPLATE_NAME_LINE() { return getToken(LGFileParser.TEMPLATE_NAME_LINE, 0); }
		public TerminalNode NEWLINE() { return getToken(LGFileParser.NEWLINE, 0); }
		public TemplateNameLineContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_templateNameLine; }
	}

	public final TemplateNameLineContext templateNameLine() throws RecognitionException {
		TemplateNameLineContext _localctx = new TemplateNameLineContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_templateNameLine);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(55);
			match(TEMPLATE_NAME_LINE);
			setState(57);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,6,_ctx) ) {
			case 1:
				{
				setState(56);
				match(NEWLINE);
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

	public static class TemplateBodyContext extends ParserRuleContext {
		public List<TemplateBodyLineContext> templateBodyLine() {
			return getRuleContexts(TemplateBodyLineContext.class);
		}
		public TemplateBodyLineContext templateBodyLine(int i) {
			return getRuleContext(TemplateBodyLineContext.class,i);
		}
		public TemplateBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_templateBody; }
	}

	public final TemplateBodyContext templateBody() throws RecognitionException {
		TemplateBodyContext _localctx = new TemplateBodyContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_templateBody);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(62);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,7,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(59);
					templateBodyLine();
					}
					} 
				}
				setState(64);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,7,_ctx);
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

	public static class TemplateBodyLineContext extends ParserRuleContext {
		public TerminalNode TEMPLATE_BODY_LINE() { return getToken(LGFileParser.TEMPLATE_BODY_LINE, 0); }
		public TerminalNode NEWLINE() { return getToken(LGFileParser.NEWLINE, 0); }
		public TemplateBodyLineContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_templateBodyLine; }
	}

	public final TemplateBodyLineContext templateBodyLine() throws RecognitionException {
		TemplateBodyLineContext _localctx = new TemplateBodyLineContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_templateBodyLine);
		try {
			setState(70);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case TEMPLATE_BODY_LINE:
				enterOuterAlt(_localctx, 1);
				{
				{
				setState(65);
				match(TEMPLATE_BODY_LINE);
				setState(67);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,8,_ctx) ) {
				case 1:
					{
					setState(66);
					match(NEWLINE);
					}
					break;
				}
				}
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(69);
				match(NEWLINE);
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

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3\tK\4\2\t\2\4\3\t"+
		"\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13\t\13\3"+
		"\2\6\2\30\n\2\r\2\16\2\31\3\2\3\2\3\3\3\3\3\3\3\3\3\3\3\3\3\3\5\3%\n\3"+
		"\3\4\3\4\5\4)\n\4\3\5\3\5\5\5-\n\5\3\6\3\6\5\6\61\n\6\3\7\3\7\5\7\65\n"+
		"\7\3\b\3\b\3\b\3\t\3\t\5\t<\n\t\3\n\7\n?\n\n\f\n\16\nB\13\n\3\13\3\13"+
		"\5\13F\n\13\3\13\5\13I\n\13\3\13\3\31\2\f\2\4\6\b\n\f\16\20\22\24\2\2"+
		"\2O\2\27\3\2\2\2\4$\3\2\2\2\6&\3\2\2\2\b*\3\2\2\2\n.\3\2\2\2\f\62\3\2"+
		"\2\2\16\66\3\2\2\2\209\3\2\2\2\22@\3\2\2\2\24H\3\2\2\2\26\30\5\4\3\2\27"+
		"\26\3\2\2\2\30\31\3\2\2\2\31\32\3\2\2\2\31\27\3\2\2\2\32\33\3\2\2\2\33"+
		"\34\7\2\2\3\34\3\3\2\2\2\35%\5\16\b\2\36%\5\b\5\2\37%\5\n\6\2 %\5\f\7"+
		"\2!%\5\6\4\2\"%\7\3\2\2#%\7\2\2\3$\35\3\2\2\2$\36\3\2\2\2$\37\3\2\2\2"+
		"$ \3\2\2\2$!\3\2\2\2$\"\3\2\2\2$#\3\2\2\2%\5\3\2\2\2&(\7\5\2\2\')\7\3"+
		"\2\2(\'\3\2\2\2()\3\2\2\2)\7\3\2\2\2*,\7\6\2\2+-\7\3\2\2,+\3\2\2\2,-\3"+
		"\2\2\2-\t\3\2\2\2.\60\7\4\2\2/\61\7\3\2\2\60/\3\2\2\2\60\61\3\2\2\2\61"+
		"\13\3\2\2\2\62\64\7\t\2\2\63\65\7\3\2\2\64\63\3\2\2\2\64\65\3\2\2\2\65"+
		"\r\3\2\2\2\66\67\5\20\t\2\678\5\22\n\28\17\3\2\2\29;\7\7\2\2:<\7\3\2\2"+
		";:\3\2\2\2;<\3\2\2\2<\21\3\2\2\2=?\5\24\13\2>=\3\2\2\2?B\3\2\2\2@>\3\2"+
		"\2\2@A\3\2\2\2A\23\3\2\2\2B@\3\2\2\2CE\7\b\2\2DF\7\3\2\2ED\3\2\2\2EF\3"+
		"\2\2\2FI\3\2\2\2GI\7\3\2\2HC\3\2\2\2HG\3\2\2\2I\25\3\2\2\2\f\31$(,\60"+
		"\64;@EH";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}