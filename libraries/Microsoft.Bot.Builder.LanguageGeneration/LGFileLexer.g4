lexer grammar LGFileLexer;

@lexer::members {
  bool startTemplate = false;
}

fragment WHITESPACE : ' '|'\t'|'\ufeff'|'\u00a0';

NEWLINE : '\r'? '\n';

OPTION : WHITESPACE* '>' WHITESPACE* '!#' ~('\r'|'\n')+ { !startTemplate }?;

COMMENT : WHITESPACE* '>' ~('\r'|'\n')* { !startTemplate }?;

IMPORT : WHITESPACE* '[' ~[\r\n[\]]*? ']' '(' ~[\r\n()]*? ')' WHITESPACE* { !startTemplate }?;

TEMPLATE_NAME_LINE : WHITESPACE* '#' ~('\r'|'\n')* { startTemplate = true; };

TEMPLATE_BODY_LINE : ~('\r'|'\n')+ { startTemplate }?;

INVALID_LINE :  ~('\r'|'\n')+ { !startTemplate }?;