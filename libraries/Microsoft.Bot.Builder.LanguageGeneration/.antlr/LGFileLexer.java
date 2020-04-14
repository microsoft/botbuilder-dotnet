// Generated from d:\project\botframework\botbuilder-dotnet\libraries\Microsoft.Bot.Builder.LanguageGeneration/LGFileLexer.g4 by ANTLR 4.7.1
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
		NEWLINE=1, OPTION=2, COMMENT=3, IMPORT=4, TEMPLATE_NAME_LINE=5, TEMPLATE_BODY_LINE=6, 
		INVALID_LINE=7;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	public static final String[] ruleNames = {
		"WHITESPACE", "NEWLINE", "OPTION", "COMMENT", "IMPORT", "TEMPLATE_NAME_LINE", 
		"TEMPLATE_BODY_LINE", "INVALID_LINE"
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


	  bool startTemplate = false;


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
		case 5:
			TEMPLATE_NAME_LINE_action((RuleContext)_localctx, actionIndex);
			break;
		}
	}
	private void TEMPLATE_NAME_LINE_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 0:
			 startTemplate = true; 
			break;
		}
	}
	@Override
	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 2:
			return OPTION_sempred((RuleContext)_localctx, predIndex);
		case 3:
			return COMMENT_sempred((RuleContext)_localctx, predIndex);
		case 4:
			return IMPORT_sempred((RuleContext)_localctx, predIndex);
		case 6:
			return TEMPLATE_BODY_LINE_sempred((RuleContext)_localctx, predIndex);
		case 7:
			return INVALID_LINE_sempred((RuleContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean OPTION_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return  !startTemplate ;
		}
		return true;
	}
	private boolean COMMENT_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 1:
			return  !startTemplate ;
		}
		return true;
	}
	private boolean IMPORT_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 2:
			return  !startTemplate ;
		}
		return true;
	}
	private boolean TEMPLATE_BODY_LINE_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 3:
			return  startTemplate ;
		}
		return true;
	}
	private boolean INVALID_LINE_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 4:
			return  !startTemplate ;
		}
		return true;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\t{\b\1\4\2\t\2\4"+
		"\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\3\2\3\2\3\3\5\3"+
		"\27\n\3\3\3\3\3\3\4\7\4\34\n\4\f\4\16\4\37\13\4\3\4\3\4\7\4#\n\4\f\4\16"+
		"\4&\13\4\3\4\3\4\3\4\3\4\6\4,\n\4\r\4\16\4-\3\4\3\4\3\5\7\5\63\n\5\f\5"+
		"\16\5\66\13\5\3\5\3\5\7\5:\n\5\f\5\16\5=\13\5\3\5\3\5\3\6\7\6B\n\6\f\6"+
		"\16\6E\13\6\3\6\3\6\7\6I\n\6\f\6\16\6L\13\6\3\6\3\6\3\6\7\6Q\n\6\f\6\16"+
		"\6T\13\6\3\6\3\6\7\6X\n\6\f\6\16\6[\13\6\3\6\3\6\3\7\7\7`\n\7\f\7\16\7"+
		"c\13\7\3\7\3\7\7\7g\n\7\f\7\16\7j\13\7\3\7\3\7\3\b\6\bo\n\b\r\b\16\bp"+
		"\3\b\3\b\3\t\6\tv\n\t\r\t\16\tw\3\t\3\t\4JR\2\n\3\2\5\3\7\4\t\5\13\6\r"+
		"\7\17\b\21\t\3\2\6\6\2\13\13\"\"\u00a2\u00a2\uff01\uff01\4\2\f\f\17\17"+
		"\6\2\f\f\17\17]]__\5\2\f\f\17\17*+\2\u0087\2\5\3\2\2\2\2\7\3\2\2\2\2\t"+
		"\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2\3\23\3\2\2"+
		"\2\5\26\3\2\2\2\7\35\3\2\2\2\t\64\3\2\2\2\13C\3\2\2\2\ra\3\2\2\2\17n\3"+
		"\2\2\2\21u\3\2\2\2\23\24\t\2\2\2\24\4\3\2\2\2\25\27\7\17\2\2\26\25\3\2"+
		"\2\2\26\27\3\2\2\2\27\30\3\2\2\2\30\31\7\f\2\2\31\6\3\2\2\2\32\34\5\3"+
		"\2\2\33\32\3\2\2\2\34\37\3\2\2\2\35\33\3\2\2\2\35\36\3\2\2\2\36 \3\2\2"+
		"\2\37\35\3\2\2\2 $\7@\2\2!#\5\3\2\2\"!\3\2\2\2#&\3\2\2\2$\"\3\2\2\2$%"+
		"\3\2\2\2%\'\3\2\2\2&$\3\2\2\2\'(\7#\2\2()\7%\2\2)+\3\2\2\2*,\n\3\2\2+"+
		"*\3\2\2\2,-\3\2\2\2-+\3\2\2\2-.\3\2\2\2./\3\2\2\2/\60\6\4\2\2\60\b\3\2"+
		"\2\2\61\63\5\3\2\2\62\61\3\2\2\2\63\66\3\2\2\2\64\62\3\2\2\2\64\65\3\2"+
		"\2\2\65\67\3\2\2\2\66\64\3\2\2\2\67;\7@\2\28:\n\3\2\298\3\2\2\2:=\3\2"+
		"\2\2;9\3\2\2\2;<\3\2\2\2<>\3\2\2\2=;\3\2\2\2>?\6\5\3\2?\n\3\2\2\2@B\5"+
		"\3\2\2A@\3\2\2\2BE\3\2\2\2CA\3\2\2\2CD\3\2\2\2DF\3\2\2\2EC\3\2\2\2FJ\7"+
		"]\2\2GI\n\4\2\2HG\3\2\2\2IL\3\2\2\2JK\3\2\2\2JH\3\2\2\2KM\3\2\2\2LJ\3"+
		"\2\2\2MN\7_\2\2NR\7*\2\2OQ\n\5\2\2PO\3\2\2\2QT\3\2\2\2RS\3\2\2\2RP\3\2"+
		"\2\2SU\3\2\2\2TR\3\2\2\2UY\7+\2\2VX\5\3\2\2WV\3\2\2\2X[\3\2\2\2YW\3\2"+
		"\2\2YZ\3\2\2\2Z\\\3\2\2\2[Y\3\2\2\2\\]\6\6\4\2]\f\3\2\2\2^`\5\3\2\2_^"+
		"\3\2\2\2`c\3\2\2\2a_\3\2\2\2ab\3\2\2\2bd\3\2\2\2ca\3\2\2\2dh\7%\2\2eg"+
		"\n\3\2\2fe\3\2\2\2gj\3\2\2\2hf\3\2\2\2hi\3\2\2\2ik\3\2\2\2jh\3\2\2\2k"+
		"l\b\7\2\2l\16\3\2\2\2mo\n\3\2\2nm\3\2\2\2op\3\2\2\2pn\3\2\2\2pq\3\2\2"+
		"\2qr\3\2\2\2rs\6\b\5\2s\20\3\2\2\2tv\n\3\2\2ut\3\2\2\2vw\3\2\2\2wu\3\2"+
		"\2\2wx\3\2\2\2xy\3\2\2\2yz\6\t\6\2z\22\3\2\2\2\21\2\26\35$-\64;CJRYah"+
		"pw\3\3\7\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}