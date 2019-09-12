parser grammar LGFileParser;

@parser::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.
@lexer::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.

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
	: HASH ((templateName parameters?) | errorTemplateName)
	;

errorTemplateName
    : (IDENTIFIER|TEXT_IN_NAME|OPEN_PARENTHESIS|COMMA|CLOSE_PARENTHESIS|DOT)*
    ;

templateName
    : IDENTIFIER (DOT IDENTIFIER)*
    ;

parameters
    : OPEN_PARENTHESIS? IDENTIFIER (COMMA IDENTIFIER)* CLOSE_PARENTHESIS?
    ;

templateBody
    : normalTemplateBody                        #normalBody
    | ifElseTemplateBody                        #ifElseBody
    | switchCaseTemplateBody                    #switchCaseBody
    ;

normalTemplateBody
    : (templateString newline)+
    ;

templateString
    : normalTemplateString
    | errorTemplateString
    ;

normalTemplateString
	: DASH (WS|TEXT|EXPRESSION|TEMPLATE_REF|TEXT_SEPARATOR|MULTI_LINE_TEXT|ESCAPE_CHARACTER)*
	;

errorTemplateString
	: INVALID_TOKEN_DEFAULT_MODE+
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