lexer grammar LGFileLexer;

// usually we don't define pattern of a complete TOKEN as fragment, since fragment don't generate token directly
// the reason we do it here is to share pattern across multiple modes
fragment F_EXPRESSION: '{' ~('\r'|'\n'|'{'|'}')* '}';
fragment F_TEMPLATE_REF: '[' (~('\r'|'\n'|']') | F_TEMPLATE_REF)* ']';
fragment F_TEXT: ~('\r'|'\n'|' '|'\t'|'('|')'|','|'{'|'}'|'['|']')+;
fragment F_NEW_LINE: '\r'?'\n';
fragment F_WS: ' '|'\t';

// those are text seperators
fragment F_OPEN_CURLY: '{';
fragment F_CLOSE_CURLY: '}';
fragment F_OPEN_BRACKET: '[';
fragment F_CLOSE_BRACKET: ']';
fragment F_OPEN_PARENTHESIS: '(';
fragment F_CLOSE_PARENTHESIS: ')';
fragment F_COMMA: ',';

fragment LETTER: 'a'..'z' | 'A'..'Z';
fragment NUMBER: '0'..'9';

// in comment mode, eveyting is skip until newline is encoutner
// we just treat $... as comment here before we handle type notation in parser
COMMENT_START
  : ('>' | '$' ) -> skip, mode(COMMENT_MODE)
  ;

HASH
  : '#'
  ;

COMMA
  : ','
  ;

DASH
  : '-' -> mode(START_TEMPLATE_MODE)
  ;

OPEN_PARENTHESIS
  : F_OPEN_PARENTHESIS
  ;

CLOSE_PARENTHESIS
  : F_CLOSE_PARENTHESIS
  ;

OPEN_CURLY
  : F_OPEN_CURLY
  ;

CLOSE_CURLY
  : F_CLOSE_CURLY
  ;

OPEN_BRACKET
  : F_OPEN_BRACKET
  ;

CLOSE_BRACKET
  : F_CLOSE_BRACKET
  ;

IDENTIFIER
  : (LETTER | NUMBER | '_') (LETTER | NUMBER | '-' | '_')*
  ;


/*
 * Treat anything inside bracket as expression
 * Won't analyze the expression synatax here
 * Offload that the expression engine
 */
EXPRESSION
  : F_EXPRESSION
  ;

TEXT
  : F_TEXT
  ; 

NEWLINE
  : F_NEW_LINE
  ;

/*
 * Normally we skip the whitespace and newline
 */
WS
  : F_WS+ -> skip
  ;



/*
 * START_TEMPLATE_MODE is the mode when first "-" is encoutered
 * At this point, we cann't decide weather it's the start of 
 *  - a CASE or DEFAULT statement (in which we should skip WS), or
 *  - a normal template string (in which we should keep WS)
 */
mode START_TEMPLATE_MODE;

S_WS
  : F_WS+ -> skip
  ;

CASE
  : 'CASE:' -> mode(DEFAULT_MODE)
  ;

DEFAULT
  : 'DEFAULT:' -> mode(DEFAULT_MODE)
  ;

// encounter anything other than CASE: and DEFAULT: will go to TEMPLATE_MODE
S_EXPRESSION
  : F_EXPRESSION -> type(EXPRESSION), mode(TEMPLATE_MODE)
  ;

TEMPLATE_REF
  : F_TEMPLATE_REF -> mode(TEMPLATE_MODE)
  ;

S_TEXT
  : F_TEXT -> type(TEXT), mode(TEMPLATE_MODE)
  ; 

S_OPEN_PARENTHESIS
  : F_OPEN_PARENTHESIS -> type(OPEN_PARENTHESIS), mode(TEMPLATE_MODE)
  ;

S_CLOSE_PARENTHESIS
  : F_CLOSE_PARENTHESIS -> type(CLOSE_PARENTHESIS), mode(TEMPLATE_MODE)
  ;

S_OPEN_CURLY
  : F_OPEN_CURLY -> type(OPEN_CURLY), mode(TEMPLATE_MODE);

S_CLOSE_CURLY
  : F_CLOSE_CURLY -> type(CLOSE_CURLY), mode(TEMPLATE_MODE);

S_OPEN_BRACKET
  : F_OPEN_BRACKET -> type(OPEN_BRACKET), mode(TEMPLATE_MODE);

S_CLOSE_BRACKET
  : F_CLOSE_BRACKET -> type(CLOSE_BRACKET), mode(TEMPLATE_MODE);


S_COMMA
  : F_COMMA -> type(COMMA), mode(TEMPLATE_MODE)
  ;


/*
 * TEMPLATE_MODE is the mode we are sure we are in a normal template string
 * WS is not skiped
 * This mode is ended when NEWLINE is encoutered
 */

mode TEMPLATE_MODE;

T_WS
  : F_WS+ -> type(WS)
  ;

T_EXPRESSION
  : F_EXPRESSION -> type(EXPRESSION)
  ;

T_TEMPLATE_REF
  : F_TEMPLATE_REF -> type(TEMPLATE_REF)
  ;

T_TEXT
  : F_TEXT -> type(TEXT)
  ;



T_OPEN_PARENTHESIS
  : F_OPEN_PARENTHESIS -> type(OPEN_PARENTHESIS)
  ;

T_CLOSE_PARENTHESIS
  : F_CLOSE_PARENTHESIS -> type(OPEN_PARENTHESIS)
  ;

T_OPEN_CURLY
  : F_OPEN_CURLY -> type(OPEN_CURLY)
  ;

T_CLOSE_CURLY
  : F_CLOSE_CURLY -> type(CLOSE_CURLY)
  ;


T_OPEN_BRACKET
  : F_OPEN_BRACKET -> type(OPEN_BRACKET)
  ;

T_CLOSE_BRACKET
  : F_CLOSE_BRACKET -> type(CLOSE_BRACKET)
  ;

T_COMMA
  : F_COMMA -> type(COMMA)
  ;

T_NEWLINE
  : F_NEW_LINE -> type(NEWLINE), mode(DEFAULT_MODE)
  ;


mode COMMENT_MODE;

// in moment mode, text rule is relaxed to cover all non-newline letters
C_TEXT
  : ~('\r'|'\n')+  -> skip
  ;

C_NEWLINE
  : F_NEW_LINE -> skip, mode(DEFAULT_MODE)
  ;