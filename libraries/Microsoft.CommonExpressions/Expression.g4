grammar Expression;

fragment LETTER : [a-zA-Z];
fragment DIGIT : [0-9];

ASTERISK : '*' ;
SLASH : '/' ;
PLUS : '+' ;
MINUS : '-' ;
EQUALS : '==';
NOTEQUALS: '!=';
BIGTHAN :'>';
SMALLTHAN: '<';

NUMBER : DIGIT + ( '.' DIGIT +)? ;

WHITESPACE : (' '|'\t') -> skip;


COMMENTS
  : ('>'|'$') ~('\r'|'\n')+ -> skip
  ;

IDENTIFIER
  : (LETTER | '_') (LETTER | DIGIT | '-' | '_')*
  ;

NEWLINE
  : '\r'? '\n' -> skip
  ;

COMMA
  : ','
  ;

DOT
  : '.'
  ;

STRING : '\'' (~'\'')* '\'';

expression:  IDENTIFIER DOT expression #dotExp
            |'(' expression ')' #parenthesisExp
		    | expression (ASTERISK|SLASH) expression #mulDivExp
			| expression (PLUS|MINUS) expression #addSubExp
            | expression (EQUALS|NOTEQUALS) expression #binaryLogicExp
			| IDENTIFIER '(' expression (COMMA expression)* ')' #functionExp
            | IDENTIFIER '[' expression ']' #bracketExp
			| NUMBER #numericAtomExp
			| STRING #stringExp
            | IDENTIFIER #idExp
			;