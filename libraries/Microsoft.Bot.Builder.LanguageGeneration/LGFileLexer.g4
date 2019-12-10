lexer grammar LGFileLexer;

// From a multiple-lanague perpective, it's not recommended to use members, predicates and actions to 
// put target-language-specific code in lexer rules 

// the reason we use it here is that
// 1. it greatly simplify the lexer rules, can avoid unnecessary lexer modes 
// 2. it helps us to output token more precisely
//    (for example, 'CASE:' not followed right after '-' will not be treated as a CASE token)
// 3. we only use very basic boolen variables, and basic predidates
//    so it would be very little effort to translate to other languages

@parser::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.
@lexer::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.

@lexer::members {
  bool ignoreWS = true; // usually we ignore whitespace, but inside template, whitespace is significant
  bool inTemplate = false; // whether we are in the template
  bool beginOfTemplateBody = false; // whether we are at the begining of template body
  bool inMultiline = false; // whether we are in multiline
  bool beginOfTemplateLine = false;// weather we are at the begining of template string
}

// fragments
fragment A: 'a' | 'A';
fragment C: 'c' | 'C';
fragment D: 'd' | 'D';
fragment E: 'e' | 'E';
fragment F: 'f' | 'F';
fragment H: 'h' | 'H';
fragment I: 'i' | 'I';
fragment L: 'l' | 'L';
fragment S: 's' | 'S';
fragment T: 't' | 'T';
fragment U: 'u' | 'U';
fragment W: 'w' | 'W';

fragment LETTER: 'a'..'z' | 'A'..'Z';

fragment NUMBER: '0'..'9';

fragment WHITESPACE : ' '|'\t'|'\ufeff'|'\u00a0';

fragment STRING_LITERAL : ('\'' (~['\r\n])* '\'') | ('"' (~["\r\n])* '"');

fragment EXPRESSION_FRAGMENT : '@' '{' (STRING_LITERAL| ~[\r\n{}'"] )*? '}';

fragment ESCAPE_CHARACTER_FRAGMENT : '\\' ~[\r\n]?;


// top level elements
COMMENTS
  : ('>'|'$') ~('\r'|'\n')+ -> skip
  ;

WS
  : WHITESPACE+ -> skip
  ;

NEWLINE
  : '\r'? '\n' -> skip
  ;

HASH
  : '#' { inTemplate = true; beginOfTemplateBody = false; } -> pushMode(TEMPLATE_NAME_MODE)
  ;

DASH
  : '-' { inTemplate }? { beginOfTemplateLine = true; beginOfTemplateBody = false; } -> pushMode(TEMPLATE_BODY_MODE)
  ;

LEFT_SQUARE_BRACKET
  : '[' { inTemplate && beginOfTemplateBody }? -> pushMode(STRUCTURED_TEMPLATE_BODY_MODE)
  ;

IMPORT
  : '[' ~[\r\n[\]]*? ']' '(' ~[\r\n()]*? ')' { inTemplate = false;}
  ;

INVALID_TOKEN
  : . { inTemplate = false; beginOfTemplateBody = false; }
  ;

mode TEMPLATE_NAME_MODE;

WS_IN_NAME
  : WHITESPACE+ -> skip
  ;

NEWLINE_IN_NAME
  : '\r'? '\n' { beginOfTemplateBody = true;}-> skip, popMode
  ;

IDENTIFIER
  : (LETTER | NUMBER | '_') (LETTER | NUMBER | '-' | '_')*
  ;

DOT
  : '.'
  ;

OPEN_PARENTHESIS
  : '('
  ;

CLOSE_PARENTHESIS
  : ')'
  ;

COMMA
  : ','
  ;

TEXT_IN_NAME
  : ~[\r\n]+?
  ;

mode TEMPLATE_BODY_MODE;

WS_IN_BODY
  : WHITESPACE+  {ignoreWS}? -> skip
  ;

MULTILINE_PREFIX
  : '```' { !inMultiline  && beginOfTemplateLine }? { inMultiline = true; beginOfTemplateLine = false;}-> pushMode(MULTILINE_MODE)
  ;

NEWLINE_IN_BODY
  : '\r'? '\n' { ignoreWS = true;} -> skip, popMode
  ;

IF
  : I F WHITESPACE* ':'  {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

ELSEIF
  : E L S E WHITESPACE* I F WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

ELSE
  : E L S E WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

SWITCH
  : S W I T C H WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

CASE
  : C A S E WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

DEFAULT
  : D E F A U L T WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

ESCAPE_CHARACTER
  : ESCAPE_CHARACTER_FRAGMENT  { ignoreWS = false; beginOfTemplateLine = false;}
  ;

EXPRESSION
  : EXPRESSION_FRAGMENT  { ignoreWS = false; beginOfTemplateLine = false;}
  ;

TEXT
  : ~[\r\n]+?  { ignoreWS = false; beginOfTemplateLine = false;}
  ;

mode MULTILINE_MODE;

MULTILINE_SUFFIX
  : '```' { inMultiline = false; } -> popMode
  ;

MULTILINE_ESCAPE_CHARACTER
  : ESCAPE_CHARACTER_FRAGMENT -> type(ESCAPE_CHARACTER)
  ;

MULTILINE_EXPRESSION
  : EXPRESSION_FRAGMENT -> type(EXPRESSION)
  ;

MULTILINE_TEXT
  : (('\r'? '\n') | ~[\r\n])+? -> type(TEXT)
  ;

mode STRUCTURED_TEMPLATE_BODY_MODE;

WS_IN_STRUCTURED
  : WHITESPACE+
  ;

STRUCTURED_COMMENTS
  : ('>'|'$') ~[\r\n]* '\r'?'\n' -> skip
  ;

STRUCTURED_NEWLINE
  : '\r'? '\n' -> skip
  ;

STRUCTURED_TEMPLATE_BODY_END
  : WS_IN_STRUCTURED? ']' WS_IN_STRUCTURED? { inTemplate = false; beginOfTemplateBody = false;} -> popMode
  ;

STRUCTURED_CONTENT
  : ~[\r\n]+
  ;