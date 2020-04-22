lexer grammar LGTemplateLexer;

@parser::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.
@lexer::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.

@lexer::members {
  bool ignoreWS = true; // usually we ignore whitespace, but inside template, whitespace is significant
  bool beginOfTemplateBody = true; // whether we are at the begining of template body
  bool inMultiline = false; // whether we are in multiline
  bool beginOfTemplateLine = false;// weather we are at the begining of template string
  bool inStructuredValue = false; // weather we are in the structure value
  bool beginOfStructureProperty = false; // weather we are at the begining of structure property
}

// fragments
fragment A: 'a' | 'A';
fragment C: 'c' | 'C';
fragment D: 'd' | 'D';
fragment E: 'e' | 'E';
fragment F: 'f' | 'F';
fragment H: 'h' | 'H';
fragment I: 'i' | 'I';
fragment L: 'l' | 'L';
fragment S: 's' | 'S';
fragment T: 't' | 'T';
fragment U: 'u' | 'U';
fragment W: 'w' | 'W';

fragment LETTER: 'a'..'z' | 'A'..'Z';

fragment NUMBER: '0'..'9';

fragment WHITESPACE : ' '|'\t'|'\ufeff'|'\u00a0';

fragment STRING_LITERAL : ('\'' (('\\'('\''|'\\'))|(~'\''))*? '\'') | ('"' (('\\'('"'|'\\'))|(~'"'))*? '"');

fragment STRING_INTERPOLATION : '`' (('\\'('`'|'\\'))|(~'`'))*? '`';

fragment ESCAPE_CHARACTER_FRAGMENT : '\\' ~[\r\n]?;

fragment IDENTIFIER : (LETTER | NUMBER | '_') (LETTER | NUMBER | '_')*;

WS
  : WHITESPACE+ -> skip
  ;

NEWLINE
  : '\r'? '\n' -> skip
  ;

COMMENTS
  : '>' ~('\r'|'\n')* -> skip
  ;

DASH
  : '-' { beginOfTemplateLine = true; beginOfTemplateBody = false; } -> pushMode(NORMAL_TEMPLATE_BODY_MODE)
  ;

OBJECT_DEFINITION
  : '{' ((WHITESPACE) | ((IDENTIFIER | STRING_LITERAL) ':' ( STRING_LITERAL | ~[{}\r\n'"`] | OBJECT_DEFINITION)+))* '}'
  ;
  
EXPRESSION_FRAGMENT
  : '$' '{' (STRING_LITERAL | STRING_INTERPOLATION | OBJECT_DEFINITION | ~[}'"`])+ '}'?
  ;

LEFT_SQUARE_BRACKET
  : '[' { beginOfTemplateBody }? {beginOfTemplateBody = false;} -> pushMode(STRUCTURE_NAME_MODE)
  ;

INVALID_TOKEN
  : . { beginOfTemplateBody = false; }
  ;

mode NORMAL_TEMPLATE_BODY_MODE;

WS_IN_BODY
  : WHITESPACE+  {ignoreWS}? -> skip
  ;

MULTILINE_PREFIX
  : '```' { !inMultiline  && beginOfTemplateLine }? { inMultiline = true; beginOfTemplateLine = false;}-> pushMode(MULTILINE_MODE)
  ;

NEWLINE_IN_BODY
  : '\r'? '\n' { ignoreWS = true;} -> skip, popMode
  ;

IF
  : I F WHITESPACE* ':'  {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

ELSEIF
  : E L S E WHITESPACE* I F WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

ELSE
  : E L S E WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

SWITCH
  : S W I T C H WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

CASE
  : C A S E WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

DEFAULT
  : D E F A U L T WHITESPACE* ':' {beginOfTemplateLine}? { ignoreWS = true; beginOfTemplateLine = false;}
  ;

ESCAPE_CHARACTER
  : ESCAPE_CHARACTER_FRAGMENT  { ignoreWS = false; beginOfTemplateLine = false;}
  ;

EXPRESSION
  : EXPRESSION_FRAGMENT  { ignoreWS = false; beginOfTemplateLine = false;}
  ;

TEXT
  : ~[\r\n]+?  { ignoreWS = false; beginOfTemplateLine = false;}
  ;

mode MULTILINE_MODE;

MULTILINE_SUFFIX
  : '```' { inMultiline = false; } -> popMode
  ;

MULTILINE_ESCAPE_CHARACTER
  : ESCAPE_CHARACTER_FRAGMENT -> type(ESCAPE_CHARACTER)
  ;

MULTILINE_EXPRESSION
  : EXPRESSION_FRAGMENT -> type(EXPRESSION)
  ;

MULTILINE_TEXT
  : (('\r'? '\n') | ~[\r\n])+? -> type(TEXT)
  ;

mode STRUCTURE_NAME_MODE;

WS_IN_STRUCTURE_NAME
  : WHITESPACE+ -> skip
  ;

NEWLINE_IN_STRUCTURE_NAME
  : '\r'? '\n' { ignoreWS = true;} {beginOfStructureProperty = true;}-> skip, pushMode(STRUCTURE_BODY_MODE)
  ;

STRUCTURE_NAME
  : (LETTER | NUMBER | '_') (LETTER | NUMBER | '-' | '_' | '.')*
  ;

TEXT_IN_STRUCTURE_NAME
  : ~[\r\n]+?
  ;

mode STRUCTURE_BODY_MODE;

STRUCTURED_COMMENTS
  : '>' ~[\r\n]* '\r'?'\n' { !inStructuredValue && beginOfStructureProperty}? -> skip
  ;

WS_IN_STRUCTURE_BODY
  : WHITESPACE+ {ignoreWS}? -> skip
  ;

STRUCTURED_NEWLINE
  : '\r'? '\n' { ignoreWS = true; inStructuredValue = false; beginOfStructureProperty = true;}
  ;

STRUCTURED_BODY_END
  : ']' {!inStructuredValue}? -> popMode, popMode
  ;

STRUCTURE_IDENTIFIER
  : (LETTER | NUMBER | '_') (LETTER | NUMBER | '-' | '_' | '.')* { !inStructuredValue && beginOfStructureProperty}? {beginOfStructureProperty = false;}
  ;

STRUCTURE_EQUALS
  : '=' {!inStructuredValue}? {inStructuredValue = true;} 
  ;

STRUCTURE_OR_MARK
  : '|' { ignoreWS = true; }
  ;

ESCAPE_CHARACTER_IN_STRUCTURE_BODY
  : ESCAPE_CHARACTER_FRAGMENT { ignoreWS = false; }
  ;

EXPRESSION_IN_STRUCTURE_BODY
  : EXPRESSION_FRAGMENT { ignoreWS = false; }
  ;

TEXT_IN_STRUCTURE_BODY
  : ~[\r\n]+?  { ignoreWS = false; beginOfStructureProperty = false;}
  ;

