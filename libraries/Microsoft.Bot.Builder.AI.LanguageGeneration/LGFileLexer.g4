lexer grammar LGFileLexer;

// From a multiple-lanague perpective, it's not recommended to use members, predicates and actions to 
// put target-language-specific code in lexer rules 

// the reason we use it here is that
// 1. it greatly simplify the lexer rules, can avoid unnecessary lexer modes 
// 2. it helps us to output token more precisely
//    (for example, 'CASE:' not followed right after '-' will not be treated as a CASE token)
// 3. we only use very basic boolen variables, and basic predidates
//    so it would be very little effort to translate to other languages

@lexer::members {
  bool ignoreWS = true;             // usually we ignore whitespace, but inside template, whitespace is significant
  bool expectCaseOrDefault = false; // whethe we are expecting CASE: or DEFAULT:
}

fragment LETTER: 'a'..'z' | 'A'..'Z';
fragment NUMBER: '0'..'9';

COMMENTS
  : ('>'|'$') ~('\r'|'\n')+ -> skip
  ;

WS
  : (' '|'\t')+ -> skip
  ;

NEWLINE
  : '\r'? '\n' -> skip
  ;

HASH
  : '#' -> pushMode(TEMPLATE_NAME_MODE)
  ;

DASH
  : '-' {expectCaseOrDefault = true;} -> pushMode(TEMPLATE_BODY_MODE)
  ;

mode TEMPLATE_NAME_MODE;

WS_IN_NAME
  : (' '|'\t')+ -> skip
  ;

NEWLINE_IN_NAME
  : '\r'? '\n' -> type(NEWLINE), popMode
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

mode TEMPLATE_BODY_MODE;

// a little tedious on the rules, a big improvement on portability
WS_IN_BODY_IGNORED
  : (' '|'\t')+  {ignoreWS}? -> skip
  ;

WS_IN_BODY
  : (' '|'\t')+  -> type(WS)
  ;

NEWLINE_IN_BODY
  : '\r'? '\n' {ignoreWS = true;} -> type(NEWLINE), popMode
  ;

// only CASE and DEFAULT makes ignoreWS = true
CASE
  : ('case:' | 'CASE:') {expectCaseOrDefault}? { ignoreWS = true;}
  ;

DEFAULT
  : ('default:' | 'DEFAULT:') {expectCaseOrDefault}? { ignoreWS = true;}
  ;

MULTI_LINE_TEXT
  : '```' .*? '```' { ignoreWS = false; expectCaseOrDefault = false;}
  ;

EXPRESSION
  : '{' ~[\r\n{}]* '}'  { ignoreWS = false; expectCaseOrDefault = false;}
  ;

TEMPLATE_REF
  : '[' (~[\r\n\]] | TEMPLATE_REF)* ']'  { ignoreWS = false; expectCaseOrDefault = false;}
  ;

TEXT_SEPARATOR
  : [ \t\r\n{}[\]()]  { ignoreWS = false; expectCaseOrDefault = false;}
  ;

TEXT
  : ~[ \t\r\n{}[\]()]+  { ignoreWS = false; expectCaseOrDefault = false;}
  ;