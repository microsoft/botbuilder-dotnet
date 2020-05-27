lexer grammar LGFileLexer;

@parser::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.
@lexer::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.

@lexer::members {
  bool startTemplate = false;
  bool startLine = true;
}

fragment WHITESPACE : ' '|'\t'|'\ufeff'|'\u00a0';

NEWLINE : '\r'? '\n' {startLine = true;};

OPTION : WHITESPACE* '>' WHITESPACE* '!#' ~('\r'|'\n')+ { !startTemplate }?;

COMMENT : WHITESPACE* '>' ~('\r'|'\n')* { !startTemplate }?;

IMPORT : WHITESPACE* '[' ~[\r\n[\]]*? ']' '(' ~[\r\n()]*? ')' WHITESPACE* { !startTemplate }?;

TEMPLATE_NAME_LINE : WHITESPACE* '#' ~('\r'|'\n')* { startLine }? { startTemplate = true; };

MULTILINE_PREFIX: WHITESPACE* '-' WHITESPACE* '```' { startTemplate && startLine }? -> pushMode(MULTILINE_MODE);

TEMPLATE_BODY : ~('\r'|'\n') { startTemplate }? { startLine = false; };

INVALID_LINE :  ~('\r'|'\n')+ { !startTemplate }?;


mode MULTILINE_MODE;
MULTILINE_SUFFIX : '```' -> popMode;

ESCAPE_CHARACTER : '\\' ~[\r\n]?;

MULTILINE_TEXT : .+?;