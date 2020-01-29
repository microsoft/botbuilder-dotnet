// Generated from d:\project\botframework\botbuilder-dotnet\libraries\Microsoft.Bot.Builder.LanguageGeneration\LGFileLexer.g4 by ANTLR 4.7.1
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
public class LGFileLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.7.1", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		COMMENTS=1, WS=2, NEWLINE=3, HASH=4, DASH=5, LEFT_SQUARE_BRACKET=6, IMPORT=7, 
		INVALID_TOKEN=8, WS_IN_NAME=9, NEWLINE_IN_NAME=10, IDENTIFIER=11, DOT=12, 
		OPEN_PARENTHESIS=13, CLOSE_PARENTHESIS=14, COMMA=15, TEXT_IN_NAME=16, 
		WS_IN_BODY=17, MULTILINE_PREFIX=18, NEWLINE_IN_BODY=19, IF=20, ELSEIF=21, 
		ELSE=22, SWITCH=23, CASE=24, DEFAULT=25, ESCAPE_CHARACTER=26, EXPRESSION=27, 
		TEXT=28, MULTILINE_SUFFIX=29, WS_IN_STRUCTURE_NAME=30, NEWLINE_IN_STRUCTURE_NAME=31, 
		STRUCTURE_NAME=32, TEXT_IN_STRUCTURE_NAME=33, STRUCTURED_COMMENTS=34, 
		WS_IN_STRUCTURE_BODY=35, STRUCTURED_NEWLINE=36, STRUCTURED_BODY_END=37, 
		STRUCTURE_IDENTIFIER=38, STRUCTURE_EQUALS=39, STRUCTURE_OR_MARK=40, ESCAPE_CHARACTER_IN_STRUCTURE_BODY=41, 
		EXPRESSION_IN_STRUCTURE_BODY=42, TEXT_IN_STRUCTURE_BODY=43;
	public static final int
		TEMPLATE_NAME_MODE=1, TEMPLATE_BODY_MODE=2, MULTILINE_MODE=3, STRUCTURE_NAME_MODE=4, 
		STRUCTURE_BODY_MODE=5;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE", "TEMPLATE_NAME_MODE", "TEMPLATE_BODY_MODE", "MULTILINE_MODE", 
		"STRUCTURE_NAME_MODE", "STRUCTURE_BODY_MODE"
	};

	public static final String[] ruleNames = {
		"A", "C", "D", "E", "F", "H", "I", "L", "S", "T", "U", "W", "LETTER", 
		"NUMBER", "WHITESPACE", "EMPTY_OBJECT", "STRING_LITERAL", "EXPRESSION_FRAGMENT", 
		"ESCAPE_CHARACTER_FRAGMENT", "COMMENTS", "WS", "NEWLINE", "HASH", "DASH", 
		"LEFT_SQUARE_BRACKET", "IMPORT", "INVALID_TOKEN", "WS_IN_NAME", "NEWLINE_IN_NAME", 
		"IDENTIFIER", "DOT", "OPEN_PARENTHESIS", "CLOSE_PARENTHESIS", "COMMA", 
		"TEXT_IN_NAME", "WS_IN_BODY", "MULTILINE_PREFIX", "NEWLINE_IN_BODY", "IF", 
		"ELSEIF", "ELSE", "SWITCH", "CASE", "DEFAULT", "ESCAPE_CHARACTER", "EXPRESSION", 
		"TEXT", "MULTILINE_SUFFIX", "MULTILINE_ESCAPE_CHARACTER", "MULTILINE_EXPRESSION", 
		"MULTILINE_TEXT", "WS_IN_STRUCTURE_NAME", "NEWLINE_IN_STRUCTURE_NAME", 
		"STRUCTURE_NAME", "TEXT_IN_STRUCTURE_NAME", "STRUCTURED_COMMENTS", "WS_IN_STRUCTURE_BODY", 
		"STRUCTURED_NEWLINE", "STRUCTURED_BODY_END", "STRUCTURE_IDENTIFIER", "STRUCTURE_EQUALS", 
		"STRUCTURE_OR_MARK", "ESCAPE_CHARACTER_IN_STRUCTURE_BODY", "EXPRESSION_IN_STRUCTURE_BODY", 
		"TEXT_IN_STRUCTURE_BODY"
	};

	private static final String[] _LITERAL_NAMES = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		"'.'", "'('", "')'", "','", null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, "'='", "'|'"
	};
	private static final String[] _SYMBOLIC_NAMES = {
		null, "COMMENTS", "WS", "NEWLINE", "HASH", "DASH", "LEFT_SQUARE_BRACKET", 
		"IMPORT", "INVALID_TOKEN", "WS_IN_NAME", "NEWLINE_IN_NAME", "IDENTIFIER", 
		"DOT", "OPEN_PARENTHESIS", "CLOSE_PARENTHESIS", "COMMA", "TEXT_IN_NAME", 
		"WS_IN_BODY", "MULTILINE_PREFIX", "NEWLINE_IN_BODY", "IF", "ELSEIF", "ELSE", 
		"SWITCH", "CASE", "DEFAULT", "ESCAPE_CHARACTER", "EXPRESSION", "TEXT", 
		"MULTILINE_SUFFIX", "WS_IN_STRUCTURE_NAME", "NEWLINE_IN_STRUCTURE_NAME", 
		"STRUCTURE_NAME", "TEXT_IN_STRUCTURE_NAME", "STRUCTURED_COMMENTS", "WS_IN_STRUCTURE_BODY", 
		"STRUCTURED_NEWLINE", "STRUCTURED_BODY_END", "STRUCTURE_IDENTIFIER", "STRUCTURE_EQUALS", 
		"STRUCTURE_OR_MARK", "ESCAPE_CHARACTER_IN_STRUCTURE_BODY", "EXPRESSION_IN_STRUCTURE_BODY", 
		"TEXT_IN_STRUCTURE_BODY"
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


	  bool ignoreWS = true; // usually we ignore whitespace, but inside template, whitespace is significant
	  bool inTemplate = false; // whether we are in the template
	  bool beginOfTemplateBody = false; // whether we are at the begining of template body
	  bool inMultiline = false; // whether we are in multiline
	  bool beginOfTemplateLine = false;// weather we are at the begining of template string
	  bool inStructuredValue = false; // weather we are in the structure value
	  bool beginOfStructureProperty = false; // weather we are at the begining of structure property


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
		case 22:
			HASH_action((RuleContext)_localctx, actionIndex);
			break;
		case 23:
			DASH_action((RuleContext)_localctx, actionIndex);
			break;
		case 25:
			IMPORT_action((RuleContext)_localctx, actionIndex);
			break;
		case 26:
			INVALID_TOKEN_action((RuleContext)_localctx, actionIndex);
			break;
		case 28:
			NEWLINE_IN_NAME_action((RuleContext)_localctx, actionIndex);
			break;
		case 36:
			MULTILINE_PREFIX_action((RuleContext)_localctx, actionIndex);
			break;
		case 37:
			NEWLINE_IN_BODY_action((RuleContext)_localctx, actionIndex);
			break;
		case 38:
			IF_action((RuleContext)_localctx, actionIndex);
			break;
		case 39:
			ELSEIF_action((RuleContext)_localctx, actionIndex);
			break;
		case 40:
			ELSE_action((RuleContext)_localctx, actionIndex);
			break;
		case 41:
			SWITCH_action((RuleContext)_localctx, actionIndex);
			break;
		case 42:
			CASE_action((RuleContext)_localctx, actionIndex);
			break;
		case 43:
			DEFAULT_action((RuleContext)_localctx, actionIndex);
			break;
		case 44:
			ESCAPE_CHARACTER_action((RuleContext)_localctx, actionIndex);
			break;
		case 45:
			EXPRESSION_action((RuleContext)_localctx, actionIndex);
			break;
		case 46:
			TEXT_action((RuleContext)_localctx, actionIndex);
			break;
		case 47:
			MULTILINE_SUFFIX_action((RuleContext)_localctx, actionIndex);
			break;
		case 52:
			NEWLINE_IN_STRUCTURE_NAME_action((RuleContext)_localctx, actionIndex);
			break;
		case 57:
			STRUCTURED_NEWLINE_action((RuleContext)_localctx, actionIndex);
			break;
		case 58:
			STRUCTURED_BODY_END_action((RuleContext)_localctx, actionIndex);
			break;
		case 59:
			STRUCTURE_IDENTIFIER_action((RuleContext)_localctx, actionIndex);
			break;
		case 60:
			STRUCTURE_EQUALS_action((RuleContext)_localctx, actionIndex);
			break;
		case 61:
			STRUCTURE_OR_MARK_action((RuleContext)_localctx, actionIndex);
			break;
		case 62:
			ESCAPE_CHARACTER_IN_STRUCTURE_BODY_action((RuleContext)_localctx, actionIndex);
			break;
		case 63:
			EXPRESSION_IN_STRUCTURE_BODY_action((RuleContext)_localctx, actionIndex);
			break;
		case 64:
			TEXT_IN_STRUCTURE_BODY_action((RuleContext)_localctx, actionIndex);
			break;
		}
	}
	private void HASH_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 0:
			 inTemplate = true; beginOfTemplateBody = false; 
			break;
		}
	}
	private void DASH_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 1:
			 beginOfTemplateLine = true; beginOfTemplateBody = false; 
			break;
		}
	}
	private void IMPORT_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 2:
			 inTemplate = false;
			break;
		}
	}
	private void INVALID_TOKEN_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 3:
			 inTemplate = false; beginOfTemplateBody = false; 
			break;
		}
	}
	private void NEWLINE_IN_NAME_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 4:
			 beginOfTemplateBody = true;
			break;
		}
	}
	private void MULTILINE_PREFIX_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 5:
			 inMultiline = true; beginOfTemplateLine = false;
			break;
		}
	}
	private void NEWLINE_IN_BODY_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 6:
			 ignoreWS = true;
			break;
		}
	}
	private void IF_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 7:
			 ignoreWS = true; beginOfTemplateLine = false;
			break;
		}
	}
	private void ELSEIF_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 8:
			 ignoreWS = true; beginOfTemplateLine = false;
			break;
		}
	}
	private void ELSE_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 9:
			 ignoreWS = true; beginOfTemplateLine = false;
			break;
		}
	}
	private void SWITCH_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 10:
			 ignoreWS = true; beginOfTemplateLine = false;
			break;
		}
	}
	private void CASE_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 11:
			 ignoreWS = true; beginOfTemplateLine = false;
			break;
		}
	}
	private void DEFAULT_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 12:
			 ignoreWS = true; beginOfTemplateLine = false;
			break;
		}
	}
	private void ESCAPE_CHARACTER_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 13:
			 ignoreWS = false; beginOfTemplateLine = false;
			break;
		}
	}
	private void EXPRESSION_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 14:
			 ignoreWS = false; beginOfTemplateLine = false;
			break;
		}
	}
	private void TEXT_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 15:
			 ignoreWS = false; beginOfTemplateLine = false;
			break;
		}
	}
	private void MULTILINE_SUFFIX_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 16:
			 inMultiline = false; 
			break;
		}
	}
	private void NEWLINE_IN_STRUCTURE_NAME_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 17:
			 ignoreWS = true;
			break;
		case 18:
			beginOfStructureProperty = true;
			break;
		}
	}
	private void STRUCTURED_NEWLINE_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 19:
			 ignoreWS = true; inStructuredValue = false; beginOfStructureProperty = true;
			break;
		}
	}
	private void STRUCTURED_BODY_END_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 20:
			 inTemplate = false; beginOfTemplateBody = false;
			break;
		}
	}
	private void STRUCTURE_IDENTIFIER_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 21:
			beginOfStructureProperty = false;
			break;
		}
	}
	private void STRUCTURE_EQUALS_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 22:
			inStructuredValue = true;
			break;
		}
	}
	private void STRUCTURE_OR_MARK_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 23:
			 ignoreWS = true; 
			break;
		}
	}
	private void ESCAPE_CHARACTER_IN_STRUCTURE_BODY_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 24:
			 ignoreWS = false; 
			break;
		}
	}
	private void EXPRESSION_IN_STRUCTURE_BODY_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 25:
			 ignoreWS = false; 
			break;
		}
	}
	private void TEXT_IN_STRUCTURE_BODY_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 26:
			 ignoreWS = false; beginOfStructureProperty = false;
			break;
		}
	}
	@Override
	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 23:
			return DASH_sempred((RuleContext)_localctx, predIndex);
		case 24:
			return LEFT_SQUARE_BRACKET_sempred((RuleContext)_localctx, predIndex);
		case 35:
			return WS_IN_BODY_sempred((RuleContext)_localctx, predIndex);
		case 36:
			return MULTILINE_PREFIX_sempred((RuleContext)_localctx, predIndex);
		case 38:
			return IF_sempred((RuleContext)_localctx, predIndex);
		case 39:
			return ELSEIF_sempred((RuleContext)_localctx, predIndex);
		case 40:
			return ELSE_sempred((RuleContext)_localctx, predIndex);
		case 41:
			return SWITCH_sempred((RuleContext)_localctx, predIndex);
		case 42:
			return CASE_sempred((RuleContext)_localctx, predIndex);
		case 43:
			return DEFAULT_sempred((RuleContext)_localctx, predIndex);
		case 55:
			return STRUCTURED_COMMENTS_sempred((RuleContext)_localctx, predIndex);
		case 56:
			return WS_IN_STRUCTURE_BODY_sempred((RuleContext)_localctx, predIndex);
		case 58:
			return STRUCTURED_BODY_END_sempred((RuleContext)_localctx, predIndex);
		case 59:
			return STRUCTURE_IDENTIFIER_sempred((RuleContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean DASH_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return  inTemplate ;
		}
		return true;
	}
	private boolean LEFT_SQUARE_BRACKET_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 1:
			return  inTemplate && beginOfTemplateBody ;
		}
		return true;
	}
	private boolean WS_IN_BODY_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 2:
			return ignoreWS;
		}
		return true;
	}
	private boolean MULTILINE_PREFIX_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 3:
			return  !inMultiline  && beginOfTemplateLine ;
		}
		return true;
	}
	private boolean IF_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 4:
			return beginOfTemplateLine;
		}
		return true;
	}
	private boolean ELSEIF_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 5:
			return beginOfTemplateLine;
		}
		return true;
	}
	private boolean ELSE_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 6:
			return beginOfTemplateLine;
		}
		return true;
	}
	private boolean SWITCH_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 7:
			return beginOfTemplateLine;
		}
		return true;
	}
	private boolean CASE_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 8:
			return beginOfTemplateLine;
		}
		return true;
	}
	private boolean DEFAULT_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 9:
			return beginOfTemplateLine;
		}
		return true;
	}
	private boolean STRUCTURED_COMMENTS_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 10:
			return  !inStructuredValue && beginOfStructureProperty;
		}
		return true;
	}
	private boolean WS_IN_STRUCTURE_BODY_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 11:
			return ignoreWS;
		}
		return true;
	}
	private boolean STRUCTURED_BODY_END_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 12:
			return !inStructuredValue;
		}
		return true;
	}
	private boolean STRUCTURE_IDENTIFIER_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 13:
			return  !inStructuredValue && beginOfStructureProperty;
		}
		return true;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2-\u0245\b\1\b\1\b"+
		"\1\b\1\b\1\b\1\4\2\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b"+
		"\4\t\t\t\4\n\t\n\4\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t"+
		"\20\4\21\t\21\4\22\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t"+
		"\27\4\30\t\30\4\31\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t"+
		"\36\4\37\t\37\4 \t \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t"+
		"(\4)\t)\4*\t*\4+\t+\4,\t,\4-\t-\4.\t.\4/\t/\4\60\t\60\4\61\t\61\4\62\t"+
		"\62\4\63\t\63\4\64\t\64\4\65\t\65\4\66\t\66\4\67\t\67\48\t8\49\t9\4:\t"+
		":\4;\t;\4<\t<\4=\t=\4>\t>\4?\t?\4@\t@\4A\tA\4B\tB\3\2\3\2\3\3\3\3\3\4"+
		"\3\4\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3\13\3\13\3\f\3\f"+
		"\3\r\3\r\3\16\3\16\3\17\3\17\3\20\3\20\3\21\3\21\7\21\u00ab\n\21\f\21"+
		"\16\21\u00ae\13\21\3\21\3\21\3\22\3\22\7\22\u00b4\n\22\f\22\16\22\u00b7"+
		"\13\22\3\22\3\22\3\22\7\22\u00bc\n\22\f\22\16\22\u00bf\13\22\3\22\5\22"+
		"\u00c2\n\22\3\23\3\23\3\23\3\23\3\23\6\23\u00c9\n\23\r\23\16\23\u00ca"+
		"\3\23\5\23\u00ce\n\23\3\24\3\24\5\24\u00d2\n\24\3\25\3\25\6\25\u00d6\n"+
		"\25\r\25\16\25\u00d7\3\25\3\25\3\26\6\26\u00dd\n\26\r\26\16\26\u00de\3"+
		"\26\3\26\3\27\5\27\u00e4\n\27\3\27\3\27\3\27\3\27\3\30\3\30\3\30\3\30"+
		"\3\30\3\31\3\31\3\31\3\31\3\31\3\31\3\32\3\32\3\32\3\32\3\32\3\33\3\33"+
		"\7\33\u00fc\n\33\f\33\16\33\u00ff\13\33\3\33\3\33\3\33\7\33\u0104\n\33"+
		"\f\33\16\33\u0107\13\33\3\33\3\33\3\33\3\34\3\34\3\34\3\35\6\35\u0110"+
		"\n\35\r\35\16\35\u0111\3\35\3\35\3\36\5\36\u0117\n\36\3\36\3\36\3\36\3"+
		"\36\3\36\3\36\3\37\3\37\3\37\5\37\u0122\n\37\3\37\3\37\3\37\7\37\u0127"+
		"\n\37\f\37\16\37\u012a\13\37\3 \3 \3!\3!\3\"\3\"\3#\3#\3$\6$\u0135\n$"+
		"\r$\16$\u0136\3%\6%\u013a\n%\r%\16%\u013b\3%\3%\3%\3%\3&\3&\3&\3&\3&\3"+
		"&\3&\3&\3&\3\'\5\'\u014c\n\'\3\'\3\'\3\'\3\'\3\'\3\'\3(\3(\3(\7(\u0157"+
		"\n(\f(\16(\u015a\13(\3(\3(\3(\3(\3)\3)\3)\3)\3)\7)\u0165\n)\f)\16)\u0168"+
		"\13)\3)\3)\3)\7)\u016d\n)\f)\16)\u0170\13)\3)\3)\3)\3)\3*\3*\3*\3*\3*"+
		"\7*\u017b\n*\f*\16*\u017e\13*\3*\3*\3*\3*\3+\3+\3+\3+\3+\3+\3+\7+\u018b"+
		"\n+\f+\16+\u018e\13+\3+\3+\3+\3+\3,\3,\3,\3,\3,\7,\u0199\n,\f,\16,\u019c"+
		"\13,\3,\3,\3,\3,\3-\3-\3-\3-\3-\3-\3-\3-\7-\u01aa\n-\f-\16-\u01ad\13-"+
		"\3-\3-\3-\3-\3.\3.\3.\3/\3/\3/\3\60\6\60\u01ba\n\60\r\60\16\60\u01bb\3"+
		"\60\3\60\3\61\3\61\3\61\3\61\3\61\3\61\3\61\3\61\3\62\3\62\3\62\3\62\3"+
		"\63\3\63\3\63\3\63\3\64\5\64\u01d1\n\64\3\64\3\64\6\64\u01d5\n\64\r\64"+
		"\16\64\u01d6\3\64\3\64\3\65\6\65\u01dc\n\65\r\65\16\65\u01dd\3\65\3\65"+
		"\3\66\5\66\u01e3\n\66\3\66\3\66\3\66\3\66\3\66\3\66\3\66\3\67\3\67\3\67"+
		"\5\67\u01ef\n\67\3\67\3\67\3\67\7\67\u01f4\n\67\f\67\16\67\u01f7\13\67"+
		"\38\68\u01fa\n8\r8\168\u01fb\39\39\79\u0200\n9\f9\169\u0203\139\39\59"+
		"\u0206\n9\39\39\39\39\39\3:\6:\u020e\n:\r:\16:\u020f\3:\3:\3:\3:\3;\5"+
		";\u0217\n;\3;\3;\3;\3<\3<\3<\3<\3<\3<\3<\3=\3=\3=\5=\u0226\n=\3=\3=\3"+
		"=\7=\u022b\n=\f=\16=\u022e\13=\3=\3=\3=\3>\3>\3>\3?\3?\3?\3@\3@\3@\3A"+
		"\3A\3A\3B\6B\u0240\nB\rB\16B\u0241\3B\3B\t\u00fd\u0105\u0136\u01bb\u01d6"+
		"\u01fb\u0241\2C\b\2\n\2\f\2\16\2\20\2\22\2\24\2\26\2\30\2\32\2\34\2\36"+
		"\2 \2\"\2$\2&\2(\2*\2,\2.\3\60\4\62\5\64\6\66\78\b:\t<\n>\13@\fB\rD\16"+
		"F\17H\20J\21L\22N\23P\24R\25T\26V\27X\30Z\31\\\32^\33`\34b\35d\36f\37"+
		"h\2j\2l\2n p!r\"t#v$x%z&|\'~(\u0080)\u0082*\u0084+\u0086,\u0088-\b\2\3"+
		"\4\5\6\7\31\4\2CCcc\4\2EEee\4\2FFff\4\2GGgg\4\2HHhh\4\2JJjj\4\2KKkk\4"+
		"\2NNnn\4\2UUuu\4\2VVvv\4\2WWww\4\2YYyy\4\2C\\c|\6\2\13\13\"\"\u00a2\u00a2"+
		"\uff01\uff01\5\2\f\f\17\17))\5\2\f\f\17\17$$\b\2\f\f\17\17$$))}}\177\177"+
		"\4\2\f\f\17\17\4\2&&@@\6\2\f\f\17\17]]__\5\2\f\f\17\17*+\4\2//aa\4\2/"+
		"\60aa\2\u0261\2.\3\2\2\2\2\60\3\2\2\2\2\62\3\2\2\2\2\64\3\2\2\2\2\66\3"+
		"\2\2\2\28\3\2\2\2\2:\3\2\2\2\2<\3\2\2\2\3>\3\2\2\2\3@\3\2\2\2\3B\3\2\2"+
		"\2\3D\3\2\2\2\3F\3\2\2\2\3H\3\2\2\2\3J\3\2\2\2\3L\3\2\2\2\4N\3\2\2\2\4"+
		"P\3\2\2\2\4R\3\2\2\2\4T\3\2\2\2\4V\3\2\2\2\4X\3\2\2\2\4Z\3\2\2\2\4\\\3"+
		"\2\2\2\4^\3\2\2\2\4`\3\2\2\2\4b\3\2\2\2\4d\3\2\2\2\5f\3\2\2\2\5h\3\2\2"+
		"\2\5j\3\2\2\2\5l\3\2\2\2\6n\3\2\2\2\6p\3\2\2\2\6r\3\2\2\2\6t\3\2\2\2\7"+
		"v\3\2\2\2\7x\3\2\2\2\7z\3\2\2\2\7|\3\2\2\2\7~\3\2\2\2\7\u0080\3\2\2\2"+
		"\7\u0082\3\2\2\2\7\u0084\3\2\2\2\7\u0086\3\2\2\2\7\u0088\3\2\2\2\b\u008a"+
		"\3\2\2\2\n\u008c\3\2\2\2\f\u008e\3\2\2\2\16\u0090\3\2\2\2\20\u0092\3\2"+
		"\2\2\22\u0094\3\2\2\2\24\u0096\3\2\2\2\26\u0098\3\2\2\2\30\u009a\3\2\2"+
		"\2\32\u009c\3\2\2\2\34\u009e\3\2\2\2\36\u00a0\3\2\2\2 \u00a2\3\2\2\2\""+
		"\u00a4\3\2\2\2$\u00a6\3\2\2\2&\u00a8\3\2\2\2(\u00c1\3\2\2\2*\u00c3\3\2"+
		"\2\2,\u00cf\3\2\2\2.\u00d3\3\2\2\2\60\u00dc\3\2\2\2\62\u00e3\3\2\2\2\64"+
		"\u00e9\3\2\2\2\66\u00ee\3\2\2\28\u00f4\3\2\2\2:\u00f9\3\2\2\2<\u010b\3"+
		"\2\2\2>\u010f\3\2\2\2@\u0116\3\2\2\2B\u0121\3\2\2\2D\u012b\3\2\2\2F\u012d"+
		"\3\2\2\2H\u012f\3\2\2\2J\u0131\3\2\2\2L\u0134\3\2\2\2N\u0139\3\2\2\2P"+
		"\u0141\3\2\2\2R\u014b\3\2\2\2T\u0153\3\2\2\2V\u015f\3\2\2\2X\u0175\3\2"+
		"\2\2Z\u0183\3\2\2\2\\\u0193\3\2\2\2^\u01a1\3\2\2\2`\u01b2\3\2\2\2b\u01b5"+
		"\3\2\2\2d\u01b9\3\2\2\2f\u01bf\3\2\2\2h\u01c7\3\2\2\2j\u01cb\3\2\2\2l"+
		"\u01d4\3\2\2\2n\u01db\3\2\2\2p\u01e2\3\2\2\2r\u01ee\3\2\2\2t\u01f9\3\2"+
		"\2\2v\u01fd\3\2\2\2x\u020d\3\2\2\2z\u0216\3\2\2\2|\u021b\3\2\2\2~\u0225"+
		"\3\2\2\2\u0080\u0232\3\2\2\2\u0082\u0235\3\2\2\2\u0084\u0238\3\2\2\2\u0086"+
		"\u023b\3\2\2\2\u0088\u023f\3\2\2\2\u008a\u008b\t\2\2\2\u008b\t\3\2\2\2"+
		"\u008c\u008d\t\3\2\2\u008d\13\3\2\2\2\u008e\u008f\t\4\2\2\u008f\r\3\2"+
		"\2\2\u0090\u0091\t\5\2\2\u0091\17\3\2\2\2\u0092\u0093\t\6\2\2\u0093\21"+
		"\3\2\2\2\u0094\u0095\t\7\2\2\u0095\23\3\2\2\2\u0096\u0097\t\b\2\2\u0097"+
		"\25\3\2\2\2\u0098\u0099\t\t\2\2\u0099\27\3\2\2\2\u009a\u009b\t\n\2\2\u009b"+
		"\31\3\2\2\2\u009c\u009d\t\13\2\2\u009d\33\3\2\2\2\u009e\u009f\t\f\2\2"+
		"\u009f\35\3\2\2\2\u00a0\u00a1\t\r\2\2\u00a1\37\3\2\2\2\u00a2\u00a3\t\16"+
		"\2\2\u00a3!\3\2\2\2\u00a4\u00a5\4\62;\2\u00a5#\3\2\2\2\u00a6\u00a7\t\17"+
		"\2\2\u00a7%\3\2\2\2\u00a8\u00ac\7}\2\2\u00a9\u00ab\5$\20\2\u00aa\u00a9"+
		"\3\2\2\2\u00ab\u00ae\3\2\2\2\u00ac\u00aa\3\2\2\2\u00ac\u00ad\3\2\2\2\u00ad"+
		"\u00af\3\2\2\2\u00ae\u00ac\3\2\2\2\u00af\u00b0\7\177\2\2\u00b0\'\3\2\2"+
		"\2\u00b1\u00b5\7)\2\2\u00b2\u00b4\n\20\2\2\u00b3\u00b2\3\2\2\2\u00b4\u00b7"+
		"\3\2\2\2\u00b5\u00b3\3\2\2\2\u00b5\u00b6\3\2\2\2\u00b6\u00b8\3\2\2\2\u00b7"+
		"\u00b5\3\2\2\2\u00b8\u00c2\7)\2\2\u00b9\u00bd\7$\2\2\u00ba\u00bc\n\21"+
		"\2\2\u00bb\u00ba\3\2\2\2\u00bc\u00bf\3\2\2\2\u00bd\u00bb\3\2\2\2\u00bd"+
		"\u00be\3\2\2\2\u00be\u00c0\3\2\2\2\u00bf\u00bd\3\2\2\2\u00c0\u00c2\7$"+
		"\2\2\u00c1\u00b1\3\2\2\2\u00c1\u00b9\3\2\2\2\u00c2)\3\2\2\2\u00c3\u00c4"+
		"\7B\2\2\u00c4\u00c8\7}\2\2\u00c5\u00c9\5(\22\2\u00c6\u00c9\n\22\2\2\u00c7"+
		"\u00c9\5&\21\2\u00c8\u00c5\3\2\2\2\u00c8\u00c6\3\2\2\2\u00c8\u00c7\3\2"+
		"\2\2\u00c9\u00ca\3\2\2\2\u00ca\u00c8\3\2\2\2\u00ca\u00cb\3\2\2\2\u00cb"+
		"\u00cd\3\2\2\2\u00cc\u00ce\7\177\2\2\u00cd\u00cc\3\2\2\2\u00cd\u00ce\3"+
		"\2\2\2\u00ce+\3\2\2\2\u00cf\u00d1\7^\2\2\u00d0\u00d2\n\23\2\2\u00d1\u00d0"+
		"\3\2\2\2\u00d1\u00d2\3\2\2\2\u00d2-\3\2\2\2\u00d3\u00d5\t\24\2\2\u00d4"+
		"\u00d6\n\23\2\2\u00d5\u00d4\3\2\2\2\u00d6\u00d7\3\2\2\2\u00d7\u00d5\3"+
		"\2\2\2\u00d7\u00d8\3\2\2\2\u00d8\u00d9\3\2\2\2\u00d9\u00da\b\25\2\2\u00da"+
		"/\3\2\2\2\u00db\u00dd\5$\20\2\u00dc\u00db\3\2\2\2\u00dd\u00de\3\2\2\2"+
		"\u00de\u00dc\3\2\2\2\u00de\u00df\3\2\2\2\u00df\u00e0\3\2\2\2\u00e0\u00e1"+
		"\b\26\2\2\u00e1\61\3\2\2\2\u00e2\u00e4\7\17\2\2\u00e3\u00e2\3\2\2\2\u00e3"+
		"\u00e4\3\2\2\2\u00e4\u00e5\3\2\2\2\u00e5\u00e6\7\f\2\2\u00e6\u00e7\3\2"+
		"\2\2\u00e7\u00e8\b\27\2\2\u00e8\63\3\2\2\2\u00e9\u00ea\7%\2\2\u00ea\u00eb"+
		"\b\30\3\2\u00eb\u00ec\3\2\2\2\u00ec\u00ed\b\30\4\2\u00ed\65\3\2\2\2\u00ee"+
		"\u00ef\7/\2\2\u00ef\u00f0\6\31\2\2\u00f0\u00f1\b\31\5\2\u00f1\u00f2\3"+
		"\2\2\2\u00f2\u00f3\b\31\6\2\u00f3\67\3\2\2\2\u00f4\u00f5\7]\2\2\u00f5"+
		"\u00f6\6\32\3\2\u00f6\u00f7\3\2\2\2\u00f7\u00f8\b\32\7\2\u00f89\3\2\2"+
		"\2\u00f9\u00fd\7]\2\2\u00fa\u00fc\n\25\2\2\u00fb\u00fa\3\2\2\2\u00fc\u00ff"+
		"\3\2\2\2\u00fd\u00fe\3\2\2\2\u00fd\u00fb\3\2\2\2\u00fe\u0100\3\2\2\2\u00ff"+
		"\u00fd\3\2\2\2\u0100\u0101\7_\2\2\u0101\u0105\7*\2\2\u0102\u0104\n\26"+
		"\2\2\u0103\u0102\3\2\2\2\u0104\u0107\3\2\2\2\u0105\u0106\3\2\2\2\u0105"+
		"\u0103\3\2\2\2\u0106\u0108\3\2\2\2\u0107\u0105\3\2\2\2\u0108\u0109\7+"+
		"\2\2\u0109\u010a\b\33\b\2\u010a;\3\2\2\2\u010b\u010c\13\2\2\2\u010c\u010d"+
		"\b\34\t\2\u010d=\3\2\2\2\u010e\u0110\5$\20\2\u010f\u010e\3\2\2\2\u0110"+
		"\u0111\3\2\2\2\u0111\u010f\3\2\2\2\u0111\u0112\3\2\2\2\u0112\u0113\3\2"+
		"\2\2\u0113\u0114\b\35\2\2\u0114?\3\2\2\2\u0115\u0117\7\17\2\2\u0116\u0115"+
		"\3\2\2\2\u0116\u0117\3\2\2\2\u0117\u0118\3\2\2\2\u0118\u0119\7\f\2\2\u0119"+
		"\u011a\b\36\n\2\u011a\u011b\3\2\2\2\u011b\u011c\b\36\2\2\u011c\u011d\b"+
		"\36\13\2\u011dA\3\2\2\2\u011e\u0122\5 \16\2\u011f\u0122\5\"\17\2\u0120"+
		"\u0122\7a\2\2\u0121\u011e\3\2\2\2\u0121\u011f\3\2\2\2\u0121\u0120\3\2"+
		"\2\2\u0122\u0128\3\2\2\2\u0123\u0127\5 \16\2\u0124\u0127\5\"\17\2\u0125"+
		"\u0127\t\27\2\2\u0126\u0123\3\2\2\2\u0126\u0124\3\2\2\2\u0126\u0125\3"+
		"\2\2\2\u0127\u012a\3\2\2\2\u0128\u0126\3\2\2\2\u0128\u0129\3\2\2\2\u0129"+
		"C\3\2\2\2\u012a\u0128\3\2\2\2\u012b\u012c\7\60\2\2\u012cE\3\2\2\2\u012d"+
		"\u012e\7*\2\2\u012eG\3\2\2\2\u012f\u0130\7+\2\2\u0130I\3\2\2\2\u0131\u0132"+
		"\7.\2\2\u0132K\3\2\2\2\u0133\u0135\n\23\2\2\u0134\u0133\3\2\2\2\u0135"+
		"\u0136\3\2\2\2\u0136\u0137\3\2\2\2\u0136\u0134\3\2\2\2\u0137M\3\2\2\2"+
		"\u0138\u013a\5$\20\2\u0139\u0138\3\2\2\2\u013a\u013b\3\2\2\2\u013b\u0139"+
		"\3\2\2\2\u013b\u013c\3\2\2\2\u013c\u013d\3\2\2\2\u013d\u013e\6%\4\2\u013e"+
		"\u013f\3\2\2\2\u013f\u0140\b%\2\2\u0140O\3\2\2\2\u0141\u0142\7b\2\2\u0142"+
		"\u0143\7b\2\2\u0143\u0144\7b\2\2\u0144\u0145\3\2\2\2\u0145\u0146\6&\5"+
		"\2\u0146\u0147\b&\f\2\u0147\u0148\3\2\2\2\u0148\u0149\b&\r\2\u0149Q\3"+
		"\2\2\2\u014a\u014c\7\17\2\2\u014b\u014a\3\2\2\2\u014b\u014c\3\2\2\2\u014c"+
		"\u014d\3\2\2\2\u014d\u014e\7\f\2\2\u014e\u014f\b\'\16\2\u014f\u0150\3"+
		"\2\2\2\u0150\u0151\b\'\2\2\u0151\u0152\b\'\13\2\u0152S\3\2\2\2\u0153\u0154"+
		"\5\24\b\2\u0154\u0158\5\20\6\2\u0155\u0157\5$\20\2\u0156\u0155\3\2\2\2"+
		"\u0157\u015a\3\2\2\2\u0158\u0156\3\2\2\2\u0158\u0159\3\2\2\2\u0159\u015b"+
		"\3\2\2\2\u015a\u0158\3\2\2\2\u015b\u015c\7<\2\2\u015c\u015d\6(\6\2\u015d"+
		"\u015e\b(\17\2\u015eU\3\2\2\2\u015f\u0160\5\16\5\2\u0160\u0161\5\26\t"+
		"\2\u0161\u0162\5\30\n\2\u0162\u0166\5\16\5\2\u0163\u0165\5$\20\2\u0164"+
		"\u0163\3\2\2\2\u0165\u0168\3\2\2\2\u0166\u0164\3\2\2\2\u0166\u0167\3\2"+
		"\2\2\u0167\u0169\3\2\2\2\u0168\u0166\3\2\2\2\u0169\u016a\5\24\b\2\u016a"+
		"\u016e\5\20\6\2\u016b\u016d\5$\20\2\u016c\u016b\3\2\2\2\u016d\u0170\3"+
		"\2\2\2\u016e\u016c\3\2\2\2\u016e\u016f\3\2\2\2\u016f\u0171\3\2\2\2\u0170"+
		"\u016e\3\2\2\2\u0171\u0172\7<\2\2\u0172\u0173\6)\7\2\u0173\u0174\b)\20"+
		"\2\u0174W\3\2\2\2\u0175\u0176\5\16\5\2\u0176\u0177\5\26\t\2\u0177\u0178"+
		"\5\30\n\2\u0178\u017c\5\16\5\2\u0179\u017b\5$\20\2\u017a\u0179\3\2\2\2"+
		"\u017b\u017e\3\2\2\2\u017c\u017a\3\2\2\2\u017c\u017d\3\2\2\2\u017d\u017f"+
		"\3\2\2\2\u017e\u017c\3\2\2\2\u017f\u0180\7<\2\2\u0180\u0181\6*\b\2\u0181"+
		"\u0182\b*\21\2\u0182Y\3\2\2\2\u0183\u0184\5\30\n\2\u0184\u0185\5\36\r"+
		"\2\u0185\u0186\5\24\b\2\u0186\u0187\5\32\13\2\u0187\u0188\5\n\3\2\u0188"+
		"\u018c\5\22\7\2\u0189\u018b\5$\20\2\u018a\u0189\3\2\2\2\u018b\u018e\3"+
		"\2\2\2\u018c\u018a\3\2\2\2\u018c\u018d\3\2\2\2\u018d\u018f\3\2\2\2\u018e"+
		"\u018c\3\2\2\2\u018f\u0190\7<\2\2\u0190\u0191\6+\t\2\u0191\u0192\b+\22"+
		"\2\u0192[\3\2\2\2\u0193\u0194\5\n\3\2\u0194\u0195\5\b\2\2\u0195\u0196"+
		"\5\30\n\2\u0196\u019a\5\16\5\2\u0197\u0199\5$\20\2\u0198\u0197\3\2\2\2"+
		"\u0199\u019c\3\2\2\2\u019a\u0198\3\2\2\2\u019a\u019b\3\2\2\2\u019b\u019d"+
		"\3\2\2\2\u019c\u019a\3\2\2\2\u019d\u019e\7<\2\2\u019e\u019f\6,\n\2\u019f"+
		"\u01a0\b,\23\2\u01a0]\3\2\2\2\u01a1\u01a2\5\f\4\2\u01a2\u01a3\5\16\5\2"+
		"\u01a3\u01a4\5\20\6\2\u01a4\u01a5\5\b\2\2\u01a5\u01a6\5\34\f\2\u01a6\u01a7"+
		"\5\26\t\2\u01a7\u01ab\5\32\13\2\u01a8\u01aa\5$\20\2\u01a9\u01a8\3\2\2"+
		"\2\u01aa\u01ad\3\2\2\2\u01ab\u01a9\3\2\2\2\u01ab\u01ac\3\2\2\2\u01ac\u01ae"+
		"\3\2\2\2\u01ad\u01ab\3\2\2\2\u01ae\u01af\7<\2\2\u01af\u01b0\6-\13\2\u01b0"+
		"\u01b1\b-\24\2\u01b1_\3\2\2\2\u01b2\u01b3\5,\24\2\u01b3\u01b4\b.\25\2"+
		"\u01b4a\3\2\2\2\u01b5\u01b6\5*\23\2\u01b6\u01b7\b/\26\2\u01b7c\3\2\2\2"+
		"\u01b8\u01ba\n\23\2\2\u01b9\u01b8\3\2\2\2\u01ba\u01bb\3\2\2\2\u01bb\u01bc"+
		"\3\2\2\2\u01bb\u01b9\3\2\2\2\u01bc\u01bd\3\2\2\2\u01bd\u01be\b\60\27\2"+
		"\u01bee\3\2\2\2\u01bf\u01c0\7b\2\2\u01c0\u01c1\7b\2\2\u01c1\u01c2\7b\2"+
		"\2\u01c2\u01c3\3\2\2\2\u01c3\u01c4\b\61\30\2\u01c4\u01c5\3\2\2\2\u01c5"+
		"\u01c6\b\61\13\2\u01c6g\3\2\2\2\u01c7\u01c8\5,\24\2\u01c8\u01c9\3\2\2"+
		"\2\u01c9\u01ca\b\62\31\2\u01cai\3\2\2\2\u01cb\u01cc\5*\23\2\u01cc\u01cd"+
		"\3\2\2\2\u01cd\u01ce\b\63\32\2\u01cek\3\2\2\2\u01cf\u01d1\7\17\2\2\u01d0"+
		"\u01cf\3\2\2\2\u01d0\u01d1\3\2\2\2\u01d1\u01d2\3\2\2\2\u01d2\u01d5\7\f"+
		"\2\2\u01d3\u01d5\n\23\2\2\u01d4\u01d0\3\2\2\2\u01d4\u01d3\3\2\2\2\u01d5"+
		"\u01d6\3\2\2\2\u01d6\u01d7\3\2\2\2\u01d6\u01d4\3\2\2\2\u01d7\u01d8\3\2"+
		"\2\2\u01d8\u01d9\b\64\33\2\u01d9m\3\2\2\2\u01da\u01dc\5$\20\2\u01db\u01da"+
		"\3\2\2\2\u01dc\u01dd\3\2\2\2\u01dd\u01db\3\2\2\2\u01dd\u01de\3\2\2\2\u01de"+
		"\u01df\3\2\2\2\u01df\u01e0\b\65\2\2\u01e0o\3\2\2\2\u01e1\u01e3\7\17\2"+
		"\2\u01e2\u01e1\3\2\2\2\u01e2\u01e3\3\2\2\2\u01e3\u01e4\3\2\2\2\u01e4\u01e5"+
		"\7\f\2\2\u01e5\u01e6\b\66\34\2\u01e6\u01e7\b\66\35\2\u01e7\u01e8\3\2\2"+
		"\2\u01e8\u01e9\b\66\2\2\u01e9\u01ea\b\66\36\2\u01eaq\3\2\2\2\u01eb\u01ef"+
		"\5 \16\2\u01ec\u01ef\5\"\17\2\u01ed\u01ef\7a\2\2\u01ee\u01eb\3\2\2\2\u01ee"+
		"\u01ec\3\2\2\2\u01ee\u01ed\3\2\2\2\u01ef\u01f5\3\2\2\2\u01f0\u01f4\5 "+
		"\16\2\u01f1\u01f4\5\"\17\2\u01f2\u01f4\t\30\2\2\u01f3\u01f0\3\2\2\2\u01f3"+
		"\u01f1\3\2\2\2\u01f3\u01f2\3\2\2\2\u01f4\u01f7\3\2\2\2\u01f5\u01f3\3\2"+
		"\2\2\u01f5\u01f6\3\2\2\2\u01f6s\3\2\2\2\u01f7\u01f5\3\2\2\2\u01f8\u01fa"+
		"\n\23\2\2\u01f9\u01f8\3\2\2\2\u01fa\u01fb\3\2\2\2\u01fb\u01fc\3\2\2\2"+
		"\u01fb\u01f9\3\2\2\2\u01fcu\3\2\2\2\u01fd\u0201\t\24\2\2\u01fe\u0200\n"+
		"\23\2\2\u01ff\u01fe\3\2\2\2\u0200\u0203\3\2\2\2\u0201\u01ff\3\2\2\2\u0201"+
		"\u0202\3\2\2\2\u0202\u0205\3\2\2\2\u0203\u0201\3\2\2\2\u0204\u0206\7\17"+
		"\2\2\u0205\u0204\3\2\2\2\u0205\u0206\3\2\2\2\u0206\u0207\3\2\2\2\u0207"+
		"\u0208\7\f\2\2\u0208\u0209\69\f\2\u0209\u020a\3\2\2\2\u020a\u020b\b9\2"+
		"\2\u020bw\3\2\2\2\u020c\u020e\5$\20\2\u020d\u020c\3\2\2\2\u020e\u020f"+
		"\3\2\2\2\u020f\u020d\3\2\2\2\u020f\u0210\3\2\2\2\u0210\u0211\3\2\2\2\u0211"+
		"\u0212\6:\r\2\u0212\u0213\3\2\2\2\u0213\u0214\b:\2\2\u0214y\3\2\2\2\u0215"+
		"\u0217\7\17\2\2\u0216\u0215\3\2\2\2\u0216\u0217\3\2\2\2\u0217\u0218\3"+
		"\2\2\2\u0218\u0219\7\f\2\2\u0219\u021a\b;\37\2\u021a{\3\2\2\2\u021b\u021c"+
		"\7_\2\2\u021c\u021d\6<\16\2\u021d\u021e\b< \2\u021e\u021f\3\2\2\2\u021f"+
		"\u0220\b<\13\2\u0220\u0221\b<\13\2\u0221}\3\2\2\2\u0222\u0226\5 \16\2"+
		"\u0223\u0226\5\"\17\2\u0224\u0226\7a\2\2\u0225\u0222\3\2\2\2\u0225\u0223"+
		"\3\2\2\2\u0225\u0224\3\2\2\2\u0226\u022c\3\2\2\2\u0227\u022b\5 \16\2\u0228"+
		"\u022b\5\"\17\2\u0229\u022b\t\30\2\2\u022a\u0227\3\2\2\2\u022a\u0228\3"+
		"\2\2\2\u022a\u0229\3\2\2\2\u022b\u022e\3\2\2\2\u022c\u022a\3\2\2\2\u022c"+
		"\u022d\3\2\2\2\u022d\u022f\3\2\2\2\u022e\u022c\3\2\2\2\u022f\u0230\6="+
		"\17\2\u0230\u0231\b=!\2\u0231\177\3\2\2\2\u0232\u0233\7?\2\2\u0233\u0234"+
		"\b>\"\2\u0234\u0081\3\2\2\2\u0235\u0236\7~\2\2\u0236\u0237\b?#\2\u0237"+
		"\u0083\3\2\2\2\u0238\u0239\5,\24\2\u0239\u023a\b@$\2\u023a\u0085\3\2\2"+
		"\2\u023b\u023c\5*\23\2\u023c\u023d\bA%\2\u023d\u0087\3\2\2\2\u023e\u0240"+
		"\n\23\2\2\u023f\u023e\3\2\2\2\u0240\u0241\3\2\2\2\u0241\u0242\3\2\2\2"+
		"\u0241\u023f\3\2\2\2\u0242\u0243\3\2\2\2\u0243\u0244\bB&\2\u0244\u0089"+
		"\3\2\2\2\66\2\3\4\5\6\7\u00ac\u00b5\u00bd\u00c1\u00c8\u00ca\u00cd\u00d1"+
		"\u00d7\u00de\u00e3\u00fd\u0105\u0111\u0116\u0121\u0126\u0128\u0136\u013b"+
		"\u014b\u0158\u0166\u016e\u017c\u018c\u019a\u01ab\u01bb\u01d0\u01d4\u01d6"+
		"\u01dd\u01e2\u01ee\u01f3\u01f5\u01fb\u0201\u0205\u020f\u0216\u0225\u022a"+
		"\u022c\u0241\'\b\2\2\3\30\2\7\3\2\3\31\3\7\4\2\7\6\2\3\33\4\3\34\5\3\36"+
		"\6\6\2\2\3&\7\7\5\2\3\'\b\3(\t\3)\n\3*\13\3+\f\3,\r\3-\16\3.\17\3/\20"+
		"\3\60\21\3\61\22\t\34\2\t\35\2\t\36\2\3\66\23\3\66\24\7\7\2\3;\25\3<\26"+
		"\3=\27\3>\30\3?\31\3@\32\3A\33\3B\34";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}