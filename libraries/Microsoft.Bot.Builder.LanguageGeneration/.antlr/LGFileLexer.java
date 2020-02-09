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
		"NUMBER", "WHITESPACE", "EMPTY_OBJECT", "STRING_LITERAL", "STRING_INTERPOLATION", 
		"EXPRESSION_FRAGMENT", "ESCAPE_CHARACTER_FRAGMENT", "COMMENTS", "WS", 
		"NEWLINE", "HASH", "DASH", "LEFT_SQUARE_BRACKET", "IMPORT", "INVALID_TOKEN", 
		"WS_IN_NAME", "NEWLINE_IN_NAME", "IDENTIFIER", "DOT", "OPEN_PARENTHESIS", 
		"CLOSE_PARENTHESIS", "COMMA", "TEXT_IN_NAME", "WS_IN_BODY", "MULTILINE_PREFIX", 
		"NEWLINE_IN_BODY", "IF", "ELSEIF", "ELSE", "SWITCH", "CASE", "DEFAULT", 
		"ESCAPE_CHARACTER", "EXPRESSION", "TEXT", "MULTILINE_SUFFIX", "MULTILINE_ESCAPE_CHARACTER", 
		"MULTILINE_EXPRESSION", "MULTILINE_TEXT", "WS_IN_STRUCTURE_NAME", "NEWLINE_IN_STRUCTURE_NAME", 
		"STRUCTURE_NAME", "TEXT_IN_STRUCTURE_NAME", "STRUCTURED_COMMENTS", "WS_IN_STRUCTURE_BODY", 
		"STRUCTURED_NEWLINE", "STRUCTURED_BODY_END", "STRUCTURE_IDENTIFIER", "STRUCTURE_EQUALS", 
		"STRUCTURE_OR_MARK", "ESCAPE_CHARACTER_IN_STRUCTURE_BODY", "EXPRESSION_IN_STRUCTURE_BODY", 
		"TEXT_IN_STRUCTURE_BODY"
	};

	private static final String[] _LITERAL_NAMES = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		"'.'", "'('", "')'", "','", null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, "'|'"
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
		case 23:
			HASH_action((RuleContext)_localctx, actionIndex);
			break;
		case 24:
			DASH_action((RuleContext)_localctx, actionIndex);
			break;
		case 26:
			IMPORT_action((RuleContext)_localctx, actionIndex);
			break;
		case 27:
			INVALID_TOKEN_action((RuleContext)_localctx, actionIndex);
			break;
		case 29:
			NEWLINE_IN_NAME_action((RuleContext)_localctx, actionIndex);
			break;
		case 37:
			MULTILINE_PREFIX_action((RuleContext)_localctx, actionIndex);
			break;
		case 38:
			NEWLINE_IN_BODY_action((RuleContext)_localctx, actionIndex);
			break;
		case 39:
			IF_action((RuleContext)_localctx, actionIndex);
			break;
		case 40:
			ELSEIF_action((RuleContext)_localctx, actionIndex);
			break;
		case 41:
			ELSE_action((RuleContext)_localctx, actionIndex);
			break;
		case 42:
			SWITCH_action((RuleContext)_localctx, actionIndex);
			break;
		case 43:
			CASE_action((RuleContext)_localctx, actionIndex);
			break;
		case 44:
			DEFAULT_action((RuleContext)_localctx, actionIndex);
			break;
		case 45:
			ESCAPE_CHARACTER_action((RuleContext)_localctx, actionIndex);
			break;
		case 46:
			EXPRESSION_action((RuleContext)_localctx, actionIndex);
			break;
		case 47:
			TEXT_action((RuleContext)_localctx, actionIndex);
			break;
		case 48:
			MULTILINE_SUFFIX_action((RuleContext)_localctx, actionIndex);
			break;
		case 53:
			NEWLINE_IN_STRUCTURE_NAME_action((RuleContext)_localctx, actionIndex);
			break;
		case 58:
			STRUCTURED_NEWLINE_action((RuleContext)_localctx, actionIndex);
			break;
		case 59:
			STRUCTURED_BODY_END_action((RuleContext)_localctx, actionIndex);
			break;
		case 60:
			STRUCTURE_IDENTIFIER_action((RuleContext)_localctx, actionIndex);
			break;
		case 61:
			STRUCTURE_EQUALS_action((RuleContext)_localctx, actionIndex);
			break;
		case 62:
			STRUCTURE_OR_MARK_action((RuleContext)_localctx, actionIndex);
			break;
		case 63:
			ESCAPE_CHARACTER_IN_STRUCTURE_BODY_action((RuleContext)_localctx, actionIndex);
			break;
		case 64:
			EXPRESSION_IN_STRUCTURE_BODY_action((RuleContext)_localctx, actionIndex);
			break;
		case 65:
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
		case 24:
			return DASH_sempred((RuleContext)_localctx, predIndex);
		case 25:
			return LEFT_SQUARE_BRACKET_sempred((RuleContext)_localctx, predIndex);
		case 36:
			return WS_IN_BODY_sempred((RuleContext)_localctx, predIndex);
		case 37:
			return MULTILINE_PREFIX_sempred((RuleContext)_localctx, predIndex);
		case 39:
			return IF_sempred((RuleContext)_localctx, predIndex);
		case 40:
			return ELSEIF_sempred((RuleContext)_localctx, predIndex);
		case 41:
			return ELSE_sempred((RuleContext)_localctx, predIndex);
		case 42:
			return SWITCH_sempred((RuleContext)_localctx, predIndex);
		case 43:
			return CASE_sempred((RuleContext)_localctx, predIndex);
		case 44:
			return DEFAULT_sempred((RuleContext)_localctx, predIndex);
		case 56:
			return STRUCTURED_COMMENTS_sempred((RuleContext)_localctx, predIndex);
		case 57:
			return WS_IN_STRUCTURE_BODY_sempred((RuleContext)_localctx, predIndex);
		case 59:
			return STRUCTURED_BODY_END_sempred((RuleContext)_localctx, predIndex);
		case 60:
			return STRUCTURE_IDENTIFIER_sempred((RuleContext)_localctx, predIndex);
		case 61:
			return STRUCTURE_EQUALS_sempred((RuleContext)_localctx, predIndex);
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
	private boolean STRUCTURE_EQUALS_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 14:
			return !inStructuredValue;
		}
		return true;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2-\u0254\b\1\b\1\b"+
		"\1\b\1\b\1\b\1\4\2\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b"+
		"\4\t\t\t\4\n\t\n\4\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t"+
		"\20\4\21\t\21\4\22\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t"+
		"\27\4\30\t\30\4\31\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t"+
		"\36\4\37\t\37\4 \t \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t"+
		"(\4)\t)\4*\t*\4+\t+\4,\t,\4-\t-\4.\t.\4/\t/\4\60\t\60\4\61\t\61\4\62\t"+
		"\62\4\63\t\63\4\64\t\64\4\65\t\65\4\66\t\66\4\67\t\67\48\t8\49\t9\4:\t"+
		":\4;\t;\4<\t<\4=\t=\4>\t>\4?\t?\4@\t@\4A\tA\4B\tB\4C\tC\3\2\3\2\3\3\3"+
		"\3\3\4\3\4\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3\13\3\13\3"+
		"\f\3\f\3\r\3\r\3\16\3\16\3\17\3\17\3\20\3\20\3\21\3\21\7\21\u00ad\n\21"+
		"\f\21\16\21\u00b0\13\21\3\21\3\21\3\22\3\22\7\22\u00b6\n\22\f\22\16\22"+
		"\u00b9\13\22\3\22\3\22\3\22\7\22\u00be\n\22\f\22\16\22\u00c1\13\22\3\22"+
		"\5\22\u00c4\n\22\3\23\3\23\3\23\3\23\7\23\u00ca\n\23\f\23\16\23\u00cd"+
		"\13\23\3\23\3\23\3\24\3\24\3\24\3\24\3\24\3\24\6\24\u00d7\n\24\r\24\16"+
		"\24\u00d8\3\24\5\24\u00dc\n\24\3\25\3\25\5\25\u00e0\n\25\3\26\3\26\6\26"+
		"\u00e4\n\26\r\26\16\26\u00e5\3\26\3\26\3\27\6\27\u00eb\n\27\r\27\16\27"+
		"\u00ec\3\27\3\27\3\30\5\30\u00f2\n\30\3\30\3\30\3\30\3\30\3\31\3\31\3"+
		"\31\3\31\3\31\3\32\3\32\3\32\3\32\3\32\3\32\3\33\3\33\3\33\3\33\3\33\3"+
		"\34\3\34\7\34\u010a\n\34\f\34\16\34\u010d\13\34\3\34\3\34\3\34\7\34\u0112"+
		"\n\34\f\34\16\34\u0115\13\34\3\34\3\34\3\34\3\35\3\35\3\35\3\36\6\36\u011e"+
		"\n\36\r\36\16\36\u011f\3\36\3\36\3\37\5\37\u0125\n\37\3\37\3\37\3\37\3"+
		"\37\3\37\3\37\3 \3 \3 \5 \u0130\n \3 \3 \3 \7 \u0135\n \f \16 \u0138\13"+
		" \3!\3!\3\"\3\"\3#\3#\3$\3$\3%\6%\u0143\n%\r%\16%\u0144\3&\6&\u0148\n"+
		"&\r&\16&\u0149\3&\3&\3&\3&\3\'\3\'\3\'\3\'\3\'\3\'\3\'\3\'\3\'\3(\5(\u015a"+
		"\n(\3(\3(\3(\3(\3(\3(\3)\3)\3)\7)\u0165\n)\f)\16)\u0168\13)\3)\3)\3)\3"+
		")\3*\3*\3*\3*\3*\7*\u0173\n*\f*\16*\u0176\13*\3*\3*\3*\7*\u017b\n*\f*"+
		"\16*\u017e\13*\3*\3*\3*\3*\3+\3+\3+\3+\3+\7+\u0189\n+\f+\16+\u018c\13"+
		"+\3+\3+\3+\3+\3,\3,\3,\3,\3,\3,\3,\7,\u0199\n,\f,\16,\u019c\13,\3,\3,"+
		"\3,\3,\3-\3-\3-\3-\3-\7-\u01a7\n-\f-\16-\u01aa\13-\3-\3-\3-\3-\3.\3.\3"+
		".\3.\3.\3.\3.\3.\7.\u01b8\n.\f.\16.\u01bb\13.\3.\3.\3.\3.\3/\3/\3/\3\60"+
		"\3\60\3\60\3\61\6\61\u01c8\n\61\r\61\16\61\u01c9\3\61\3\61\3\62\3\62\3"+
		"\62\3\62\3\62\3\62\3\62\3\62\3\63\3\63\3\63\3\63\3\64\3\64\3\64\3\64\3"+
		"\65\5\65\u01df\n\65\3\65\3\65\6\65\u01e3\n\65\r\65\16\65\u01e4\3\65\3"+
		"\65\3\66\6\66\u01ea\n\66\r\66\16\66\u01eb\3\66\3\66\3\67\5\67\u01f1\n"+
		"\67\3\67\3\67\3\67\3\67\3\67\3\67\3\67\38\38\38\58\u01fd\n8\38\38\38\7"+
		"8\u0202\n8\f8\168\u0205\138\39\69\u0208\n9\r9\169\u0209\3:\3:\7:\u020e"+
		"\n:\f:\16:\u0211\13:\3:\5:\u0214\n:\3:\3:\3:\3:\3:\3;\6;\u021c\n;\r;\16"+
		";\u021d\3;\3;\3;\3;\3<\5<\u0225\n<\3<\3<\3<\3=\3=\3=\3=\3=\3=\3=\3>\3"+
		">\3>\5>\u0234\n>\3>\3>\3>\7>\u0239\n>\f>\16>\u023c\13>\3>\3>\3>\3?\3?"+
		"\3?\3?\3@\3@\3@\3A\3A\3A\3B\3B\3B\3C\6C\u024f\nC\rC\16C\u0250\3C\3C\t"+
		"\u010b\u0113\u0144\u01c9\u01e4\u0209\u0250\2D\b\2\n\2\f\2\16\2\20\2\22"+
		"\2\24\2\26\2\30\2\32\2\34\2\36\2 \2\"\2$\2&\2(\2*\2,\2.\2\60\3\62\4\64"+
		"\5\66\68\7:\b<\t>\n@\13B\fD\rF\16H\17J\20L\21N\22P\23R\24T\25V\26X\27"+
		"Z\30\\\31^\32`\33b\34d\35f\36h\37j\2l\2n\2p r!t\"v#x$z%|&~\'\u0080(\u0082"+
		")\u0084*\u0086+\u0088,\u008a-\b\2\3\4\5\6\7\32\4\2CCcc\4\2EEee\4\2FFf"+
		"f\4\2GGgg\4\2HHhh\4\2JJjj\4\2KKkk\4\2NNnn\4\2UUuu\4\2VVvv\4\2WWww\4\2"+
		"YYyy\4\2C\\c|\6\2\13\13\"\"\u00a2\u00a2\uff01\uff01\5\2\f\f\17\17))\5"+
		"\2\f\f\17\17$$\3\2bb\t\2\f\f\17\17$$))bb}}\177\177\4\2\f\f\17\17\4\2&"+
		"&@@\6\2\f\f\17\17]]__\5\2\f\f\17\17*+\4\2//aa\4\2/\60aa\2\u0272\2\60\3"+
		"\2\2\2\2\62\3\2\2\2\2\64\3\2\2\2\2\66\3\2\2\2\28\3\2\2\2\2:\3\2\2\2\2"+
		"<\3\2\2\2\2>\3\2\2\2\3@\3\2\2\2\3B\3\2\2\2\3D\3\2\2\2\3F\3\2\2\2\3H\3"+
		"\2\2\2\3J\3\2\2\2\3L\3\2\2\2\3N\3\2\2\2\4P\3\2\2\2\4R\3\2\2\2\4T\3\2\2"+
		"\2\4V\3\2\2\2\4X\3\2\2\2\4Z\3\2\2\2\4\\\3\2\2\2\4^\3\2\2\2\4`\3\2\2\2"+
		"\4b\3\2\2\2\4d\3\2\2\2\4f\3\2\2\2\5h\3\2\2\2\5j\3\2\2\2\5l\3\2\2\2\5n"+
		"\3\2\2\2\6p\3\2\2\2\6r\3\2\2\2\6t\3\2\2\2\6v\3\2\2\2\7x\3\2\2\2\7z\3\2"+
		"\2\2\7|\3\2\2\2\7~\3\2\2\2\7\u0080\3\2\2\2\7\u0082\3\2\2\2\7\u0084\3\2"+
		"\2\2\7\u0086\3\2\2\2\7\u0088\3\2\2\2\7\u008a\3\2\2\2\b\u008c\3\2\2\2\n"+
		"\u008e\3\2\2\2\f\u0090\3\2\2\2\16\u0092\3\2\2\2\20\u0094\3\2\2\2\22\u0096"+
		"\3\2\2\2\24\u0098\3\2\2\2\26\u009a\3\2\2\2\30\u009c\3\2\2\2\32\u009e\3"+
		"\2\2\2\34\u00a0\3\2\2\2\36\u00a2\3\2\2\2 \u00a4\3\2\2\2\"\u00a6\3\2\2"+
		"\2$\u00a8\3\2\2\2&\u00aa\3\2\2\2(\u00c3\3\2\2\2*\u00c5\3\2\2\2,\u00d0"+
		"\3\2\2\2.\u00dd\3\2\2\2\60\u00e1\3\2\2\2\62\u00ea\3\2\2\2\64\u00f1\3\2"+
		"\2\2\66\u00f7\3\2\2\28\u00fc\3\2\2\2:\u0102\3\2\2\2<\u0107\3\2\2\2>\u0119"+
		"\3\2\2\2@\u011d\3\2\2\2B\u0124\3\2\2\2D\u012f\3\2\2\2F\u0139\3\2\2\2H"+
		"\u013b\3\2\2\2J\u013d\3\2\2\2L\u013f\3\2\2\2N\u0142\3\2\2\2P\u0147\3\2"+
		"\2\2R\u014f\3\2\2\2T\u0159\3\2\2\2V\u0161\3\2\2\2X\u016d\3\2\2\2Z\u0183"+
		"\3\2\2\2\\\u0191\3\2\2\2^\u01a1\3\2\2\2`\u01af\3\2\2\2b\u01c0\3\2\2\2"+
		"d\u01c3\3\2\2\2f\u01c7\3\2\2\2h\u01cd\3\2\2\2j\u01d5\3\2\2\2l\u01d9\3"+
		"\2\2\2n\u01e2\3\2\2\2p\u01e9\3\2\2\2r\u01f0\3\2\2\2t\u01fc\3\2\2\2v\u0207"+
		"\3\2\2\2x\u020b\3\2\2\2z\u021b\3\2\2\2|\u0224\3\2\2\2~\u0229\3\2\2\2\u0080"+
		"\u0233\3\2\2\2\u0082\u0240\3\2\2\2\u0084\u0244\3\2\2\2\u0086\u0247\3\2"+
		"\2\2\u0088\u024a\3\2\2\2\u008a\u024e\3\2\2\2\u008c\u008d\t\2\2\2\u008d"+
		"\t\3\2\2\2\u008e\u008f\t\3\2\2\u008f\13\3\2\2\2\u0090\u0091\t\4\2\2\u0091"+
		"\r\3\2\2\2\u0092\u0093\t\5\2\2\u0093\17\3\2\2\2\u0094\u0095\t\6\2\2\u0095"+
		"\21\3\2\2\2\u0096\u0097\t\7\2\2\u0097\23\3\2\2\2\u0098\u0099\t\b\2\2\u0099"+
		"\25\3\2\2\2\u009a\u009b\t\t\2\2\u009b\27\3\2\2\2\u009c\u009d\t\n\2\2\u009d"+
		"\31\3\2\2\2\u009e\u009f\t\13\2\2\u009f\33\3\2\2\2\u00a0\u00a1\t\f\2\2"+
		"\u00a1\35\3\2\2\2\u00a2\u00a3\t\r\2\2\u00a3\37\3\2\2\2\u00a4\u00a5\t\16"+
		"\2\2\u00a5!\3\2\2\2\u00a6\u00a7\4\62;\2\u00a7#\3\2\2\2\u00a8\u00a9\t\17"+
		"\2\2\u00a9%\3\2\2\2\u00aa\u00ae\7}\2\2\u00ab\u00ad\5$\20\2\u00ac\u00ab"+
		"\3\2\2\2\u00ad\u00b0\3\2\2\2\u00ae\u00ac\3\2\2\2\u00ae\u00af\3\2\2\2\u00af"+
		"\u00b1\3\2\2\2\u00b0\u00ae\3\2\2\2\u00b1\u00b2\7\177\2\2\u00b2\'\3\2\2"+
		"\2\u00b3\u00b7\7)\2\2\u00b4\u00b6\n\20\2\2\u00b5\u00b4\3\2\2\2\u00b6\u00b9"+
		"\3\2\2\2\u00b7\u00b5\3\2\2\2\u00b7\u00b8\3\2\2\2\u00b8\u00ba\3\2\2\2\u00b9"+
		"\u00b7\3\2\2\2\u00ba\u00c4\7)\2\2\u00bb\u00bf\7$\2\2\u00bc\u00be\n\21"+
		"\2\2\u00bd\u00bc\3\2\2\2\u00be\u00c1\3\2\2\2\u00bf\u00bd\3\2\2\2\u00bf"+
		"\u00c0\3\2\2\2\u00c0\u00c2\3\2\2\2\u00c1\u00bf\3\2\2\2\u00c2\u00c4\7$"+
		"\2\2\u00c3\u00b3\3\2\2\2\u00c3\u00bb\3\2\2\2\u00c4)\3\2\2\2\u00c5\u00cb"+
		"\7b\2\2\u00c6\u00c7\7^\2\2\u00c7\u00ca\7b\2\2\u00c8\u00ca\n\22\2\2\u00c9"+
		"\u00c6\3\2\2\2\u00c9\u00c8\3\2\2\2\u00ca\u00cd\3\2\2\2\u00cb\u00c9\3\2"+
		"\2\2\u00cb\u00cc\3\2\2\2\u00cc\u00ce\3\2\2\2\u00cd\u00cb\3\2\2\2\u00ce"+
		"\u00cf\7b\2\2\u00cf+\3\2\2\2\u00d0\u00d1\7B\2\2\u00d1\u00d6\7}\2\2\u00d2"+
		"\u00d7\5(\22\2\u00d3\u00d7\5*\23\2\u00d4\u00d7\5&\21\2\u00d5\u00d7\n\23"+
		"\2\2\u00d6\u00d2\3\2\2\2\u00d6\u00d3\3\2\2\2\u00d6\u00d4\3\2\2\2\u00d6"+
		"\u00d5\3\2\2\2\u00d7\u00d8\3\2\2\2\u00d8\u00d6\3\2\2\2\u00d8\u00d9\3\2"+
		"\2\2\u00d9\u00db\3\2\2\2\u00da\u00dc\7\177\2\2\u00db\u00da\3\2\2\2\u00db"+
		"\u00dc\3\2\2\2\u00dc-\3\2\2\2\u00dd\u00df\7^\2\2\u00de\u00e0\n\24\2\2"+
		"\u00df\u00de\3\2\2\2\u00df\u00e0\3\2\2\2\u00e0/\3\2\2\2\u00e1\u00e3\t"+
		"\25\2\2\u00e2\u00e4\n\24\2\2\u00e3\u00e2\3\2\2\2\u00e4\u00e5\3\2\2\2\u00e5"+
		"\u00e3\3\2\2\2\u00e5\u00e6\3\2\2\2\u00e6\u00e7\3\2\2\2\u00e7\u00e8\b\26"+
		"\2\2\u00e8\61\3\2\2\2\u00e9\u00eb\5$\20\2\u00ea\u00e9\3\2\2\2\u00eb\u00ec"+
		"\3\2\2\2\u00ec\u00ea\3\2\2\2\u00ec\u00ed\3\2\2\2\u00ed\u00ee\3\2\2\2\u00ee"+
		"\u00ef\b\27\2\2\u00ef\63\3\2\2\2\u00f0\u00f2\7\17\2\2\u00f1\u00f0\3\2"+
		"\2\2\u00f1\u00f2\3\2\2\2\u00f2\u00f3\3\2\2\2\u00f3\u00f4\7\f\2\2\u00f4"+
		"\u00f5\3\2\2\2\u00f5\u00f6\b\30\2\2\u00f6\65\3\2\2\2\u00f7\u00f8\7%\2"+
		"\2\u00f8\u00f9\b\31\3\2\u00f9\u00fa\3\2\2\2\u00fa\u00fb\b\31\4\2\u00fb"+
		"\67\3\2\2\2\u00fc\u00fd\7/\2\2\u00fd\u00fe\6\32\2\2\u00fe\u00ff\b\32\5"+
		"\2\u00ff\u0100\3\2\2\2\u0100\u0101\b\32\6\2\u01019\3\2\2\2\u0102\u0103"+
		"\7]\2\2\u0103\u0104\6\33\3\2\u0104\u0105\3\2\2\2\u0105\u0106\b\33\7\2"+
		"\u0106;\3\2\2\2\u0107\u010b\7]\2\2\u0108\u010a\n\26\2\2\u0109\u0108\3"+
		"\2\2\2\u010a\u010d\3\2\2\2\u010b\u010c\3\2\2\2\u010b\u0109\3\2\2\2\u010c"+
		"\u010e\3\2\2\2\u010d\u010b\3\2\2\2\u010e\u010f\7_\2\2\u010f\u0113\7*\2"+
		"\2\u0110\u0112\n\27\2\2\u0111\u0110\3\2\2\2\u0112\u0115\3\2\2\2\u0113"+
		"\u0114\3\2\2\2\u0113\u0111\3\2\2\2\u0114\u0116\3\2\2\2\u0115\u0113\3\2"+
		"\2\2\u0116\u0117\7+\2\2\u0117\u0118\b\34\b\2\u0118=\3\2\2\2\u0119\u011a"+
		"\13\2\2\2\u011a\u011b\b\35\t\2\u011b?\3\2\2\2\u011c\u011e\5$\20\2\u011d"+
		"\u011c\3\2\2\2\u011e\u011f\3\2\2\2\u011f\u011d\3\2\2\2\u011f\u0120\3\2"+
		"\2\2\u0120\u0121\3\2\2\2\u0121\u0122\b\36\2\2\u0122A\3\2\2\2\u0123\u0125"+
		"\7\17\2\2\u0124\u0123\3\2\2\2\u0124\u0125\3\2\2\2\u0125\u0126\3\2\2\2"+
		"\u0126\u0127\7\f\2\2\u0127\u0128\b\37\n\2\u0128\u0129\3\2\2\2\u0129\u012a"+
		"\b\37\2\2\u012a\u012b\b\37\13\2\u012bC\3\2\2\2\u012c\u0130\5 \16\2\u012d"+
		"\u0130\5\"\17\2\u012e\u0130\7a\2\2\u012f\u012c\3\2\2\2\u012f\u012d\3\2"+
		"\2\2\u012f\u012e\3\2\2\2\u0130\u0136\3\2\2\2\u0131\u0135\5 \16\2\u0132"+
		"\u0135\5\"\17\2\u0133\u0135\t\30\2\2\u0134\u0131\3\2\2\2\u0134\u0132\3"+
		"\2\2\2\u0134\u0133\3\2\2\2\u0135\u0138\3\2\2\2\u0136\u0134\3\2\2\2\u0136"+
		"\u0137\3\2\2\2\u0137E\3\2\2\2\u0138\u0136\3\2\2\2\u0139\u013a\7\60\2\2"+
		"\u013aG\3\2\2\2\u013b\u013c\7*\2\2\u013cI\3\2\2\2\u013d\u013e\7+\2\2\u013e"+
		"K\3\2\2\2\u013f\u0140\7.\2\2\u0140M\3\2\2\2\u0141\u0143\n\24\2\2\u0142"+
		"\u0141\3\2\2\2\u0143\u0144\3\2\2\2\u0144\u0145\3\2\2\2\u0144\u0142\3\2"+
		"\2\2\u0145O\3\2\2\2\u0146\u0148\5$\20\2\u0147\u0146\3\2\2\2\u0148\u0149"+
		"\3\2\2\2\u0149\u0147\3\2\2\2\u0149\u014a\3\2\2\2\u014a\u014b\3\2\2\2\u014b"+
		"\u014c\6&\4\2\u014c\u014d\3\2\2\2\u014d\u014e\b&\2\2\u014eQ\3\2\2\2\u014f"+
		"\u0150\7b\2\2\u0150\u0151\7b\2\2\u0151\u0152\7b\2\2\u0152\u0153\3\2\2"+
		"\2\u0153\u0154\6\'\5\2\u0154\u0155\b\'\f\2\u0155\u0156\3\2\2\2\u0156\u0157"+
		"\b\'\r\2\u0157S\3\2\2\2\u0158\u015a\7\17\2\2\u0159\u0158\3\2\2\2\u0159"+
		"\u015a\3\2\2\2\u015a\u015b\3\2\2\2\u015b\u015c\7\f\2\2\u015c\u015d\b("+
		"\16\2\u015d\u015e\3\2\2\2\u015e\u015f\b(\2\2\u015f\u0160\b(\13\2\u0160"+
		"U\3\2\2\2\u0161\u0162\5\24\b\2\u0162\u0166\5\20\6\2\u0163\u0165\5$\20"+
		"\2\u0164\u0163\3\2\2\2\u0165\u0168\3\2\2\2\u0166\u0164\3\2\2\2\u0166\u0167"+
		"\3\2\2\2\u0167\u0169\3\2\2\2\u0168\u0166\3\2\2\2\u0169\u016a\7<\2\2\u016a"+
		"\u016b\6)\6\2\u016b\u016c\b)\17\2\u016cW\3\2\2\2\u016d\u016e\5\16\5\2"+
		"\u016e\u016f\5\26\t\2\u016f\u0170\5\30\n\2\u0170\u0174\5\16\5\2\u0171"+
		"\u0173\5$\20\2\u0172\u0171\3\2\2\2\u0173\u0176\3\2\2\2\u0174\u0172\3\2"+
		"\2\2\u0174\u0175\3\2\2\2\u0175\u0177\3\2\2\2\u0176\u0174\3\2\2\2\u0177"+
		"\u0178\5\24\b\2\u0178\u017c\5\20\6\2\u0179\u017b\5$\20\2\u017a\u0179\3"+
		"\2\2\2\u017b\u017e\3\2\2\2\u017c\u017a\3\2\2\2\u017c\u017d\3\2\2\2\u017d"+
		"\u017f\3\2\2\2\u017e\u017c\3\2\2\2\u017f\u0180\7<\2\2\u0180\u0181\6*\7"+
		"\2\u0181\u0182\b*\20\2\u0182Y\3\2\2\2\u0183\u0184\5\16\5\2\u0184\u0185"+
		"\5\26\t\2\u0185\u0186\5\30\n\2\u0186\u018a\5\16\5\2\u0187\u0189\5$\20"+
		"\2\u0188\u0187\3\2\2\2\u0189\u018c\3\2\2\2\u018a\u0188\3\2\2\2\u018a\u018b"+
		"\3\2\2\2\u018b\u018d\3\2\2\2\u018c\u018a\3\2\2\2\u018d\u018e\7<\2\2\u018e"+
		"\u018f\6+\b\2\u018f\u0190\b+\21\2\u0190[\3\2\2\2\u0191\u0192\5\30\n\2"+
		"\u0192\u0193\5\36\r\2\u0193\u0194\5\24\b\2\u0194\u0195\5\32\13\2\u0195"+
		"\u0196\5\n\3\2\u0196\u019a\5\22\7\2\u0197\u0199\5$\20\2\u0198\u0197\3"+
		"\2\2\2\u0199\u019c\3\2\2\2\u019a\u0198\3\2\2\2\u019a\u019b\3\2\2\2\u019b"+
		"\u019d\3\2\2\2\u019c\u019a\3\2\2\2\u019d\u019e\7<\2\2\u019e\u019f\6,\t"+
		"\2\u019f\u01a0\b,\22\2\u01a0]\3\2\2\2\u01a1\u01a2\5\n\3\2\u01a2\u01a3"+
		"\5\b\2\2\u01a3\u01a4\5\30\n\2\u01a4\u01a8\5\16\5\2\u01a5\u01a7\5$\20\2"+
		"\u01a6\u01a5\3\2\2\2\u01a7\u01aa\3\2\2\2\u01a8\u01a6\3\2\2\2\u01a8\u01a9"+
		"\3\2\2\2\u01a9\u01ab\3\2\2\2\u01aa\u01a8\3\2\2\2\u01ab\u01ac\7<\2\2\u01ac"+
		"\u01ad\6-\n\2\u01ad\u01ae\b-\23\2\u01ae_\3\2\2\2\u01af\u01b0\5\f\4\2\u01b0"+
		"\u01b1\5\16\5\2\u01b1\u01b2\5\20\6\2\u01b2\u01b3\5\b\2\2\u01b3\u01b4\5"+
		"\34\f\2\u01b4\u01b5\5\26\t\2\u01b5\u01b9\5\32\13\2\u01b6\u01b8\5$\20\2"+
		"\u01b7\u01b6\3\2\2\2\u01b8\u01bb\3\2\2\2\u01b9\u01b7\3\2\2\2\u01b9\u01ba"+
		"\3\2\2\2\u01ba\u01bc\3\2\2\2\u01bb\u01b9\3\2\2\2\u01bc\u01bd\7<\2\2\u01bd"+
		"\u01be\6.\13\2\u01be\u01bf\b.\24\2\u01bfa\3\2\2\2\u01c0\u01c1\5.\25\2"+
		"\u01c1\u01c2\b/\25\2\u01c2c\3\2\2\2\u01c3\u01c4\5,\24\2\u01c4\u01c5\b"+
		"\60\26\2\u01c5e\3\2\2\2\u01c6\u01c8\n\24\2\2\u01c7\u01c6\3\2\2\2\u01c8"+
		"\u01c9\3\2\2\2\u01c9\u01ca\3\2\2\2\u01c9\u01c7\3\2\2\2\u01ca\u01cb\3\2"+
		"\2\2\u01cb\u01cc\b\61\27\2\u01ccg\3\2\2\2\u01cd\u01ce\7b\2\2\u01ce\u01cf"+
		"\7b\2\2\u01cf\u01d0\7b\2\2\u01d0\u01d1\3\2\2\2\u01d1\u01d2\b\62\30\2\u01d2"+
		"\u01d3\3\2\2\2\u01d3\u01d4\b\62\13\2\u01d4i\3\2\2\2\u01d5\u01d6\5.\25"+
		"\2\u01d6\u01d7\3\2\2\2\u01d7\u01d8\b\63\31\2\u01d8k\3\2\2\2\u01d9\u01da"+
		"\5,\24\2\u01da\u01db\3\2\2\2\u01db\u01dc\b\64\32\2\u01dcm\3\2\2\2\u01dd"+
		"\u01df\7\17\2\2\u01de\u01dd\3\2\2\2\u01de\u01df\3\2\2\2\u01df\u01e0\3"+
		"\2\2\2\u01e0\u01e3\7\f\2\2\u01e1\u01e3\n\24\2\2\u01e2\u01de\3\2\2\2\u01e2"+
		"\u01e1\3\2\2\2\u01e3\u01e4\3\2\2\2\u01e4\u01e5\3\2\2\2\u01e4\u01e2\3\2"+
		"\2\2\u01e5\u01e6\3\2\2\2\u01e6\u01e7\b\65\33\2\u01e7o\3\2\2\2\u01e8\u01ea"+
		"\5$\20\2\u01e9\u01e8\3\2\2\2\u01ea\u01eb\3\2\2\2\u01eb\u01e9\3\2\2\2\u01eb"+
		"\u01ec\3\2\2\2\u01ec\u01ed\3\2\2\2\u01ed\u01ee\b\66\2\2\u01eeq\3\2\2\2"+
		"\u01ef\u01f1\7\17\2\2\u01f0\u01ef\3\2\2\2\u01f0\u01f1\3\2\2\2\u01f1\u01f2"+
		"\3\2\2\2\u01f2\u01f3\7\f\2\2\u01f3\u01f4\b\67\34\2\u01f4\u01f5\b\67\35"+
		"\2\u01f5\u01f6\3\2\2\2\u01f6\u01f7\b\67\2\2\u01f7\u01f8\b\67\36\2\u01f8"+
		"s\3\2\2\2\u01f9\u01fd\5 \16\2\u01fa\u01fd\5\"\17\2\u01fb\u01fd\7a\2\2"+
		"\u01fc\u01f9\3\2\2\2\u01fc\u01fa\3\2\2\2\u01fc\u01fb\3\2\2\2\u01fd\u0203"+
		"\3\2\2\2\u01fe\u0202\5 \16\2\u01ff\u0202\5\"\17\2\u0200\u0202\t\31\2\2"+
		"\u0201\u01fe\3\2\2\2\u0201\u01ff\3\2\2\2\u0201\u0200\3\2\2\2\u0202\u0205"+
		"\3\2\2\2\u0203\u0201\3\2\2\2\u0203\u0204\3\2\2\2\u0204u\3\2\2\2\u0205"+
		"\u0203\3\2\2\2\u0206\u0208\n\24\2\2\u0207\u0206\3\2\2\2\u0208\u0209\3"+
		"\2\2\2\u0209\u020a\3\2\2\2\u0209\u0207\3\2\2\2\u020aw\3\2\2\2\u020b\u020f"+
		"\t\25\2\2\u020c\u020e\n\24\2\2\u020d\u020c\3\2\2\2\u020e\u0211\3\2\2\2"+
		"\u020f\u020d\3\2\2\2\u020f\u0210\3\2\2\2\u0210\u0213\3\2\2\2\u0211\u020f"+
		"\3\2\2\2\u0212\u0214\7\17\2\2\u0213\u0212\3\2\2\2\u0213\u0214\3\2\2\2"+
		"\u0214\u0215\3\2\2\2\u0215\u0216\7\f\2\2\u0216\u0217\6:\f\2\u0217\u0218"+
		"\3\2\2\2\u0218\u0219\b:\2\2\u0219y\3\2\2\2\u021a\u021c\5$\20\2\u021b\u021a"+
		"\3\2\2\2\u021c\u021d\3\2\2\2\u021d\u021b\3\2\2\2\u021d\u021e\3\2\2\2\u021e"+
		"\u021f\3\2\2\2\u021f\u0220\6;\r\2\u0220\u0221\3\2\2\2\u0221\u0222\b;\2"+
		"\2\u0222{\3\2\2\2\u0223\u0225\7\17\2\2\u0224\u0223\3\2\2\2\u0224\u0225"+
		"\3\2\2\2\u0225\u0226\3\2\2\2\u0226\u0227\7\f\2\2\u0227\u0228\b<\37\2\u0228"+
		"}\3\2\2\2\u0229\u022a\7_\2\2\u022a\u022b\6=\16\2\u022b\u022c\b= \2\u022c"+
		"\u022d\3\2\2\2\u022d\u022e\b=\13\2\u022e\u022f\b=\13\2\u022f\177\3\2\2"+
		"\2\u0230\u0234\5 \16\2\u0231\u0234\5\"\17\2\u0232\u0234\7a\2\2\u0233\u0230"+
		"\3\2\2\2\u0233\u0231\3\2\2\2\u0233\u0232\3\2\2\2\u0234\u023a\3\2\2\2\u0235"+
		"\u0239\5 \16\2\u0236\u0239\5\"\17\2\u0237\u0239\t\31\2\2\u0238\u0235\3"+
		"\2\2\2\u0238\u0236\3\2\2\2\u0238\u0237\3\2\2\2\u0239\u023c\3\2\2\2\u023a"+
		"\u0238\3\2\2\2\u023a\u023b\3\2\2\2\u023b\u023d\3\2\2\2\u023c\u023a\3\2"+
		"\2\2\u023d\u023e\6>\17\2\u023e\u023f\b>!\2\u023f\u0081\3\2\2\2\u0240\u0241"+
		"\7?\2\2\u0241\u0242\6?\20\2\u0242\u0243\b?\"\2\u0243\u0083\3\2\2\2\u0244"+
		"\u0245\7~\2\2\u0245\u0246\b@#\2\u0246\u0085\3\2\2\2\u0247\u0248\5.\25"+
		"\2\u0248\u0249\bA$\2\u0249\u0087\3\2\2\2\u024a\u024b\5,\24\2\u024b\u024c"+
		"\bB%\2\u024c\u0089\3\2\2\2\u024d\u024f\n\24\2\2\u024e\u024d\3\2\2\2\u024f"+
		"\u0250\3\2\2\2\u0250\u0251\3\2\2\2\u0250\u024e\3\2\2\2\u0251\u0252\3\2"+
		"\2\2\u0252\u0253\bC&\2\u0253\u008b\3\2\2\28\2\3\4\5\6\7\u00ae\u00b7\u00bf"+
		"\u00c3\u00c9\u00cb\u00d6\u00d8\u00db\u00df\u00e5\u00ec\u00f1\u010b\u0113"+
		"\u011f\u0124\u012f\u0134\u0136\u0144\u0149\u0159\u0166\u0174\u017c\u018a"+
		"\u019a\u01a8\u01b9\u01c9\u01de\u01e2\u01e4\u01eb\u01f0\u01fc\u0201\u0203"+
		"\u0209\u020f\u0213\u021d\u0224\u0233\u0238\u023a\u0250\'\b\2\2\3\31\2"+
		"\7\3\2\3\32\3\7\4\2\7\6\2\3\34\4\3\35\5\3\37\6\6\2\2\3\'\7\7\5\2\3(\b"+
		"\3)\t\3*\n\3+\13\3,\f\3-\r\3.\16\3/\17\3\60\20\3\61\21\3\62\22\t\34\2"+
		"\t\35\2\t\36\2\3\67\23\3\67\24\7\7\2\3<\25\3=\26\3>\27\3?\30\3@\31\3A"+
		"\32\3B\33\3C\34";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}