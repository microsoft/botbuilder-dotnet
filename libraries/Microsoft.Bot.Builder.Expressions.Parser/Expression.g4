grammar Expression;

expression
    : ('!'|'-'|'+') expression                            #unaryOpExp
    | <assoc=right> expression '^' expression   #binaryOpExp 
    | expression ('*'|'/'|'%') expression       #binaryOpExp
    | expression ('+'|'-') expression           #binaryOpExp
    | expression ('=='|'!='|'<>') expression    #binaryOpExp
    | expression ('&') expression               #binaryOpExp
    | expression ('<'|'<='|'>'|'>=') expression #binaryOpExp
    | expression '&&' expression                #binaryOpExp
    | expression '||' expression                #binaryOpExp
    | primaryExpression                         #primaryExp
    ;
 
primaryExpression 
    : '(' expression ')'                      #parenthesisExp
    | NUMBER                                  #numericAtom
    | STRING                                  #stringAtom
    | IDENTIFIER                              #idAtom
    | primaryExpression '.' IDENTIFIER        #memberAccessExp
    | primaryExpression '(' argsList? ')'     #funcInvokeExp
    | primaryExpression '[' expression ']'    #indexAccessExp
    ;

argsList
    : expression (',' expression)*
    ;

fragment LETTER : [a-zA-Z];
fragment DIGIT : [0-9];

NUMBER : DIGIT + ( '.' DIGIT +)? ;

WHITESPACE : (' '|'\t'|'\u00a0'|'\ufeff') -> skip;

IDENTIFIER : (LETTER | '_' | '#' | '@' | '$') (LETTER | DIGIT | '-' | '_')*;

NEWLINE : '\r'? '\n' -> skip;

STRING : ('\'' (~'\'')* '\'') | ('"' (~'"')* '"');