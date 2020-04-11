parser grammar LGFileParser;

options { tokenVocab=LGFileLexer; }

file
    : (paragraph? NEWLINE)* paragraph EOF
    ;

paragraph
    : templateDefinition
    | importDefinition
    | optionDefinition
    | errorDefinition
    | commentDefinition
    | NEWLINE
    | EOF
    ;

commentDefinition
    : COMMENT
    ;

importDefinition
    : IMPORT
    ;

optionDefinition
    : OPTION
    ;

errorDefinition
    : INVALID_LINE
    ;

templateDefinition
    : templateNameLine templateBody
    ;

templateBody
    : templateBodyLine*
    ;

templateNameLine
    : TEMPLATE_NAME_LINE NEWLINE?
    ;

templateBodyLine
    : (TEMPLATE_BODY_LINE NEWLINE?) | NEWLINE
    ;

