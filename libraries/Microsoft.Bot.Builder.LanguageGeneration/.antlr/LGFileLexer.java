// Generated from d:\projects\BotFramework\botbuilder-dotnet\libraries\Microsoft.Bot.Builder.LanguageGeneration\LGFileLexer.g4 by ANTLR 4.7.1
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
		"NUMBER", "WHITESPACE", "STRING_LITERAL", "STRING_INTERPOLATION", "EXPRESSION_FRAGMENT", 
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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2-\u0247\b\1\b\1\b"+
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
		"\16\21\u00ae\13\21\3\21\3\21\3\21\7\21\u00b3\n\21\f\21\16\21\u00b6\13"+
		"\21\3\21\5\21\u00b9\n\21\3\22\3\22\3\22\3\22\7\22\u00bf\n\22\f\22\16\22"+
		"\u00c2\13\22\3\22\3\22\3\23\3\23\3\23\3\23\3\23\7\23\u00cb\n\23\f\23\16"+
		"\23\u00ce\13\23\3\23\3\23\3\24\3\24\5\24\u00d4\n\24\3\25\3\25\6\25\u00d8"+
		"\n\25\r\25\16\25\u00d9\3\25\3\25\3\26\6\26\u00df\n\26\r\26\16\26\u00e0"+
		"\3\26\3\26\3\27\5\27\u00e6\n\27\3\27\3\27\3\27\3\27\3\30\3\30\3\30\3\30"+
		"\3\30\3\31\3\31\3\31\3\31\3\31\3\31\3\32\3\32\3\32\3\32\3\32\3\33\3\33"+
		"\7\33\u00fe\n\33\f\33\16\33\u0101\13\33\3\33\3\33\3\33\7\33\u0106\n\33"+
		"\f\33\16\33\u0109\13\33\3\33\3\33\3\33\3\34\3\34\3\34\3\35\6\35\u0112"+
		"\n\35\r\35\16\35\u0113\3\35\3\35\3\36\5\36\u0119\n\36\3\36\3\36\3\36\3"+
		"\36\3\36\3\36\3\37\3\37\3\37\5\37\u0124\n\37\3\37\3\37\3\37\7\37\u0129"+
		"\n\37\f\37\16\37\u012c\13\37\3 \3 \3!\3!\3\"\3\"\3#\3#\3$\6$\u0137\n$"+
		"\r$\16$\u0138\3%\6%\u013c\n%\r%\16%\u013d\3%\3%\3%\3%\3&\3&\3&\3&\3&\3"+
		"&\3&\3&\3&\3\'\5\'\u014e\n\'\3\'\3\'\3\'\3\'\3\'\3\'\3(\3(\3(\7(\u0159"+
		"\n(\f(\16(\u015c\13(\3(\3(\3(\3(\3)\3)\3)\3)\3)\7)\u0167\n)\f)\16)\u016a"+
		"\13)\3)\3)\3)\7)\u016f\n)\f)\16)\u0172\13)\3)\3)\3)\3)\3*\3*\3*\3*\3*"+
		"\7*\u017d\n*\f*\16*\u0180\13*\3*\3*\3*\3*\3+\3+\3+\3+\3+\3+\3+\7+\u018d"+
		"\n+\f+\16+\u0190\13+\3+\3+\3+\3+\3,\3,\3,\3,\3,\7,\u019b\n,\f,\16,\u019e"+
		"\13,\3,\3,\3,\3,\3-\3-\3-\3-\3-\3-\3-\3-\7-\u01ac\n-\f-\16-\u01af\13-"+
		"\3-\3-\3-\3-\3.\3.\3.\3/\3/\3/\3\60\6\60\u01bc\n\60\r\60\16\60\u01bd\3"+
		"\60\3\60\3\61\3\61\3\61\3\61\3\61\3\61\3\61\3\61\3\62\3\62\3\62\3\62\3"+
		"\63\3\63\3\63\3\63\3\64\5\64\u01d3\n\64\3\64\3\64\6\64\u01d7\n\64\r\64"+
		"\16\64\u01d8\3\64\3\64\3\65\6\65\u01de\n\65\r\65\16\65\u01df\3\65\3\65"+
		"\3\66\5\66\u01e5\n\66\3\66\3\66\3\66\3\66\3\66\3\66\3\66\3\67\3\67\3\67"+
		"\5\67\u01f1\n\67\3\67\3\67\3\67\7\67\u01f6\n\67\f\67\16\67\u01f9\13\67"+
		"\38\68\u01fc\n8\r8\168\u01fd\39\39\79\u0202\n9\f9\169\u0205\139\39\59"+
		"\u0208\n9\39\39\39\39\39\3:\6:\u0210\n:\r:\16:\u0211\3:\3:\3:\3:\3;\5"+
		";\u0219\n;\3;\3;\3;\3<\3<\3<\3<\3<\3<\3<\3=\3=\3=\5=\u0228\n=\3=\3=\3"+
		"=\7=\u022d\n=\f=\16=\u0230\13=\3=\3=\3=\3>\3>\3>\3?\3?\3?\3@\3@\3@\3A"+
		"\3A\3A\3B\6B\u0242\nB\rB\16B\u0243\3B\3B\n\u00cc\u00ff\u0107\u0138\u01bd"+
		"\u01d8\u01fd\u0243\2C\b\2\n\2\f\2\16\2\20\2\22\2\24\2\26\2\30\2\32\2\34"+
		"\2\36\2 \2\"\2$\2&\2(\2*\2,\2.\3\60\4\62\5\64\6\66\78\b:\t<\n>\13@\fB"+
		"\rD\16F\17H\20J\21L\22N\23P\24R\25T\26V\27X\30Z\31\\\32^\33`\34b\35d\36"+
		"f\37h\2j\2l\2n p!r\"t#v$x%z&|\'~(\u0080)\u0082*\u0084+\u0086,\u0088-\b"+
		"\2\3\4\5\6\7\32\4\2CCcc\4\2EEee\4\2FFff\4\2GGgg\4\2HHhh\4\2JJjj\4\2KK"+
		"kk\4\2NNnn\4\2UUuu\4\2VVvv\4\2WWww\4\2YYyy\4\2C\\c|\6\2\13\13\"\"\u00a2"+
		"\u00a2\uff01\uff01\5\2\f\f\17\17))\5\2\f\f\17\17$$\3\2bb\t\2\f\f\17\17"+
		"$$))bb}}\177\177\4\2\f\f\17\17\4\2&&@@\6\2\f\f\17\17]]__\5\2\f\f\17\17"+
		"*+\4\2//aa\4\2/\60aa\2\u0263\2.\3\2\2\2\2\60\3\2\2\2\2\62\3\2\2\2\2\64"+
		"\3\2\2\2\2\66\3\2\2\2\28\3\2\2\2\2:\3\2\2\2\2<\3\2\2\2\3>\3\2\2\2\3@\3"+
		"\2\2\2\3B\3\2\2\2\3D\3\2\2\2\3F\3\2\2\2\3H\3\2\2\2\3J\3\2\2\2\3L\3\2\2"+
		"\2\4N\3\2\2\2\4P\3\2\2\2\4R\3\2\2\2\4T\3\2\2\2\4V\3\2\2\2\4X\3\2\2\2\4"+
		"Z\3\2\2\2\4\\\3\2\2\2\4^\3\2\2\2\4`\3\2\2\2\4b\3\2\2\2\4d\3\2\2\2\5f\3"+
		"\2\2\2\5h\3\2\2\2\5j\3\2\2\2\5l\3\2\2\2\6n\3\2\2\2\6p\3\2\2\2\6r\3\2\2"+
		"\2\6t\3\2\2\2\7v\3\2\2\2\7x\3\2\2\2\7z\3\2\2\2\7|\3\2\2\2\7~\3\2\2\2\7"+
		"\u0080\3\2\2\2\7\u0082\3\2\2\2\7\u0084\3\2\2\2\7\u0086\3\2\2\2\7\u0088"+
		"\3\2\2\2\b\u008a\3\2\2\2\n\u008c\3\2\2\2\f\u008e\3\2\2\2\16\u0090\3\2"+
		"\2\2\20\u0092\3\2\2\2\22\u0094\3\2\2\2\24\u0096\3\2\2\2\26\u0098\3\2\2"+
		"\2\30\u009a\3\2\2\2\32\u009c\3\2\2\2\34\u009e\3\2\2\2\36\u00a0\3\2\2\2"+
		" \u00a2\3\2\2\2\"\u00a4\3\2\2\2$\u00a6\3\2\2\2&\u00b8\3\2\2\2(\u00ba\3"+
		"\2\2\2*\u00c5\3\2\2\2,\u00d1\3\2\2\2.\u00d5\3\2\2\2\60\u00de\3\2\2\2\62"+
		"\u00e5\3\2\2\2\64\u00eb\3\2\2\2\66\u00f0\3\2\2\28\u00f6\3\2\2\2:\u00fb"+
		"\3\2\2\2<\u010d\3\2\2\2>\u0111\3\2\2\2@\u0118\3\2\2\2B\u0123\3\2\2\2D"+
		"\u012d\3\2\2\2F\u012f\3\2\2\2H\u0131\3\2\2\2J\u0133\3\2\2\2L\u0136\3\2"+
		"\2\2N\u013b\3\2\2\2P\u0143\3\2\2\2R\u014d\3\2\2\2T\u0155\3\2\2\2V\u0161"+
		"\3\2\2\2X\u0177\3\2\2\2Z\u0185\3\2\2\2\\\u0195\3\2\2\2^\u01a3\3\2\2\2"+
		"`\u01b4\3\2\2\2b\u01b7\3\2\2\2d\u01bb\3\2\2\2f\u01c1\3\2\2\2h\u01c9\3"+
		"\2\2\2j\u01cd\3\2\2\2l\u01d6\3\2\2\2n\u01dd\3\2\2\2p\u01e4\3\2\2\2r\u01f0"+
		"\3\2\2\2t\u01fb\3\2\2\2v\u01ff\3\2\2\2x\u020f\3\2\2\2z\u0218\3\2\2\2|"+
		"\u021d\3\2\2\2~\u0227\3\2\2\2\u0080\u0234\3\2\2\2\u0082\u0237\3\2\2\2"+
		"\u0084\u023a\3\2\2\2\u0086\u023d\3\2\2\2\u0088\u0241\3\2\2\2\u008a\u008b"+
		"\t\2\2\2\u008b\t\3\2\2\2\u008c\u008d\t\3\2\2\u008d\13\3\2\2\2\u008e\u008f"+
		"\t\4\2\2\u008f\r\3\2\2\2\u0090\u0091\t\5\2\2\u0091\17\3\2\2\2\u0092\u0093"+
		"\t\6\2\2\u0093\21\3\2\2\2\u0094\u0095\t\7\2\2\u0095\23\3\2\2\2\u0096\u0097"+
		"\t\b\2\2\u0097\25\3\2\2\2\u0098\u0099\t\t\2\2\u0099\27\3\2\2\2\u009a\u009b"+
		"\t\n\2\2\u009b\31\3\2\2\2\u009c\u009d\t\13\2\2\u009d\33\3\2\2\2\u009e"+
		"\u009f\t\f\2\2\u009f\35\3\2\2\2\u00a0\u00a1\t\r\2\2\u00a1\37\3\2\2\2\u00a2"+
		"\u00a3\t\16\2\2\u00a3!\3\2\2\2\u00a4\u00a5\4\62;\2\u00a5#\3\2\2\2\u00a6"+
		"\u00a7\t\17\2\2\u00a7%\3\2\2\2\u00a8\u00ac\7)\2\2\u00a9\u00ab\n\20\2\2"+
		"\u00aa\u00a9\3\2\2\2\u00ab\u00ae\3\2\2\2\u00ac\u00aa\3\2\2\2\u00ac\u00ad"+
		"\3\2\2\2\u00ad\u00af\3\2\2\2\u00ae\u00ac\3\2\2\2\u00af\u00b9\7)\2\2\u00b0"+
		"\u00b4\7$\2\2\u00b1\u00b3\n\21\2\2\u00b2\u00b1\3\2\2\2\u00b3\u00b6\3\2"+
		"\2\2\u00b4\u00b2\3\2\2\2\u00b4\u00b5\3\2\2\2\u00b5\u00b7\3\2\2\2\u00b6"+
		"\u00b4\3\2\2\2\u00b7\u00b9\7$\2\2\u00b8\u00a8\3\2\2\2\u00b8\u00b0\3\2"+
		"\2\2\u00b9\'\3\2\2\2\u00ba\u00c0\7b\2\2\u00bb\u00bc\7^\2\2\u00bc\u00bf"+
		"\7b\2\2\u00bd\u00bf\n\22\2\2\u00be\u00bb\3\2\2\2\u00be\u00bd\3\2\2\2\u00bf"+
		"\u00c2\3\2\2\2\u00c0\u00be\3\2\2\2\u00c0\u00c1\3\2\2\2\u00c1\u00c3\3\2"+
		"\2\2\u00c2\u00c0\3\2\2\2\u00c3\u00c4\7b\2\2\u00c4)\3\2\2\2\u00c5\u00c6"+
		"\7B\2\2\u00c6\u00cc\7}\2\2\u00c7\u00cb\5&\21\2\u00c8\u00cb\5(\22\2\u00c9"+
		"\u00cb\n\23\2\2\u00ca\u00c7\3\2\2\2\u00ca\u00c8\3\2\2\2\u00ca\u00c9\3"+
		"\2\2\2\u00cb\u00ce\3\2\2\2\u00cc\u00cd\3\2\2\2\u00cc\u00ca\3\2\2\2\u00cd"+
		"\u00cf\3\2\2\2\u00ce\u00cc\3\2\2\2\u00cf\u00d0\7\177\2\2\u00d0+\3\2\2"+
		"\2\u00d1\u00d3\7^\2\2\u00d2\u00d4\n\24\2\2\u00d3\u00d2\3\2\2\2\u00d3\u00d4"+
		"\3\2\2\2\u00d4-\3\2\2\2\u00d5\u00d7\t\25\2\2\u00d6\u00d8\n\24\2\2\u00d7"+
		"\u00d6\3\2\2\2\u00d8\u00d9\3\2\2\2\u00d9\u00d7\3\2\2\2\u00d9\u00da\3\2"+
		"\2\2\u00da\u00db\3\2\2\2\u00db\u00dc\b\25\2\2\u00dc/\3\2\2\2\u00dd\u00df"+
		"\5$\20\2\u00de\u00dd\3\2\2\2\u00df\u00e0\3\2\2\2\u00e0\u00de\3\2\2\2\u00e0"+
		"\u00e1\3\2\2\2\u00e1\u00e2\3\2\2\2\u00e2\u00e3\b\26\2\2\u00e3\61\3\2\2"+
		"\2\u00e4\u00e6\7\17\2\2\u00e5\u00e4\3\2\2\2\u00e5\u00e6\3\2\2\2\u00e6"+
		"\u00e7\3\2\2\2\u00e7\u00e8\7\f\2\2\u00e8\u00e9\3\2\2\2\u00e9\u00ea\b\27"+
		"\2\2\u00ea\63\3\2\2\2\u00eb\u00ec\7%\2\2\u00ec\u00ed\b\30\3\2\u00ed\u00ee"+
		"\3\2\2\2\u00ee\u00ef\b\30\4\2\u00ef\65\3\2\2\2\u00f0\u00f1\7/\2\2\u00f1"+
		"\u00f2\6\31\2\2\u00f2\u00f3\b\31\5\2\u00f3\u00f4\3\2\2\2\u00f4\u00f5\b"+
		"\31\6\2\u00f5\67\3\2\2\2\u00f6\u00f7\7]\2\2\u00f7\u00f8\6\32\3\2\u00f8"+
		"\u00f9\3\2\2\2\u00f9\u00fa\b\32\7\2\u00fa9\3\2\2\2\u00fb\u00ff\7]\2\2"+
		"\u00fc\u00fe\n\26\2\2\u00fd\u00fc\3\2\2\2\u00fe\u0101\3\2\2\2\u00ff\u0100"+
		"\3\2\2\2\u00ff\u00fd\3\2\2\2\u0100\u0102\3\2\2\2\u0101\u00ff\3\2\2\2\u0102"+
		"\u0103\7_\2\2\u0103\u0107\7*\2\2\u0104\u0106\n\27\2\2\u0105\u0104\3\2"+
		"\2\2\u0106\u0109\3\2\2\2\u0107\u0108\3\2\2\2\u0107\u0105\3\2\2\2\u0108"+
		"\u010a\3\2\2\2\u0109\u0107\3\2\2\2\u010a\u010b\7+\2\2\u010b\u010c\b\33"+
		"\b\2\u010c;\3\2\2\2\u010d\u010e\13\2\2\2\u010e\u010f\b\34\t\2\u010f=\3"+
		"\2\2\2\u0110\u0112\5$\20\2\u0111\u0110\3\2\2\2\u0112\u0113\3\2\2\2\u0113"+
		"\u0111\3\2\2\2\u0113\u0114\3\2\2\2\u0114\u0115\3\2\2\2\u0115\u0116\b\35"+
		"\2\2\u0116?\3\2\2\2\u0117\u0119\7\17\2\2\u0118\u0117\3\2\2\2\u0118\u0119"+
		"\3\2\2\2\u0119\u011a\3\2\2\2\u011a\u011b\7\f\2\2\u011b\u011c\b\36\n\2"+
		"\u011c\u011d\3\2\2\2\u011d\u011e\b\36\2\2\u011e\u011f\b\36\13\2\u011f"+
		"A\3\2\2\2\u0120\u0124\5 \16\2\u0121\u0124\5\"\17\2\u0122\u0124\7a\2\2"+
		"\u0123\u0120\3\2\2\2\u0123\u0121\3\2\2\2\u0123\u0122\3\2\2\2\u0124\u012a"+
		"\3\2\2\2\u0125\u0129\5 \16\2\u0126\u0129\5\"\17\2\u0127\u0129\t\30\2\2"+
		"\u0128\u0125\3\2\2\2\u0128\u0126\3\2\2\2\u0128\u0127\3\2\2\2\u0129\u012c"+
		"\3\2\2\2\u012a\u0128\3\2\2\2\u012a\u012b\3\2\2\2\u012bC\3\2\2\2\u012c"+
		"\u012a\3\2\2\2\u012d\u012e\7\60\2\2\u012eE\3\2\2\2\u012f\u0130\7*\2\2"+
		"\u0130G\3\2\2\2\u0131\u0132\7+\2\2\u0132I\3\2\2\2\u0133\u0134\7.\2\2\u0134"+
		"K\3\2\2\2\u0135\u0137\n\24\2\2\u0136\u0135\3\2\2\2\u0137\u0138\3\2\2\2"+
		"\u0138\u0139\3\2\2\2\u0138\u0136\3\2\2\2\u0139M\3\2\2\2\u013a\u013c\5"+
		"$\20\2\u013b\u013a\3\2\2\2\u013c\u013d\3\2\2\2\u013d\u013b\3\2\2\2\u013d"+
		"\u013e\3\2\2\2\u013e\u013f\3\2\2\2\u013f\u0140\6%\4\2\u0140\u0141\3\2"+
		"\2\2\u0141\u0142\b%\2\2\u0142O\3\2\2\2\u0143\u0144\7b\2\2\u0144\u0145"+
		"\7b\2\2\u0145\u0146\7b\2\2\u0146\u0147\3\2\2\2\u0147\u0148\6&\5\2\u0148"+
		"\u0149\b&\f\2\u0149\u014a\3\2\2\2\u014a\u014b\b&\r\2\u014bQ\3\2\2\2\u014c"+
		"\u014e\7\17\2\2\u014d\u014c\3\2\2\2\u014d\u014e\3\2\2\2\u014e\u014f\3"+
		"\2\2\2\u014f\u0150\7\f\2\2\u0150\u0151\b\'\16\2\u0151\u0152\3\2\2\2\u0152"+
		"\u0153\b\'\2\2\u0153\u0154\b\'\13\2\u0154S\3\2\2\2\u0155\u0156\5\24\b"+
		"\2\u0156\u015a\5\20\6\2\u0157\u0159\5$\20\2\u0158\u0157\3\2\2\2\u0159"+
		"\u015c\3\2\2\2\u015a\u0158\3\2\2\2\u015a\u015b\3\2\2\2\u015b\u015d\3\2"+
		"\2\2\u015c\u015a\3\2\2\2\u015d\u015e\7<\2\2\u015e\u015f\6(\6\2\u015f\u0160"+
		"\b(\17\2\u0160U\3\2\2\2\u0161\u0162\5\16\5\2\u0162\u0163\5\26\t\2\u0163"+
		"\u0164\5\30\n\2\u0164\u0168\5\16\5\2\u0165\u0167\5$\20\2\u0166\u0165\3"+
		"\2\2\2\u0167\u016a\3\2\2\2\u0168\u0166\3\2\2\2\u0168\u0169\3\2\2\2\u0169"+
		"\u016b\3\2\2\2\u016a\u0168\3\2\2\2\u016b\u016c\5\24\b\2\u016c\u0170\5"+
		"\20\6\2\u016d\u016f\5$\20\2\u016e\u016d\3\2\2\2\u016f\u0172\3\2\2\2\u0170"+
		"\u016e\3\2\2\2\u0170\u0171\3\2\2\2\u0171\u0173\3\2\2\2\u0172\u0170\3\2"+
		"\2\2\u0173\u0174\7<\2\2\u0174\u0175\6)\7\2\u0175\u0176\b)\20\2\u0176W"+
		"\3\2\2\2\u0177\u0178\5\16\5\2\u0178\u0179\5\26\t\2\u0179\u017a\5\30\n"+
		"\2\u017a\u017e\5\16\5\2\u017b\u017d\5$\20\2\u017c\u017b\3\2\2\2\u017d"+
		"\u0180\3\2\2\2\u017e\u017c\3\2\2\2\u017e\u017f\3\2\2\2\u017f\u0181\3\2"+
		"\2\2\u0180\u017e\3\2\2\2\u0181\u0182\7<\2\2\u0182\u0183\6*\b\2\u0183\u0184"+
		"\b*\21\2\u0184Y\3\2\2\2\u0185\u0186\5\30\n\2\u0186\u0187\5\36\r\2\u0187"+
		"\u0188\5\24\b\2\u0188\u0189\5\32\13\2\u0189\u018a\5\n\3\2\u018a\u018e"+
		"\5\22\7\2\u018b\u018d\5$\20\2\u018c\u018b\3\2\2\2\u018d\u0190\3\2\2\2"+
		"\u018e\u018c\3\2\2\2\u018e\u018f\3\2\2\2\u018f\u0191\3\2\2\2\u0190\u018e"+
		"\3\2\2\2\u0191\u0192\7<\2\2\u0192\u0193\6+\t\2\u0193\u0194\b+\22\2\u0194"+
		"[\3\2\2\2\u0195\u0196\5\n\3\2\u0196\u0197\5\b\2\2\u0197\u0198\5\30\n\2"+
		"\u0198\u019c\5\16\5\2\u0199\u019b\5$\20\2\u019a\u0199\3\2\2\2\u019b\u019e"+
		"\3\2\2\2\u019c\u019a\3\2\2\2\u019c\u019d\3\2\2\2\u019d\u019f\3\2\2\2\u019e"+
		"\u019c\3\2\2\2\u019f\u01a0\7<\2\2\u01a0\u01a1\6,\n\2\u01a1\u01a2\b,\23"+
		"\2\u01a2]\3\2\2\2\u01a3\u01a4\5\f\4\2\u01a4\u01a5\5\16\5\2\u01a5\u01a6"+
		"\5\20\6\2\u01a6\u01a7\5\b\2\2\u01a7\u01a8\5\34\f\2\u01a8\u01a9\5\26\t"+
		"\2\u01a9\u01ad\5\32\13\2\u01aa\u01ac\5$\20\2\u01ab\u01aa\3\2\2\2\u01ac"+
		"\u01af\3\2\2\2\u01ad\u01ab\3\2\2\2\u01ad\u01ae\3\2\2\2\u01ae\u01b0\3\2"+
		"\2\2\u01af\u01ad\3\2\2\2\u01b0\u01b1\7<\2\2\u01b1\u01b2\6-\13\2\u01b2"+
		"\u01b3\b-\24\2\u01b3_\3\2\2\2\u01b4\u01b5\5,\24\2\u01b5\u01b6\b.\25\2"+
		"\u01b6a\3\2\2\2\u01b7\u01b8\5*\23\2\u01b8\u01b9\b/\26\2\u01b9c\3\2\2\2"+
		"\u01ba\u01bc\n\24\2\2\u01bb\u01ba\3\2\2\2\u01bc\u01bd\3\2\2\2\u01bd\u01be"+
		"\3\2\2\2\u01bd\u01bb\3\2\2\2\u01be\u01bf\3\2\2\2\u01bf\u01c0\b\60\27\2"+
		"\u01c0e\3\2\2\2\u01c1\u01c2\7b\2\2\u01c2\u01c3\7b\2\2\u01c3\u01c4\7b\2"+
		"\2\u01c4\u01c5\3\2\2\2\u01c5\u01c6\b\61\30\2\u01c6\u01c7\3\2\2\2\u01c7"+
		"\u01c8\b\61\13\2\u01c8g\3\2\2\2\u01c9\u01ca\5,\24\2\u01ca\u01cb\3\2\2"+
		"\2\u01cb\u01cc\b\62\31\2\u01cci\3\2\2\2\u01cd\u01ce\5*\23\2\u01ce\u01cf"+
		"\3\2\2\2\u01cf\u01d0\b\63\32\2\u01d0k\3\2\2\2\u01d1\u01d3\7\17\2\2\u01d2"+
		"\u01d1\3\2\2\2\u01d2\u01d3\3\2\2\2\u01d3\u01d4\3\2\2\2\u01d4\u01d7\7\f"+
		"\2\2\u01d5\u01d7\n\24\2\2\u01d6\u01d2\3\2\2\2\u01d6\u01d5\3\2\2\2\u01d7"+
		"\u01d8\3\2\2\2\u01d8\u01d9\3\2\2\2\u01d8\u01d6\3\2\2\2\u01d9\u01da\3\2"+
		"\2\2\u01da\u01db\b\64\33\2\u01dbm\3\2\2\2\u01dc\u01de\5$\20\2\u01dd\u01dc"+
		"\3\2\2\2\u01de\u01df\3\2\2\2\u01df\u01dd\3\2\2\2\u01df\u01e0\3\2\2\2\u01e0"+
		"\u01e1\3\2\2\2\u01e1\u01e2\b\65\2\2\u01e2o\3\2\2\2\u01e3\u01e5\7\17\2"+
		"\2\u01e4\u01e3\3\2\2\2\u01e4\u01e5\3\2\2\2\u01e5\u01e6\3\2\2\2\u01e6\u01e7"+
		"\7\f\2\2\u01e7\u01e8\b\66\34\2\u01e8\u01e9\b\66\35\2\u01e9\u01ea\3\2\2"+
		"\2\u01ea\u01eb\b\66\2\2\u01eb\u01ec\b\66\36\2\u01ecq\3\2\2\2\u01ed\u01f1"+
		"\5 \16\2\u01ee\u01f1\5\"\17\2\u01ef\u01f1\7a\2\2\u01f0\u01ed\3\2\2\2\u01f0"+
		"\u01ee\3\2\2\2\u01f0\u01ef\3\2\2\2\u01f1\u01f7\3\2\2\2\u01f2\u01f6\5 "+
		"\16\2\u01f3\u01f6\5\"\17\2\u01f4\u01f6\t\31\2\2\u01f5\u01f2\3\2\2\2\u01f5"+
		"\u01f3\3\2\2\2\u01f5\u01f4\3\2\2\2\u01f6\u01f9\3\2\2\2\u01f7\u01f5\3\2"+
		"\2\2\u01f7\u01f8\3\2\2\2\u01f8s\3\2\2\2\u01f9\u01f7\3\2\2\2\u01fa\u01fc"+
		"\n\24\2\2\u01fb\u01fa\3\2\2\2\u01fc\u01fd\3\2\2\2\u01fd\u01fe\3\2\2\2"+
		"\u01fd\u01fb\3\2\2\2\u01feu\3\2\2\2\u01ff\u0203\t\25\2\2\u0200\u0202\n"+
		"\24\2\2\u0201\u0200\3\2\2\2\u0202\u0205\3\2\2\2\u0203\u0201\3\2\2\2\u0203"+
		"\u0204\3\2\2\2\u0204\u0207\3\2\2\2\u0205\u0203\3\2\2\2\u0206\u0208\7\17"+
		"\2\2\u0207\u0206\3\2\2\2\u0207\u0208\3\2\2\2\u0208\u0209\3\2\2\2\u0209"+
		"\u020a\7\f\2\2\u020a\u020b\69\f\2\u020b\u020c\3\2\2\2\u020c\u020d\b9\2"+
		"\2\u020dw\3\2\2\2\u020e\u0210\5$\20\2\u020f\u020e\3\2\2\2\u0210\u0211"+
		"\3\2\2\2\u0211\u020f\3\2\2\2\u0211\u0212\3\2\2\2\u0212\u0213\3\2\2\2\u0213"+
		"\u0214\6:\r\2\u0214\u0215\3\2\2\2\u0215\u0216\b:\2\2\u0216y\3\2\2\2\u0217"+
		"\u0219\7\17\2\2\u0218\u0217\3\2\2\2\u0218\u0219\3\2\2\2\u0219\u021a\3"+
		"\2\2\2\u021a\u021b\7\f\2\2\u021b\u021c\b;\37\2\u021c{\3\2\2\2\u021d\u021e"+
		"\7_\2\2\u021e\u021f\6<\16\2\u021f\u0220\b< \2\u0220\u0221\3\2\2\2\u0221"+
		"\u0222\b<\13\2\u0222\u0223\b<\13\2\u0223}\3\2\2\2\u0224\u0228\5 \16\2"+
		"\u0225\u0228\5\"\17\2\u0226\u0228\7a\2\2\u0227\u0224\3\2\2\2\u0227\u0225"+
		"\3\2\2\2\u0227\u0226\3\2\2\2\u0228\u022e\3\2\2\2\u0229\u022d\5 \16\2\u022a"+
		"\u022d\5\"\17\2\u022b\u022d\t\31\2\2\u022c\u0229\3\2\2\2\u022c\u022a\3"+
		"\2\2\2\u022c\u022b\3\2\2\2\u022d\u0230\3\2\2\2\u022e\u022c\3\2\2\2\u022e"+
		"\u022f\3\2\2\2\u022f\u0231\3\2\2\2\u0230\u022e\3\2\2\2\u0231\u0232\6="+
		"\17\2\u0232\u0233\b=!\2\u0233\177\3\2\2\2\u0234\u0235\7?\2\2\u0235\u0236"+
		"\b>\"\2\u0236\u0081\3\2\2\2\u0237\u0238\7~\2\2\u0238\u0239\b?#\2\u0239"+
		"\u0083\3\2\2\2\u023a\u023b\5,\24\2\u023b\u023c\b@$\2\u023c\u0085\3\2\2"+
		"\2\u023d\u023e\5*\23\2\u023e\u023f\bA%\2\u023f\u0087\3\2\2\2\u0240\u0242"+
		"\n\24\2\2\u0241\u0240\3\2\2\2\u0242\u0243\3\2\2\2\u0243\u0244\3\2\2\2"+
		"\u0243\u0241\3\2\2\2\u0244\u0245\3\2\2\2\u0245\u0246\bB&\2\u0246\u0089"+
		"\3\2\2\2\66\2\3\4\5\6\7\u00ac\u00b4\u00b8\u00be\u00c0\u00ca\u00cc\u00d3"+
		"\u00d9\u00e0\u00e5\u00ff\u0107\u0113\u0118\u0123\u0128\u012a\u0138\u013d"+
		"\u014d\u015a\u0168\u0170\u017e\u018e\u019c\u01ad\u01bd\u01d2\u01d6\u01d8"+
		"\u01df\u01e4\u01f0\u01f5\u01f7\u01fd\u0203\u0207\u0211\u0218\u0227\u022c"+
		"\u022e\u0243\'\b\2\2\3\30\2\7\3\2\3\31\3\7\4\2\7\6\2\3\33\4\3\34\5\3\36"+
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