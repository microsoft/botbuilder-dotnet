// Generated from d:\projects\BotFramework\myfork\botbuilder-dotnet\libraries\Microsoft.Bot.Builder.AI.LanguageGeneration/LGFileLexer.g4 by ANTLR 4.7.1
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class LGFileLexer extends Lexer {
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
		TEMPLATE_NAME_MODE=1, TEMPLATE_BODY_MODE=2;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE", "TEMPLATE_NAME_MODE", "TEMPLATE_BODY_MODE"
	};

	public static final String[] ruleNames = {
		"LETTER", "NUMBER", "COMMENTS", "WS", "NEWLINE", "HASH", "DASH", "WS_IN_NAME", 
		"NEWLINE_IN_NAME", "IDENTIFIER", "DOT", "OPEN_PARENTHESIS", "CLOSE_PARENTHESIS", 
		"COMMA", "WS_IN_BODY_IGNORED", "WS_IN_BODY", "NEWLINE_IN_BODY", "CASE", 
		"DEFAULT", "MULTI_LINE_TEXT", "ESCAPE_CHARACTER", "INVALID_ESCAPE", "EXPRESSION", 
		"TEMPLATE_REF", "TEXT_SEPARATOR", "TEXT"
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


	  bool ignoreWS = true;             // usually we ignore whitespace, but inside template, whitespace is significant
	  bool expectCaseOrDefault = false; // whethe we are expecting CASE: or DEFAULT:


	public LGFileLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "LGFileLexer.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	@Override
	public void action(RuleContext _localctx, int ruleIndex, int actionIndex) {
		switch (ruleIndex) {
		case 6:
			DASH_action((RuleContext)_localctx, actionIndex);
			break;
		case 16:
			NEWLINE_IN_BODY_action((RuleContext)_localctx, actionIndex);
			break;
		case 17:
			CASE_action((RuleContext)_localctx, actionIndex);
			break;
		case 18:
			DEFAULT_action((RuleContext)_localctx, actionIndex);
			break;
		case 19:
			MULTI_LINE_TEXT_action((RuleContext)_localctx, actionIndex);
			break;
		case 20:
			ESCAPE_CHARACTER_action((RuleContext)_localctx, actionIndex);
			break;
		case 22:
			EXPRESSION_action((RuleContext)_localctx, actionIndex);
			break;
		case 23:
			TEMPLATE_REF_action((RuleContext)_localctx, actionIndex);
			break;
		case 24:
			TEXT_SEPARATOR_action((RuleContext)_localctx, actionIndex);
			break;
		case 25:
			TEXT_action((RuleContext)_localctx, actionIndex);
			break;
		}
	}
	private void DASH_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 0:
			expectCaseOrDefault = true;
			break;
		}
	}
	private void NEWLINE_IN_BODY_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 1:
			ignoreWS = true;
			break;
		}
	}
	private void CASE_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 2:
			 ignoreWS = true;
			break;
		}
	}
	private void DEFAULT_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 3:
			 ignoreWS = true;
			break;
		}
	}
	private void MULTI_LINE_TEXT_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 4:
			 ignoreWS = false; expectCaseOrDefault = false;
			break;
		}
	}
	private void ESCAPE_CHARACTER_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 5:
			 ignoreWS = false; expectCaseOrDefault = false;
			break;
		}
	}
	private void EXPRESSION_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 6:
			 ignoreWS = false; expectCaseOrDefault = false;
			break;
		}
	}
	private void TEMPLATE_REF_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 7:
			 ignoreWS = false; expectCaseOrDefault = false;
			break;
		}
	}
	private void TEXT_SEPARATOR_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 8:
			 ignoreWS = false; expectCaseOrDefault = false;
			break;
		}
	}
	private void TEXT_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 9:
			 ignoreWS = false; expectCaseOrDefault = false;
			break;
		}
	}
	@Override
	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 14:
			return WS_IN_BODY_IGNORED_sempred((RuleContext)_localctx, predIndex);
		case 17:
			return CASE_sempred((RuleContext)_localctx, predIndex);
		case 18:
			return DEFAULT_sempred((RuleContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean WS_IN_BODY_IGNORED_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return ignoreWS;
		}
		return true;
	}
	private boolean CASE_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 1:
			return expectCaseOrDefault;
		}
		return true;
	}
	private boolean DEFAULT_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 2:
			return expectCaseOrDefault;
		}
		return true;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\27\u00fb\b\1\b\1"+
		"\b\1\4\2\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4"+
		"\n\t\n\4\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t"+
		"\21\4\22\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t"+
		"\30\4\31\t\31\4\32\t\32\4\33\t\33\3\2\3\2\3\3\3\3\3\4\3\4\6\4@\n\4\r\4"+
		"\16\4A\3\4\3\4\3\5\6\5G\n\5\r\5\16\5H\3\5\3\5\3\6\5\6N\n\6\3\6\3\6\3\6"+
		"\3\6\3\7\3\7\3\7\3\7\3\b\3\b\3\b\3\b\3\b\3\t\6\t^\n\t\r\t\16\t_\3\t\3"+
		"\t\3\n\5\ne\n\n\3\n\3\n\3\n\3\n\3\n\3\13\3\13\3\13\5\13o\n\13\3\13\3\13"+
		"\3\13\7\13t\n\13\f\13\16\13w\13\13\3\f\3\f\3\r\3\r\3\16\3\16\3\17\3\17"+
		"\3\20\6\20\u0082\n\20\r\20\16\20\u0083\3\20\3\20\3\20\3\20\3\21\6\21\u008b"+
		"\n\21\r\21\16\21\u008c\3\21\3\21\3\22\5\22\u0092\n\22\3\22\3\22\3\22\3"+
		"\22\3\22\3\22\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\5\23\u00a4"+
		"\n\23\3\23\3\23\3\23\3\24\3\24\3\24\3\24\3\24\3\24\3\24\3\24\3\24\3\24"+
		"\3\24\3\24\3\24\3\24\3\24\3\24\5\24\u00b9\n\24\3\24\3\24\3\24\3\25\3\25"+
		"\3\25\3\25\3\25\7\25\u00c3\n\25\f\25\16\25\u00c6\13\25\3\25\3\25\3\25"+
		"\3\25\3\25\3\25\3\26\3\26\3\26\3\26\3\26\3\26\3\26\3\26\3\26\5\26\u00d7"+
		"\n\26\3\27\3\27\5\27\u00db\n\27\3\30\3\30\7\30\u00df\n\30\f\30\16\30\u00e2"+
		"\13\30\3\30\3\30\3\30\3\31\3\31\3\31\7\31\u00ea\n\31\f\31\16\31\u00ed"+
		"\13\31\3\31\3\31\3\31\3\32\3\32\3\32\3\33\6\33\u00f6\n\33\r\33\16\33\u00f7"+
		"\3\33\3\33\3\u00c4\2\34\5\2\7\2\t\3\13\4\r\5\17\6\21\7\23\b\25\2\27\t"+
		"\31\n\33\13\35\f\37\r!\16#\2%\2\'\17)\20+\21-\22/\23\61\24\63\25\65\26"+
		"\67\27\5\2\3\4\f\4\2C\\c|\4\2&&@@\4\2\f\f\17\17\4\2\13\13\"\"\4\2//aa"+
		"\7\2__ppttvv\177\177\6\2\f\f\17\17}}\177\177\5\2\f\f\17\17__\n\2\13\f"+
		"\17\17\"\"*+]]__}}\177\177\t\2\13\f\17\17\"\"*+]_}}\177\177\2\u010e\2"+
		"\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2\3\23\3\2"+
		"\2\2\3\25\3\2\2\2\3\27\3\2\2\2\3\31\3\2\2\2\3\33\3\2\2\2\3\35\3\2\2\2"+
		"\3\37\3\2\2\2\4!\3\2\2\2\4#\3\2\2\2\4%\3\2\2\2\4\'\3\2\2\2\4)\3\2\2\2"+
		"\4+\3\2\2\2\4-\3\2\2\2\4/\3\2\2\2\4\61\3\2\2\2\4\63\3\2\2\2\4\65\3\2\2"+
		"\2\4\67\3\2\2\2\59\3\2\2\2\7;\3\2\2\2\t=\3\2\2\2\13F\3\2\2\2\rM\3\2\2"+
		"\2\17S\3\2\2\2\21W\3\2\2\2\23]\3\2\2\2\25d\3\2\2\2\27n\3\2\2\2\31x\3\2"+
		"\2\2\33z\3\2\2\2\35|\3\2\2\2\37~\3\2\2\2!\u0081\3\2\2\2#\u008a\3\2\2\2"+
		"%\u0091\3\2\2\2\'\u00a3\3\2\2\2)\u00b8\3\2\2\2+\u00bd\3\2\2\2-\u00d6\3"+
		"\2\2\2/\u00d8\3\2\2\2\61\u00dc\3\2\2\2\63\u00e6\3\2\2\2\65\u00f1\3\2\2"+
		"\2\67\u00f5\3\2\2\29:\t\2\2\2:\6\3\2\2\2;<\4\62;\2<\b\3\2\2\2=?\t\3\2"+
		"\2>@\n\4\2\2?>\3\2\2\2@A\3\2\2\2A?\3\2\2\2AB\3\2\2\2BC\3\2\2\2CD\b\4\2"+
		"\2D\n\3\2\2\2EG\t\5\2\2FE\3\2\2\2GH\3\2\2\2HF\3\2\2\2HI\3\2\2\2IJ\3\2"+
		"\2\2JK\b\5\2\2K\f\3\2\2\2LN\7\17\2\2ML\3\2\2\2MN\3\2\2\2NO\3\2\2\2OP\7"+
		"\f\2\2PQ\3\2\2\2QR\b\6\2\2R\16\3\2\2\2ST\7%\2\2TU\3\2\2\2UV\b\7\3\2V\20"+
		"\3\2\2\2WX\7/\2\2XY\b\b\4\2YZ\3\2\2\2Z[\b\b\5\2[\22\3\2\2\2\\^\t\5\2\2"+
		"]\\\3\2\2\2^_\3\2\2\2_]\3\2\2\2_`\3\2\2\2`a\3\2\2\2ab\b\t\2\2b\24\3\2"+
		"\2\2ce\7\17\2\2dc\3\2\2\2de\3\2\2\2ef\3\2\2\2fg\7\f\2\2gh\3\2\2\2hi\b"+
		"\n\6\2ij\b\n\7\2j\26\3\2\2\2ko\5\5\2\2lo\5\7\3\2mo\7a\2\2nk\3\2\2\2nl"+
		"\3\2\2\2nm\3\2\2\2ou\3\2\2\2pt\5\5\2\2qt\5\7\3\2rt\t\6\2\2sp\3\2\2\2s"+
		"q\3\2\2\2sr\3\2\2\2tw\3\2\2\2us\3\2\2\2uv\3\2\2\2v\30\3\2\2\2wu\3\2\2"+
		"\2xy\7\60\2\2y\32\3\2\2\2z{\7*\2\2{\34\3\2\2\2|}\7+\2\2}\36\3\2\2\2~\177"+
		"\7.\2\2\177 \3\2\2\2\u0080\u0082\t\5\2\2\u0081\u0080\3\2\2\2\u0082\u0083"+
		"\3\2\2\2\u0083\u0081\3\2\2\2\u0083\u0084\3\2\2\2\u0084\u0085\3\2\2\2\u0085"+
		"\u0086\6\20\2\2\u0086\u0087\3\2\2\2\u0087\u0088\b\20\2\2\u0088\"\3\2\2"+
		"\2\u0089\u008b\t\5\2\2\u008a\u0089\3\2\2\2\u008b\u008c\3\2\2\2\u008c\u008a"+
		"\3\2\2\2\u008c\u008d\3\2\2\2\u008d\u008e\3\2\2\2\u008e\u008f\b\21\b\2"+
		"\u008f$\3\2\2\2\u0090\u0092\7\17\2\2\u0091\u0090\3\2\2\2\u0091\u0092\3"+
		"\2\2\2\u0092\u0093\3\2\2\2\u0093\u0094\7\f\2\2\u0094\u0095\b\22\t\2\u0095"+
		"\u0096\3\2\2\2\u0096\u0097\b\22\6\2\u0097\u0098\b\22\7\2\u0098&\3\2\2"+
		"\2\u0099\u009a\7e\2\2\u009a\u009b\7c\2\2\u009b\u009c\7u\2\2\u009c\u009d"+
		"\7g\2\2\u009d\u00a4\7<\2\2\u009e\u009f\7E\2\2\u009f\u00a0\7C\2\2\u00a0"+
		"\u00a1\7U\2\2\u00a1\u00a2\7G\2\2\u00a2\u00a4\7<\2\2\u00a3\u0099\3\2\2"+
		"\2\u00a3\u009e\3\2\2\2\u00a4\u00a5\3\2\2\2\u00a5\u00a6\6\23\3\2\u00a6"+
		"\u00a7\b\23\n\2\u00a7(\3\2\2\2\u00a8\u00a9\7f\2\2\u00a9\u00aa\7g\2\2\u00aa"+
		"\u00ab\7h\2\2\u00ab\u00ac\7c\2\2\u00ac\u00ad\7w\2\2\u00ad\u00ae\7n\2\2"+
		"\u00ae\u00af\7v\2\2\u00af\u00b9\7<\2\2\u00b0\u00b1\7F\2\2\u00b1\u00b2"+
		"\7G\2\2\u00b2\u00b3\7H\2\2\u00b3\u00b4\7C\2\2\u00b4\u00b5\7W\2\2\u00b5"+
		"\u00b6\7N\2\2\u00b6\u00b7\7V\2\2\u00b7\u00b9\7<\2\2\u00b8\u00a8\3\2\2"+
		"\2\u00b8\u00b0\3\2\2\2\u00b9\u00ba\3\2\2\2\u00ba\u00bb\6\24\4\2\u00bb"+
		"\u00bc\b\24\13\2\u00bc*\3\2\2\2\u00bd\u00be\7b\2\2\u00be\u00bf\7b\2\2"+
		"\u00bf\u00c0\7b\2\2\u00c0\u00c4\3\2\2\2\u00c1\u00c3\13\2\2\2\u00c2\u00c1"+
		"\3\2\2\2\u00c3\u00c6\3\2\2\2\u00c4\u00c5\3\2\2\2\u00c4\u00c2\3\2\2\2\u00c5"+
		"\u00c7\3\2\2\2\u00c6\u00c4\3\2\2\2\u00c7\u00c8\7b\2\2\u00c8\u00c9\7b\2"+
		"\2\u00c9\u00ca\7b\2\2\u00ca\u00cb\3\2\2\2\u00cb\u00cc\b\25\f\2\u00cc,"+
		"\3\2\2\2\u00cd\u00ce\7^\2\2\u00ce\u00d7\7}\2\2\u00cf\u00d0\7^\2\2\u00d0"+
		"\u00d7\7]\2\2\u00d1\u00d2\7^\2\2\u00d2\u00d7\7^\2\2\u00d3\u00d4\7^\2\2"+
		"\u00d4\u00d5\t\7\2\2\u00d5\u00d7\b\26\r\2\u00d6\u00cd\3\2\2\2\u00d6\u00cf"+
		"\3\2\2\2\u00d6\u00d1\3\2\2\2\u00d6\u00d3\3\2\2\2\u00d7.\3\2\2\2\u00d8"+
		"\u00da\7^\2\2\u00d9\u00db\n\4\2\2\u00da\u00d9\3\2\2\2\u00da\u00db\3\2"+
		"\2\2\u00db\60\3\2\2\2\u00dc\u00e0\7}\2\2\u00dd\u00df\n\b\2\2\u00de\u00dd"+
		"\3\2\2\2\u00df\u00e2\3\2\2\2\u00e0\u00de\3\2\2\2\u00e0\u00e1\3\2\2\2\u00e1"+
		"\u00e3\3\2\2\2\u00e2\u00e0\3\2\2\2\u00e3\u00e4\7\177\2\2\u00e4\u00e5\b"+
		"\30\16\2\u00e5\62\3\2\2\2\u00e6\u00eb\7]\2\2\u00e7\u00ea\n\t\2\2\u00e8"+
		"\u00ea\5\63\31\2\u00e9\u00e7\3\2\2\2\u00e9\u00e8\3\2\2\2\u00ea\u00ed\3"+
		"\2\2\2\u00eb\u00e9\3\2\2\2\u00eb\u00ec\3\2\2\2\u00ec\u00ee\3\2\2\2\u00ed"+
		"\u00eb\3\2\2\2\u00ee\u00ef\7_\2\2\u00ef\u00f0\b\31\17\2\u00f0\64\3\2\2"+
		"\2\u00f1\u00f2\t\n\2\2\u00f2\u00f3\b\32\20\2\u00f3\66\3\2\2\2\u00f4\u00f6"+
		"\n\13\2\2\u00f5\u00f4\3\2\2\2\u00f6\u00f7\3\2\2\2\u00f7\u00f5\3\2\2\2"+
		"\u00f7\u00f8\3\2\2\2\u00f8\u00f9\3\2\2\2\u00f9\u00fa\b\33\21\2\u00fa8"+
		"\3\2\2\2\31\2\3\4AHM_dnsu\u0083\u008c\u0091\u00a3\u00b8\u00c4\u00d6\u00da"+
		"\u00e0\u00e9\u00eb\u00f7\22\b\2\2\7\3\2\3\b\2\7\4\2\t\5\2\6\2\2\t\4\2"+
		"\3\22\3\3\23\4\3\24\5\3\25\6\3\26\7\3\30\b\3\31\t\3\32\n\3\33\13";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}