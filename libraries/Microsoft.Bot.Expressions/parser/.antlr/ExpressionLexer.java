// Generated from d:\project\botframework\botbuilder-dotnet\libraries\Microsoft.Bot.Expressions\parser/ExpressionLexer.g4 by ANTLR 4.7.1
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class ExpressionLexer extends Lexer {
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
		STRING_INTERPOLATION_MODE=1;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE", "STRING_INTERPOLATION_MODE"
	};

	public static final String[] ruleNames = {
		"LETTER", "DIGIT", "STRING_INTERPOLATION_START", "PLUS", "SUBSTRACT", 
		"NON", "XOR", "ASTERISK", "SLASH", "PERCENT", "DOUBLE_EQUAL", "NOT_EQUAL", 
		"SINGLE_AND", "DOUBLE_AND", "DOUBLE_VERTICAL_CYLINDER", "LESS_THAN", "MORE_THAN", 
		"LESS_OR_EQUAl", "MORE_OR_EQUAL", "OPEN_BRACKET", "CLOSE_BRACKET", "DOT", 
		"OPEN_SQUARE_BRACKET", "CLOSE_SQUARE_BRACKET", "COMMA", "NUMBER", "WHITESPACE", 
		"IDENTIFIER", "NEWLINE", "STRING", "CONSTANT", "INVALID_TOKEN_DEFAULT_MODE", 
		"STRING_INTERPOLATION_END", "TEMPLATE", "ESCAPE_CHARACTER", "TEXT_CONTENT"
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


	  bool ignoreWS = true;      // usually we ignore whitespace, but inside stringInterpolation, whitespace is significant


	public ExpressionLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "ExpressionLexer.g4"; }

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
		case 2:
			STRING_INTERPOLATION_START_action((RuleContext)_localctx, actionIndex);
			break;
		case 32:
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
		case 26:
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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2#\u00f5\b\1\b\1\4"+
		"\2\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n"+
		"\4\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\3\2\3\2\3\3\3\3\3\4\3\4\3\4\3\4\3\4"+
		"\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3\13\3\13\3\f\3\f\3\f"+
		"\3\r\3\r\3\r\3\r\5\rk\n\r\3\16\3\16\3\17\3\17\3\17\3\20\3\20\3\20\3\21"+
		"\3\21\3\22\3\22\3\23\3\23\3\23\3\24\3\24\3\24\3\25\3\25\3\26\3\26\3\27"+
		"\3\27\3\30\3\30\3\31\3\31\3\32\3\32\3\33\6\33\u008c\n\33\r\33\16\33\u008d"+
		"\3\33\3\33\6\33\u0092\n\33\r\33\16\33\u0093\5\33\u0096\n\33\3\34\3\34"+
		"\3\34\3\34\3\34\3\35\3\35\3\35\3\35\3\35\5\35\u00a2\n\35\3\35\3\35\3\35"+
		"\7\35\u00a7\n\35\f\35\16\35\u00aa\13\35\3\35\5\35\u00ad\n\35\3\36\5\36"+
		"\u00b0\n\36\3\36\3\36\3\36\3\36\3\37\3\37\7\37\u00b8\n\37\f\37\16\37\u00bb"+
		"\13\37\3\37\3\37\3\37\7\37\u00c0\n\37\f\37\16\37\u00c3\13\37\3\37\5\37"+
		"\u00c6\n\37\3 \3 \7 \u00ca\n \f \16 \u00cd\13 \3 \3 \3 \7 \u00d2\n \f"+
		" \16 \u00d5\13 \3 \5 \u00d8\n \3!\3!\3\"\3\"\3\"\3\"\3\"\3\"\3#\3#\3#"+
		"\3#\7#\u00e6\n#\f#\16#\u00e9\13#\3#\3#\3$\3$\5$\u00ef\n$\3%\3%\3%\5%\u00f4"+
		"\n%\3\u00e7\2&\4\2\6\2\b\3\n\4\f\5\16\6\20\7\22\b\24\t\26\n\30\13\32\f"+
		"\34\r\36\16 \17\"\20$\21&\22(\23*\24,\25.\26\60\27\62\30\64\31\66\328"+
		"\33:\34<\35>\36@\37B D\2F!H\"J#\4\2\3\13\4\2C\\c|\3\2\62;\6\2\13\13\""+
		"\"\u00a2\u00a2\uff01\uff01\5\2%%BBaa\4\2//aa\3\2))\3\2$$\b\2\f\f\17\17"+
		"$$))}}\177\177\4\2\f\f\17\17\2\u0107\2\b\3\2\2\2\2\n\3\2\2\2\2\f\3\2\2"+
		"\2\2\16\3\2\2\2\2\20\3\2\2\2\2\22\3\2\2\2\2\24\3\2\2\2\2\26\3\2\2\2\2"+
		"\30\3\2\2\2\2\32\3\2\2\2\2\34\3\2\2\2\2\36\3\2\2\2\2 \3\2\2\2\2\"\3\2"+
		"\2\2\2$\3\2\2\2\2&\3\2\2\2\2(\3\2\2\2\2*\3\2\2\2\2,\3\2\2\2\2.\3\2\2\2"+
		"\2\60\3\2\2\2\2\62\3\2\2\2\2\64\3\2\2\2\2\66\3\2\2\2\28\3\2\2\2\2:\3\2"+
		"\2\2\2<\3\2\2\2\2>\3\2\2\2\2@\3\2\2\2\2B\3\2\2\2\3D\3\2\2\2\3F\3\2\2\2"+
		"\3H\3\2\2\2\3J\3\2\2\2\4L\3\2\2\2\6N\3\2\2\2\bP\3\2\2\2\nU\3\2\2\2\fW"+
		"\3\2\2\2\16Y\3\2\2\2\20[\3\2\2\2\22]\3\2\2\2\24_\3\2\2\2\26a\3\2\2\2\30"+
		"c\3\2\2\2\32j\3\2\2\2\34l\3\2\2\2\36n\3\2\2\2 q\3\2\2\2\"t\3\2\2\2$v\3"+
		"\2\2\2&x\3\2\2\2({\3\2\2\2*~\3\2\2\2,\u0080\3\2\2\2.\u0082\3\2\2\2\60"+
		"\u0084\3\2\2\2\62\u0086\3\2\2\2\64\u0088\3\2\2\2\66\u008b\3\2\2\28\u0097"+
		"\3\2\2\2:\u00a1\3\2\2\2<\u00af\3\2\2\2>\u00c5\3\2\2\2@\u00d7\3\2\2\2B"+
		"\u00d9\3\2\2\2D\u00db\3\2\2\2F\u00e1\3\2\2\2H\u00ec\3\2\2\2J\u00f3\3\2"+
		"\2\2LM\t\2\2\2M\5\3\2\2\2NO\t\3\2\2O\7\3\2\2\2PQ\7b\2\2QR\b\4\2\2RS\3"+
		"\2\2\2ST\b\4\3\2T\t\3\2\2\2UV\7-\2\2V\13\3\2\2\2WX\7/\2\2X\r\3\2\2\2Y"+
		"Z\7#\2\2Z\17\3\2\2\2[\\\7`\2\2\\\21\3\2\2\2]^\7,\2\2^\23\3\2\2\2_`\7\61"+
		"\2\2`\25\3\2\2\2ab\7\'\2\2b\27\3\2\2\2cd\7?\2\2de\7?\2\2e\31\3\2\2\2f"+
		"g\7#\2\2gk\7?\2\2hi\7>\2\2ik\7@\2\2jf\3\2\2\2jh\3\2\2\2k\33\3\2\2\2lm"+
		"\7(\2\2m\35\3\2\2\2no\7(\2\2op\7(\2\2p\37\3\2\2\2qr\7~\2\2rs\7~\2\2s!"+
		"\3\2\2\2tu\7>\2\2u#\3\2\2\2vw\7@\2\2w%\3\2\2\2xy\7>\2\2yz\7?\2\2z\'\3"+
		"\2\2\2{|\7@\2\2|}\7?\2\2})\3\2\2\2~\177\7*\2\2\177+\3\2\2\2\u0080\u0081"+
		"\7+\2\2\u0081-\3\2\2\2\u0082\u0083\7\60\2\2\u0083/\3\2\2\2\u0084\u0085"+
		"\7]\2\2\u0085\61\3\2\2\2\u0086\u0087\7_\2\2\u0087\63\3\2\2\2\u0088\u0089"+
		"\7.\2\2\u0089\65\3\2\2\2\u008a\u008c\5\6\3\2\u008b\u008a\3\2\2\2\u008c"+
		"\u008d\3\2\2\2\u008d\u008b\3\2\2\2\u008d\u008e\3\2\2\2\u008e\u0095\3\2"+
		"\2\2\u008f\u0091\7\60\2\2\u0090\u0092\5\6\3\2\u0091\u0090\3\2\2\2\u0092"+
		"\u0093\3\2\2\2\u0093\u0091\3\2\2\2\u0093\u0094\3\2\2\2\u0094\u0096\3\2"+
		"\2\2\u0095\u008f\3\2\2\2\u0095\u0096\3\2\2\2\u0096\67\3\2\2\2\u0097\u0098"+
		"\t\4\2\2\u0098\u0099\6\34\2\2\u0099\u009a\3\2\2\2\u009a\u009b\b\34\4\2"+
		"\u009b9\3\2\2\2\u009c\u00a2\5\4\2\2\u009d\u00a2\t\5\2\2\u009e\u009f\7"+
		"B\2\2\u009f\u00a2\7B\2\2\u00a0\u00a2\4&\'\2\u00a1\u009c\3\2\2\2\u00a1"+
		"\u009d\3\2\2\2\u00a1\u009e\3\2\2\2\u00a1\u00a0\3\2\2\2\u00a2\u00a8\3\2"+
		"\2\2\u00a3\u00a7\5\4\2\2\u00a4\u00a7\5\6\3\2\u00a5\u00a7\t\6\2\2\u00a6"+
		"\u00a3\3\2\2\2\u00a6\u00a4\3\2\2\2\u00a6\u00a5\3\2\2\2\u00a7\u00aa\3\2"+
		"\2\2\u00a8\u00a6\3\2\2\2\u00a8\u00a9\3\2\2\2\u00a9\u00ac\3\2\2\2\u00aa"+
		"\u00a8\3\2\2\2\u00ab\u00ad\7#\2\2\u00ac\u00ab\3\2\2\2\u00ac\u00ad\3\2"+
		"\2\2\u00ad;\3\2\2\2\u00ae\u00b0\7\17\2\2\u00af\u00ae\3\2\2\2\u00af\u00b0"+
		"\3\2\2\2\u00b0\u00b1\3\2\2\2\u00b1\u00b2\7\f\2\2\u00b2\u00b3\3\2\2\2\u00b3"+
		"\u00b4\b\36\4\2\u00b4=\3\2\2\2\u00b5\u00b9\7)\2\2\u00b6\u00b8\n\7\2\2"+
		"\u00b7\u00b6\3\2\2\2\u00b8\u00bb\3\2\2\2\u00b9\u00b7\3\2\2\2\u00b9\u00ba"+
		"\3\2\2\2\u00ba\u00bc\3\2\2\2\u00bb\u00b9\3\2\2\2\u00bc\u00c6\7)\2\2\u00bd"+
		"\u00c1\7$\2\2\u00be\u00c0\n\b\2\2\u00bf\u00be\3\2\2\2\u00c0\u00c3\3\2"+
		"\2\2\u00c1\u00bf\3\2\2\2\u00c1\u00c2\3\2\2\2\u00c2\u00c4\3\2\2\2\u00c3"+
		"\u00c1\3\2\2\2\u00c4\u00c6\7$\2\2\u00c5\u00b5\3\2\2\2\u00c5\u00bd\3\2"+
		"\2\2\u00c6?\3\2\2\2\u00c7\u00cb\7]\2\2\u00c8\u00ca\58\34\2\u00c9\u00c8"+
		"\3\2\2\2\u00ca\u00cd\3\2\2\2\u00cb\u00c9\3\2\2\2\u00cb\u00cc\3\2\2\2\u00cc"+
		"\u00ce\3\2\2\2\u00cd\u00cb\3\2\2\2\u00ce\u00d8\7_\2\2\u00cf\u00d3\7}\2"+
		"\2\u00d0\u00d2\58\34\2\u00d1\u00d0\3\2\2\2\u00d2\u00d5\3\2\2\2\u00d3\u00d1"+
		"\3\2\2\2\u00d3\u00d4\3\2\2\2\u00d4\u00d6\3\2\2\2\u00d5\u00d3\3\2\2\2\u00d6"+
		"\u00d8\7\177\2\2\u00d7\u00c7\3\2\2\2\u00d7\u00cf\3\2\2\2\u00d8A\3\2\2"+
		"\2\u00d9\u00da\13\2\2\2\u00daC\3\2\2\2\u00db\u00dc\7b\2\2\u00dc\u00dd"+
		"\b\"\5\2\u00dd\u00de\3\2\2\2\u00de\u00df\b\"\6\2\u00df\u00e0\b\"\7\2\u00e0"+
		"E\3\2\2\2\u00e1\u00e2\7B\2\2\u00e2\u00e7\7}\2\2\u00e3\u00e6\5>\37\2\u00e4"+
		"\u00e6\n\t\2\2\u00e5\u00e3\3\2\2\2\u00e5\u00e4\3\2\2\2\u00e6\u00e9\3\2"+
		"\2\2\u00e7\u00e8\3\2\2\2\u00e7\u00e5\3\2\2\2\u00e8\u00ea\3\2\2\2\u00e9"+
		"\u00e7\3\2\2\2\u00ea\u00eb\7\177\2\2\u00ebG\3\2\2\2\u00ec\u00ee\7^\2\2"+
		"\u00ed\u00ef\n\n\2\2\u00ee\u00ed\3\2\2\2\u00ee\u00ef\3\2\2\2\u00efI\3"+
		"\2\2\2\u00f0\u00f1\7^\2\2\u00f1\u00f4\7b\2\2\u00f2\u00f4\n\n\2\2\u00f3"+
		"\u00f0\3\2\2\2\u00f3\u00f2\3\2\2\2\u00f4K\3\2\2\2\27\2\3j\u008d\u0093"+
		"\u0095\u00a1\u00a6\u00a8\u00ac\u00af\u00b9\u00c1\u00c5\u00cb\u00d3\u00d7"+
		"\u00e5\u00e7\u00ee\u00f3\b\3\4\2\7\3\2\b\2\2\3\"\3\t\3\2\6\2\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}