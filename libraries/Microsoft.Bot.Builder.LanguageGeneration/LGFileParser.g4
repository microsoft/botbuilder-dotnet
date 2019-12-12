parser grammar LGFileParser;

@parser::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.
@lexer::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.

options { tokenVocab=LGFileLexer; }

file
	: paragraph+? EOF
	;

paragraph
    : templateDefinition
    | importDefinition
    | EOF
    | errorTemplate
    ;

errorTemplate
    : INVALID_TOKEN+
    ;

templateDefinition
	: templateNameLine templateBody?
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
    : OPEN_PARENTHESIS (IDENTIFIER (COMMA IDENTIFIER)*)? CLOSE_PARENTHESIS
    ;

templateBody
    : normalTemplateBody                        #normalBody
    | ifElseTemplateBody                        #ifElseBody
    | switchCaseTemplateBody                    #switchCaseBody
    | structuredTemplateBody                    #structuredBody
    ;

structuredTemplateBody
    : structuredBodyNameLine structuredBodyContentLine? structuredBodyEndLine?
    ;

structuredBodyNameLine
    : LEFT_SQUARE_BRACKET STRUCTURED_CONTENT
    ;

structuredBodyContentLine
    : STRUCTURED_CONTENT+
    ;

structuredBodyEndLine
    : STRUCTURED_TEMPLATE_BODY_END
    ;

normalTemplateBody
    : templateString+
    ;

templateString
    : normalTemplateString
    | errorTemplateString
    ;

normalTemplateString
    : DASH MULTILINE_PREFIX? (TEXT|EXPRESSION|ESCAPE_CHARACTER)* MULTILINE_SUFFIX?
    ;

errorTemplateString
	: INVALID_TOKEN+
	;

ifElseTemplateBody
    : ifConditionRule+
    ;

ifConditionRule
    : ifCondition normalTemplateBody?
    ;

ifCondition
    : DASH (IF|ELSE|ELSEIF) (WS|TEXT|EXPRESSION)*
    ;

switchCaseTemplateBody
    : switchCaseRule+
    ;

switchCaseRule
    : switchCaseStat normalTemplateBody?
    ;

switchCaseStat
    : DASH (SWITCH|CASE|DEFAULT) (WS|TEXT|EXPRESSION)*
    ;

importDefinition
    : IMPORT
    ;