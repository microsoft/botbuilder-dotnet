lexer grammar LGFileLexer;

@lexer::members {
  bool inTemplate = false;
}

fragment WHITESPACE : ' '|'\t'|'\ufeff'|'\u00a0';

NEWLINE : '\r'? '\n';

OPTION : WHITESPACE* '>' WHITESPACE* '!#' ~('\r'|'\n')+ { inTemplate = false; };

COMMENT : WHITESPACE* '>' ~('\r'|'\n')* { !inTemplate }?;

IMPORT : WHITESPACE* '[' ~[\r\n[\]]*? ']' '(' ~[\r\n()]*? ')' WHITESPACE* { inTemplate = false;};

TEMPLATE_NAME_LINE : WHITESPACE* '#' ~('\r'|'\n')* { inTemplate = true; };

TEMPLATE_BODY_LINE : ~('\r'|'\n')+ { inTemplate }?;

INVALID_LINE :  ~('\r'|'\n')+ { !inTemplate }?;