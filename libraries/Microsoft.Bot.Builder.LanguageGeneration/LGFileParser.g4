parser grammar LGFileParser;

options { tokenVocab=LGFileLexer; }

file
	: paragraph+? EOF
	;

paragraph
    : newline
    | templateDefinition
    | importDefinition
    ;

// Treat EOF as newline to hanle file end gracefully
// It's possible that parser doesn't even have to handle NEWLINE, 
// but before the syntax is finalized, we still keep the NEWLINE in grammer 
newline
    : NEWLINE
    | EOF
    ;

templateDefinition
	: templateNameLine newline templateBody?
	;

templateNameLine
	: HASH templateName parameters?
	;

templateName
    : IDENTIFIER (DOT IDENTIFIER)*
    ;

parameters
    : OPEN_PARENTHESIS? IDENTIFIER ((COMMA|INVALID_SEPERATE_CHAR) IDENTIFIER)* CLOSE_PARENTHESIS?
    ;

templateBody
    : normalTemplateBody                        #normalBody
    | ifElseTemplateBody                        #ifElseBody
    | switchCaseTemplateBody                    #switchCaseBody
    ;

normalTemplateBody
    : (normalTemplateString newline)+
    ;

normalTemplateString
	: DASH (WS|TEXT|EXPRESSION|TEMPLATE_REF|TEXT_SEPARATOR|MULTI_LINE_TEXT|ESCAPE_CHARACTER)*
	;

ifElseTemplateBody
    : ifConditionRule+
    ;

ifConditionRule
    : ifCondition newline normalTemplateBody?
    ;

ifCondition
    : DASH (IF|ELSE|ELSEIF) (WS|TEXT|EXPRESSION)*
    ;

    switchCaseTemplateBody
    : switchCaseRule+
    ;

switchCaseRule
    : switchCaseStat newline normalTemplateBody?
    ;

switchCaseStat
    : DASH (SWITCH|CASE|DEFAULT) (WS|TEXT|EXPRESSION)*
    ;

importDefinition
    : IMPORT_DESC IMPORT_PATH
    ;