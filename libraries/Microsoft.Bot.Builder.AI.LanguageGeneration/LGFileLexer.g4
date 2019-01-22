lexer grammar LGFileLexer;

// usually we don't define pattern of a complete TOKEN as fragment, since fragment don't generate token directly
// the reason we do it here is to share pattern across multiple modes
fragment F_EXPRESSION: '{' ~('\r'|'\n'|'{'|'}')* '}';
fragment F_TEMPLATE_REF: '[' ~('\r'|'\n'|'['|']')* ']';
fragment F_TEXT: ~('\r'|'\n'|' '|'\t')+;
fragment F_NEW_LINE: '\r'?'\n';
fragment F_WS: ' '|'\t';


// in comment mode, eveyting is skip until newline is encoutner
// we just treat $... as comment here before we handle type notation in parser
COMMENT_START
  : ('>' | '$' ) -> skip, mode(COMMENT_MODE)
  ;

HASH
  : '#'
  ;


DASH
  : '-' -> mode(START_TEMPLATE_MODE)
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

S_EXPRESSION
  : F_EXPRESSION -> type(EXPRESSION), mode(TEMPLATE_MODE)
  ;


S_TEXT
  : F_TEXT -> type(TEXT), mode(TEMPLATE_MODE)
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

TEMPLATE_REF
  : F_TEMPLATE_REF
  ;

T_TEXT
  : F_TEXT -> type(TEXT)
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