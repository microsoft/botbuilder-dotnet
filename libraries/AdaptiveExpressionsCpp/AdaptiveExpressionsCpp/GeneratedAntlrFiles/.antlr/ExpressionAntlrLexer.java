// Generated from c:\Repos\TemplatingLib\botbuilder-dotnet\libraries\AdaptiveExpressionsCpp\AdaptiveExpressionsCpp\Parser\ExpressionAntlrLexer.g4 by ANTLR 4.8
#pragma warning disable 3021
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class ExpressionAntlrLexer extends Lexer {
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
		STRING_INTERPOLATION_MODE=1;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE", "STRING_INTERPOLATION_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"LETTER", "DIGIT", "OBJECT_DEFINITION", "STRING_INTERPOLATION_START", 
			"PLUS", "SUBSTRACT", "NON", "XOR", "ASTERISK", "SLASH", "PERCENT", "DOUBLE_EQUAL", 
			"NOT_EQUAL", "SINGLE_AND", "DOUBLE_AND", "DOUBLE_VERTICAL_CYLINDER", 
			"LESS_THAN", "MORE_THAN", "LESS_OR_EQUAl", "MORE_OR_EQUAL", "OPEN_BRACKET", 
			"CLOSE_BRACKET", "DOT", "OPEN_SQUARE_BRACKET", "CLOSE_SQUARE_BRACKET", 
			"OPEN_CURLY_BRACKET", "CLOSE_CURLY_BRACKET", "COMMA", "COLON", "ARROW", 
			"NUMBER", "WHITESPACE", "IDENTIFIER", "NEWLINE", "STRING", "INVALID_TOKEN_DEFAULT_MODE", 
			"STRING_INTERPOLATION_END", "TEMPLATE", "ESCAPE_CHARACTER", "TEXT_CONTENT"
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


	  bool ignoreWS = true;      // usually we ignore whitespace, but inside stringInterpolation, whitespace is significant


	public ExpressionAntlrLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "ExpressionAntlrLexer.g4"; }

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
		case 3:
			STRING_INTERPOLATION_START_action((RuleContext)_localctx, actionIndex);
			break;
		case 36:
			STRING_INTERPOLATION_END_action((RuleContext)_localctx, actionIndex);
			break;
		}
	}
	private void STRING_INTERPOLATION_START_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 0:
			 ignoreWS = false;
			break;
		}
	}
	private void STRING_INTERPOLATION_END_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 1:
			ignoreWS = true;
			break;
		}
	}
	@Override
	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 31:
			return WHITESPACE_sempred((RuleContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean WHITESPACE_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return ignoreWS;
		}
		return true;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2&\u0107\b\1\b\1\4"+
		"\2\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n"+
		"\4\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\3\2\3\2\3"+
		"\3\3\3\3\4\3\4\3\4\3\4\5\4]\n\4\3\4\3\4\3\4\3\4\6\4c\n\4\r\4\16\4d\7\4"+
		"g\n\4\f\4\16\4j\13\4\3\4\3\4\3\5\3\5\3\5\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3"+
		"\b\3\t\3\t\3\n\3\n\3\13\3\13\3\f\3\f\3\r\3\r\3\r\3\16\3\16\3\16\3\16\5"+
		"\16\u0088\n\16\3\17\3\17\3\20\3\20\3\20\3\21\3\21\3\21\3\22\3\22\3\23"+
		"\3\23\3\24\3\24\3\24\3\25\3\25\3\25\3\26\3\26\3\27\3\27\3\30\3\30\3\31"+
		"\3\31\3\32\3\32\3\33\3\33\3\34\3\34\3\35\3\35\3\36\3\36\3\37\3\37\3\37"+
		"\3 \6 \u00b2\n \r \16 \u00b3\3 \3 \6 \u00b8\n \r \16 \u00b9\5 \u00bc\n"+
		" \3!\3!\3!\3!\3!\3\"\3\"\3\"\3\"\3\"\5\"\u00c8\n\"\3\"\3\"\3\"\7\"\u00cd"+
		"\n\"\f\"\16\"\u00d0\13\"\3#\5#\u00d3\n#\3#\3#\3#\3#\3$\3$\3$\3$\7$\u00dd"+
		"\n$\f$\16$\u00e0\13$\3$\3$\3$\3$\3$\7$\u00e7\n$\f$\16$\u00ea\13$\3$\5"+
		"$\u00ed\n$\3%\3%\3&\3&\3&\3&\3&\3&\3\'\3\'\3\'\3\'\3\'\6\'\u00fc\n\'\r"+
		"\'\16\'\u00fd\3\'\3\'\3(\3(\5(\u0104\n(\3)\3)\4\u00de\u00e8\2*\4\2\6\2"+
		"\b\2\n\3\f\4\16\5\20\6\22\7\24\b\26\t\30\n\32\13\34\f\36\r \16\"\17$\20"+
		"&\21(\22*\23,\24.\25\60\26\62\27\64\30\66\318\32:\33<\34>\35@\36B\37D"+
		" F!H\"J#L\2N$P%R&\4\2\3\f\4\2C\\c|\3\2\62;\t\2\f\f\17\17$$))bb}}\177\177"+
		"\6\2\13\13\"\"\u00a2\u00a2\uff01\uff01\5\2%%BBaa\4\2))^^\3\2))\4\2$$^"+
		"^\3\2$$\4\2\f\f\17\17\2\u011c\2\n\3\2\2\2\2\f\3\2\2\2\2\16\3\2\2\2\2\20"+
		"\3\2\2\2\2\22\3\2\2\2\2\24\3\2\2\2\2\26\3\2\2\2\2\30\3\2\2\2\2\32\3\2"+
		"\2\2\2\34\3\2\2\2\2\36\3\2\2\2\2 \3\2\2\2\2\"\3\2\2\2\2$\3\2\2\2\2&\3"+
		"\2\2\2\2(\3\2\2\2\2*\3\2\2\2\2,\3\2\2\2\2.\3\2\2\2\2\60\3\2\2\2\2\62\3"+
		"\2\2\2\2\64\3\2\2\2\2\66\3\2\2\2\28\3\2\2\2\2:\3\2\2\2\2<\3\2\2\2\2>\3"+
		"\2\2\2\2@\3\2\2\2\2B\3\2\2\2\2D\3\2\2\2\2F\3\2\2\2\2H\3\2\2\2\2J\3\2\2"+
		"\2\3L\3\2\2\2\3N\3\2\2\2\3P\3\2\2\2\3R\3\2\2\2\4T\3\2\2\2\6V\3\2\2\2\b"+
		"X\3\2\2\2\nm\3\2\2\2\fr\3\2\2\2\16t\3\2\2\2\20v\3\2\2\2\22x\3\2\2\2\24"+
		"z\3\2\2\2\26|\3\2\2\2\30~\3\2\2\2\32\u0080\3\2\2\2\34\u0087\3\2\2\2\36"+
		"\u0089\3\2\2\2 \u008b\3\2\2\2\"\u008e\3\2\2\2$\u0091\3\2\2\2&\u0093\3"+
		"\2\2\2(\u0095\3\2\2\2*\u0098\3\2\2\2,\u009b\3\2\2\2.\u009d\3\2\2\2\60"+
		"\u009f\3\2\2\2\62\u00a1\3\2\2\2\64\u00a3\3\2\2\2\66\u00a5\3\2\2\28\u00a7"+
		"\3\2\2\2:\u00a9\3\2\2\2<\u00ab\3\2\2\2>\u00ad\3\2\2\2@\u00b1\3\2\2\2B"+
		"\u00bd\3\2\2\2D\u00c7\3\2\2\2F\u00d2\3\2\2\2H\u00ec\3\2\2\2J\u00ee\3\2"+
		"\2\2L\u00f0\3\2\2\2N\u00f6\3\2\2\2P\u0101\3\2\2\2R\u0105\3\2\2\2TU\t\2"+
		"\2\2U\5\3\2\2\2VW\t\3\2\2W\7\3\2\2\2Xh\7}\2\2Yg\5B!\2Z]\5D\"\2[]\5H$\2"+
		"\\Z\3\2\2\2\\[\3\2\2\2]^\3\2\2\2^b\7<\2\2_c\5H$\2`c\n\4\2\2ac\5\b\4\2"+
		"b_\3\2\2\2b`\3\2\2\2ba\3\2\2\2cd\3\2\2\2db\3\2\2\2de\3\2\2\2eg\3\2\2\2"+
		"fY\3\2\2\2f\\\3\2\2\2gj\3\2\2\2hf\3\2\2\2hi\3\2\2\2ik\3\2\2\2jh\3\2\2"+
		"\2kl\7\177\2\2l\t\3\2\2\2mn\7b\2\2no\b\5\2\2op\3\2\2\2pq\b\5\3\2q\13\3"+
		"\2\2\2rs\7-\2\2s\r\3\2\2\2tu\7/\2\2u\17\3\2\2\2vw\7#\2\2w\21\3\2\2\2x"+
		"y\7`\2\2y\23\3\2\2\2z{\7,\2\2{\25\3\2\2\2|}\7\61\2\2}\27\3\2\2\2~\177"+
		"\7\'\2\2\177\31\3\2\2\2\u0080\u0081\7?\2\2\u0081\u0082\7?\2\2\u0082\33"+
		"\3\2\2\2\u0083\u0084\7#\2\2\u0084\u0088\7?\2\2\u0085\u0086\7>\2\2\u0086"+
		"\u0088\7@\2\2\u0087\u0083\3\2\2\2\u0087\u0085\3\2\2\2\u0088\35\3\2\2\2"+
		"\u0089\u008a\7(\2\2\u008a\37\3\2\2\2\u008b\u008c\7(\2\2\u008c\u008d\7"+
		"(\2\2\u008d!\3\2\2\2\u008e\u008f\7~\2\2\u008f\u0090\7~\2\2\u0090#\3\2"+
		"\2\2\u0091\u0092\7>\2\2\u0092%\3\2\2\2\u0093\u0094\7@\2\2\u0094\'\3\2"+
		"\2\2\u0095\u0096\7>\2\2\u0096\u0097\7?\2\2\u0097)\3\2\2\2\u0098\u0099"+
		"\7@\2\2\u0099\u009a\7?\2\2\u009a+\3\2\2\2\u009b\u009c\7*\2\2\u009c-\3"+
		"\2\2\2\u009d\u009e\7+\2\2\u009e/\3\2\2\2\u009f\u00a0\7\60\2\2\u00a0\61"+
		"\3\2\2\2\u00a1\u00a2\7]\2\2\u00a2\63\3\2\2\2\u00a3\u00a4\7_\2\2\u00a4"+
		"\65\3\2\2\2\u00a5\u00a6\7}\2\2\u00a6\67\3\2\2\2\u00a7\u00a8\7\177\2\2"+
		"\u00a89\3\2\2\2\u00a9\u00aa\7.\2\2\u00aa;\3\2\2\2\u00ab\u00ac\7<\2\2\u00ac"+
		"=\3\2\2\2\u00ad\u00ae\7?\2\2\u00ae\u00af\7@\2\2\u00af?\3\2\2\2\u00b0\u00b2"+
		"\5\6\3\2\u00b1\u00b0\3\2\2\2\u00b2\u00b3\3\2\2\2\u00b3\u00b1\3\2\2\2\u00b3"+
		"\u00b4\3\2\2\2\u00b4\u00bb\3\2\2\2\u00b5\u00b7\7\60\2\2\u00b6\u00b8\5"+
		"\6\3\2\u00b7\u00b6\3\2\2\2\u00b8\u00b9\3\2\2\2\u00b9\u00b7\3\2\2\2\u00b9"+
		"\u00ba\3\2\2\2\u00ba\u00bc\3\2\2\2\u00bb\u00b5\3\2\2\2\u00bb\u00bc\3\2"+
		"\2\2\u00bcA\3\2\2\2\u00bd\u00be\t\5\2\2\u00be\u00bf\6!\2\2\u00bf\u00c0"+
		"\3\2\2\2\u00c0\u00c1\b!\4\2\u00c1C\3\2\2\2\u00c2\u00c8\5\4\2\2\u00c3\u00c8"+
		"\t\6\2\2\u00c4\u00c5\7B\2\2\u00c5\u00c8\7B\2\2\u00c6\u00c8\4&\'\2\u00c7"+
		"\u00c2\3\2\2\2\u00c7\u00c3\3\2\2\2\u00c7\u00c4\3\2\2\2\u00c7\u00c6\3\2"+
		"\2\2\u00c8\u00ce\3\2\2\2\u00c9\u00cd\5\4\2\2\u00ca\u00cd\5\6\3\2\u00cb"+
		"\u00cd\7a\2\2\u00cc\u00c9\3\2\2\2\u00cc\u00ca\3\2\2\2\u00cc\u00cb\3\2"+
		"\2\2\u00cd\u00d0\3\2\2\2\u00ce\u00cc\3\2\2\2\u00ce\u00cf\3\2\2\2\u00cf"+
		"E\3\2\2\2\u00d0\u00ce\3\2\2\2\u00d1\u00d3\7\17\2\2\u00d2\u00d1\3\2\2\2"+
		"\u00d2\u00d3\3\2\2\2\u00d3\u00d4\3\2\2\2\u00d4\u00d5\7\f\2\2\u00d5\u00d6"+
		"\3\2\2\2\u00d6\u00d7\b#\4\2\u00d7G\3\2\2\2\u00d8\u00de\7)\2\2\u00d9\u00da"+
		"\7^\2\2\u00da\u00dd\t\7\2\2\u00db\u00dd\n\b\2\2\u00dc\u00d9\3\2\2\2\u00dc"+
		"\u00db\3\2\2\2\u00dd\u00e0\3\2\2\2\u00de\u00df\3\2\2\2\u00de\u00dc\3\2"+
		"\2\2\u00df\u00e1\3\2\2\2\u00e0\u00de\3\2\2\2\u00e1\u00ed\7)\2\2\u00e2"+
		"\u00e8\7$\2\2\u00e3\u00e4\7^\2\2\u00e4\u00e7\t\t\2\2\u00e5\u00e7\n\n\2"+
		"\2\u00e6\u00e3\3\2\2\2\u00e6\u00e5\3\2\2\2\u00e7\u00ea\3\2\2\2\u00e8\u00e9"+
		"\3\2\2\2\u00e8\u00e6\3\2\2\2\u00e9\u00eb\3\2\2\2\u00ea\u00e8\3\2\2\2\u00eb"+
		"\u00ed\7$\2\2\u00ec\u00d8\3\2\2\2\u00ec\u00e2\3\2\2\2\u00edI\3\2\2\2\u00ee"+
		"\u00ef\13\2\2\2\u00efK\3\2\2\2\u00f0\u00f1\7b\2\2\u00f1\u00f2\b&\5\2\u00f2"+
		"\u00f3\3\2\2\2\u00f3\u00f4\b&\6\2\u00f4\u00f5\b&\7\2\u00f5M\3\2\2\2\u00f6"+
		"\u00f7\7&\2\2\u00f7\u00fb\7}\2\2\u00f8\u00fc\5H$\2\u00f9\u00fc\5\b\4\2"+
		"\u00fa\u00fc\n\4\2\2\u00fb\u00f8\3\2\2\2\u00fb\u00f9\3\2\2\2\u00fb\u00fa"+
		"\3\2\2\2\u00fc\u00fd\3\2\2\2\u00fd\u00fb\3\2\2\2\u00fd\u00fe\3\2\2\2\u00fe"+
		"\u00ff\3\2\2\2\u00ff\u0100\7\177\2\2\u0100O\3\2\2\2\u0101\u0103\7^\2\2"+
		"\u0102\u0104\n\13\2\2\u0103\u0102\3\2\2\2\u0103\u0104\3\2\2\2\u0104Q\3"+
		"\2\2\2\u0105\u0106\13\2\2\2\u0106S\3\2\2\2\31\2\3\\bdfh\u0087\u00b3\u00b9"+
		"\u00bb\u00c7\u00cc\u00ce\u00d2\u00dc\u00de\u00e6\u00e8\u00ec\u00fb\u00fd"+
		"\u0103\b\3\5\2\7\3\2\b\2\2\3&\3\t\3\2\6\2\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}