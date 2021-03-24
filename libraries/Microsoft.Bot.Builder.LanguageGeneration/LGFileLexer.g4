lexer grammar LGFileLexer;

@parser::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.
@lexer::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.

@lexer::members {
  bool startTemplate = false;
}

fragment WHITESPACE : ' '|'\t'|'\ufeff'|'\u00a0';

NEWLINE : '\r'? '\n';

OPTION : WHITESPACE* '>' WHITESPACE* '!#' ~('\r'|'\n')+ { !startTemplate }?;

COMMENT : WHITESPACE* '>' ~('\r'|'\n')* { !startTemplate }?;

IMPORT : WHITESPACE* '[' ~[\r\n[\]]*? ']' '(' ~[\r\n()]*? ')' ~('\r'|'\n')* { !startTemplate }?;

TEMPLATE_NAME_LINE : WHITESPACE* '#' ~('\r'|'\n')* { TokenStartColumn == 0}? { startTemplate = true; };

INLINE_MULTILINE: WHITESPACE* '-' WHITESPACE* '```' ~('\r'|'\n')* '```' WHITESPACE* { startTemplate && TokenStartColumn == 0 }?;

MULTILINE_PREFIX: WHITESPACE* '-' WHITESPACE* '```' ~('\r'|'\n')* { startTemplate && TokenStartColumn == 0 }? -> pushMode(MULTILINE_MODE);

TEMPLATE_BODY : ~('\r'|'\n')+ { startTemplate }?;

INVALID_LINE :  ~('\r'|'\n')+ { !startTemplate }?;


mode MULTILINE_MODE;
MULTILINE_SUFFIX : '```' -> popMode;

ESCAPE_CHARACTER : '\\' ~[\r\n]?;

MULTILINE_TEXT : .+?;