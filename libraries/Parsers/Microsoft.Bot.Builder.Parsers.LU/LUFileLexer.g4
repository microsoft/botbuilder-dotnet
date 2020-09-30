lexer grammar LUFileLexer;

// fragments
fragment A: 'a' | 'A';
fragment B: 'b' | 'B';
fragment C: 'c' | 'C';
fragment D: 'd' | 'D';
fragment E: 'e' | 'E';
fragment F: 'f' | 'F';
fragment G: 'g' | 'G';
fragment H: 'h' | 'H';
fragment I: 'i' | 'I';
fragment J: 'j' | 'J';
fragment K: 'k' | 'K';
fragment L: 'l' | 'L';
fragment M: 'm' | 'M';
fragment N: 'n' | 'N';
fragment O: 'o' | 'O';
fragment P: 'p' | 'P';
fragment Q: 'q' | 'Q';
fragment R: 'r' | 'R';
fragment S: 's' | 'S';
fragment T: 't' | 'T';
fragment U: 'u' | 'U';
fragment V: 'v' | 'V';
fragment W: 'w' | 'W';
fragment X: 'x' | 'X';
fragment Y: 'y' | 'Y';
fragment Z: 'z' | 'Z';

fragment LETTER: 'a'..'z' | 'A'..'Z';
fragment NUMBER: '0'..'9';

fragment WHITESPACE
  : ' '|'\t'|'\ufeff'|'\u00a0'
  ;

fragment UTTERANCE_MARK: '-' | '*' | '+';

WS
  : WHITESPACE+
  ;

NEWLINE
  : '\r'? '\n' -> skip
  ;

QNA_SOURCE_INFO
  : WS* '>' WS* '!# @qna.pair.source' WS* '=' ~('\r'|'\n')+
  ;

MODEL_INFO
  : WS* '>' WS* '!#' ~('\r'|'\n')+
  ;

COMMENT
  : WS* '>' ~('\r'|'\n')* -> skip
  ;

QNA
  : '#'+ WS* '?' -> pushMode(QNA_MODE)
  ;

HASH
  : '#' -> pushMode(INTENT_NAME_MODE)
  ;

DASH
  : UTTERANCE_MARK -> pushMode(LIST_BODY_MODE)
  ;

DOLLAR
  : '$' -> pushMode(ENTITY_MODE)
  ;

AT
  : '@' -> pushMode(NEW_ENTITY_MODE)
  ;

IMPORT
  : WS* '[' ~[\r\n[\]]*? ']' WS* ('(' ~[\r\n()]*? ')' | '[' ~[\r\n[\]]*? ']')
  ;

REFERENCE
  : WS* '[' ~[\r\n[\]]*? ']' WS* ':' WS* ~[\r\n]*
  ;

FILTER_MARK
  : '**' F I L T E R S ':**'
  ;

QNA_ID_MARK
  : '<a' .*? '</a>'
  ;

MULTI_LINE_TEXT
  : '```' .*? '```'
  ;
PROMPT_MARK
  : '**' P R O M P T S ':**'
  ;
  
INVALID_TOKEN_DEFAULT_MODE
  : .
  ;
  
mode NEW_ENTITY_MODE;

WS_IN_NEW_ENTITY
  : WS -> type(WS)
  ;

NEWLINE_IN_NEW_ENTITY
  : '\r'? '\n' -> type(NEWLINE), popMode
  ;

EQUAL
  : '='
  ;

COMMA
  : ','
  ;

HAS_ROLES_LABEL
  : H A S R O L E S?
  ;

HAS_FEATURES_LABEL
  : U S E S F E A T U R E S?
  ;

NEW_ENTITY_TYPE_IDENTIFIER
  : S I M P L E | L I S T | R E G E X | P R E B U I L T | C O M P O S I T E | M L | P A T T E R N A N Y | P H R A S E L I S T | I N T E N T
  ;

PHRASE_LIST_LABEL
  : '(' (~[\r\n])* ')'
  ;

NEW_COMPOSITE_ENTITY
  : '[' (~[\r\n{}[\]()])* ']'
  ;

NEW_REGEX_ENTITY
  : '/' (~[\r\n])*
  ;

NEW_ENTITY_IDENTIFIER
  : (~[ \t\r\n,;'"])+
  ;

NEW_ENTITY_IDENTIFIER_WITH_WS
  : ('\'' | '"') (~[\t\r\n,;'"])+ ('\'' | '"')
  ;

mode INTENT_NAME_MODE;
  
WS_IN_NAME
  : WS -> type(WS)
  ;

HASH_IN_NAME
  : '#' -> type(HASH)
  ;

NEWLINE_IN_NAME
  : '\r'? '\n' -> skip, popMode
  ;

IDENTIFIER
  : (LETTER | NUMBER | '_') (LETTER | NUMBER | '-' | '_')*
  ;

DOT
  : '.'
  ;

mode LIST_BODY_MODE;

WS_IN_LIST_BODY
  : WS -> type(WS)
  ;

NEWLINE_IN_LIST_BODY
  : '\r'? '\n' -> type(NEWLINE), popMode
  ;

ESCAPE_CHARACTER
  : '\\' ~[\r\n]?
  ;

EXPRESSION
  : '{' (~[\r\n{}] | ('{' ~[\r\n]* '}'))* '}'
  ;

TEXT
  : ~[ \t\r\n\\]+?
  ;

mode ENTITY_MODE;

WS_IN_ENTITY
  : WS -> type(WS)
  ;

NEWLINE_IN_ENTITY
  : '\r'? '\n' -> skip, popMode
  ;

COMPOSITE_ENTITY
  : '[' (~[\r\n{}[\]()])* ']'
  ;

REGEX_ENTITY
  : '/' (~[\r\n])*
  ;

ENTITY_TEXT
  : ~[ \t\r\n:]+
  ;

COLON_MARK
  : ':'
  ;

mode QNA_MODE;

NEWLINE_IN_QNA
  : '\r'? '\n' -> skip, popMode
  ;

QNA_TEXT
  : ~[\t\r\n]+
  ;